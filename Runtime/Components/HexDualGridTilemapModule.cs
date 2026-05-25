using UnityEngine;
using UnityEngine.Tilemaps;
using skner.DualGrid.Utils;

namespace skner.DualGrid
{
    /// <summary>
    /// Drives two hex render tilemaps (up- and down-triangle vertex lattices)
    /// from a single hex data tilemap. The hex analogue of
    /// <see cref="DualGridTilemapModule"/>.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    [DisallowMultipleComponent]
    public class HexDualGridTilemapModule : MonoBehaviour
    {
        public const string UpRenderTilemapName = "RenderTilemap_Up";
        public const string DownRenderTilemapName = "RenderTilemap_Down";

        [SerializeField] private HexDualGridRuleTile _ruleTile;
        public HexDualGridRuleTile RuleTile { get => _ruleTile; set => _ruleTile = value; }

        [SerializeField] private HexOrientation _orientation = HexOrientation.PointyTop;
        public HexOrientation Orientation { get => _orientation; set => _orientation = value; }

        [SerializeField] private DualGridDataTile _dataTile;
        public DualGridDataTile DataTile { get => _dataTile; set => _dataTile = value; }

        private Tilemap _dataTilemap;
        public Tilemap DataTilemap
        {
            get
            {
                if (_dataTilemap == null) _dataTilemap = GetComponent<Tilemap>();
                return _dataTilemap;
            }
        }

        private Tilemap _upRender;
        public Tilemap UpRenderTilemap
        {
            get
            {
                if (_upRender == null) _upRender = FindChildTilemap(UpRenderTilemapName);
                return _upRender;
            }
        }

        private Tilemap _downRender;
        public Tilemap DownRenderTilemap
        {
            get
            {
                if (_downRender == null) _downRender = FindChildTilemap(DownRenderTilemapName);
                return _downRender;
            }
        }

        private Tilemap FindChildTilemap(string name)
        {
            var child = transform.Find(name);
            return child == null ? null : child.GetComponent<Tilemap>();
        }

#if UNITY_EDITOR || UNITY_2022_1_OR_NEWER
        private void OnEnable()
        {
            Tilemap.tilemapTileChanged += HandleTilemapChange;
        }

        private void OnDisable()
        {
            Tilemap.tilemapTileChanged -= HandleTilemapChange;
        }

        internal void HandleTilemapChange(Tilemap tilemap, Tilemap.SyncTile[] tileChanges)
        {
            if (tilemap != DataTilemap) return;
            if (_ruleTile == null)
            {
                Debug.LogError("HexDualGridTilemapModule: rule tile not assigned.", this);
                return;
            }
            foreach (var change in tileChanges) RefreshRenderTiles(change.position);
        }
#endif

        public void SetDataTile(Vector3Int position)
        {
            DataTilemap.SetTile(position, _dataTile);
            RefreshRenderTiles(position);
        }

        public void SetDataTiles(BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin) SetDataTile(position);
        }

        public void ClearDataTile(Vector3Int position)
        {
            DataTilemap.SetTile(position, null);
            RefreshRenderTiles(position);
        }

        public void ClearDataTiles(BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin) ClearDataTile(position);
        }

        public virtual void RefreshRenderTilemap()
        {
            if (_ruleTile == null)
            {
                Debug.LogError("HexDualGridTilemapModule: rule tile not assigned.", this);
                return;
            }
            UpRenderTilemap.ClearAllTiles();
            DownRenderTilemap.ClearAllTiles();
            foreach (var p in DataTilemap.cellBounds.allPositionsWithin)
            {
                if (DataTilemap.HasTile(p)) RefreshRenderTiles(p);
            }
        }

        public virtual void RefreshRenderTiles(Vector3Int dataCell)
        {
            var set = HexDualGridUtils.GetRenderCellsForDataCell(dataCell, _orientation);
            foreach (var rc in set.UpCells) RefreshOne(UpRenderTilemap, rc, TriangleKind.Up);
            foreach (var rc in set.DownCells) RefreshOne(DownRenderTilemap, rc, TriangleKind.Down);
        }

        private void RefreshOne(Tilemap renderMap, Vector3Int renderCell, TriangleKind kind)
        {
            if (renderMap == null) return;

            var neighbors = HexDualGridUtils.GetDataNeighborsForRenderCell(renderCell, kind, _orientation);
            int mask = HexDualGridUtils.EncodeNeighborMask(
                DataTilemap.HasTile(neighbors[0]),
                DataTilemap.HasTile(neighbors[1]),
                DataTilemap.HasTile(neighbors[2]));

            if (mask == 0)
            {
                if (renderMap.HasTile(renderCell)) renderMap.SetTile(renderCell, null);
            }
            else if (!renderMap.HasTile(renderCell))
            {
                renderMap.SetTile(renderCell, _ruleTile);
            }
            else
            {
                renderMap.RefreshTile(renderCell);
            }
        }
    }
}
