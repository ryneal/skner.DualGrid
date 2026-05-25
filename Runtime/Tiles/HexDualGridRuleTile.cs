using UnityEngine;
using UnityEngine.Tilemaps;
using skner.DualGrid.Utils;

namespace skner.DualGrid
{
    /// <summary>
    /// A tile that renders one of 8 sprite variants per triangle kind, indexed
    /// by a 3-bit neighbor mask. Used by <see cref="HexDualGridTilemapModule"/>
    /// to render the up- and down-triangle vertex lattice of a hex data grid.
    /// </summary>
    /// <remarks>
    /// The variant is resolved in <see cref="GetTileData"/> by walking up to
    /// the owning module via the render tilemap's parent. The render tilemap's
    /// GameObject name tells us which triangle kind it represents.
    /// </remarks>
    [CreateAssetMenu(fileName = "New Hex Dual Grid Rule Tile",
        menuName = "2D/Tiles/Hex Dual Grid Rule Tile")]
    public class HexDualGridRuleTile : TileBase
    {
        [SerializeField] private Sprite[] _upTriangleSprites = new Sprite[8];
        [SerializeField] private Sprite[] _downTriangleSprites = new Sprite[8];
        [SerializeField] private Tile.ColliderType[] _upColliderTypes = new Tile.ColliderType[8];
        [SerializeField] private Tile.ColliderType[] _downColliderTypes = new Tile.ColliderType[8];
        [SerializeField] private GameObject[] _upGameObjects = new GameObject[8];
        [SerializeField] private GameObject[] _downGameObjects = new GameObject[8];

        public Sprite[] UpTriangleSprites => _upTriangleSprites;
        public Sprite[] DownTriangleSprites => _downTriangleSprites;
        public Tile.ColliderType[] UpColliderTypes => _upColliderTypes;
        public Tile.ColliderType[] DownColliderTypes => _downColliderTypes;
        public GameObject[] UpGameObjects => _upGameObjects;
        public GameObject[] DownGameObjects => _downGameObjects;

        public Sprite GetSprite(TriangleKind kind, int mask)
        {
            if (mask < 0 || mask > 7) return null;
            return (kind == TriangleKind.Up ? _upTriangleSprites : _downTriangleSprites)[mask];
        }

        public Tile.ColliderType GetColliderType(TriangleKind kind, int mask)
        {
            if (mask < 0 || mask > 7) return Tile.ColliderType.None;
            return (kind == TriangleKind.Up ? _upColliderTypes : _downColliderTypes)[mask];
        }

        public GameObject GetGameObject(TriangleKind kind, int mask)
        {
            if (mask < 0 || mask > 7) return null;
            return (kind == TriangleKind.Up ? _upGameObjects : _downGameObjects)[mask];
        }

        public override void GetTileData(Vector3Int position, ITilemap itilemap, ref TileData tileData)
        {
            var tilemapComponent = itilemap.GetComponent<Tilemap>();
            if (tilemapComponent == null) return;

            var go = tilemapComponent.gameObject;
            var module = go.transform.parent != null
                ? go.transform.parent.GetComponent<HexDualGridTilemapModule>()
                : null;
            if (module == null) return;

            TriangleKind kind = go.name == HexDualGridTilemapModule.UpRenderTilemapName
                ? TriangleKind.Up
                : TriangleKind.Down;

            var neighbors = HexDualGridUtils.GetDataNeighborsForRenderCell(
                position, kind, module.Orientation);
            int mask = HexDualGridUtils.EncodeNeighborMask(
                module.DataTilemap.HasTile(neighbors[0]),
                module.DataTilemap.HasTile(neighbors[1]),
                module.DataTilemap.HasTile(neighbors[2]));

            tileData.sprite = GetSprite(kind, mask);
            tileData.colliderType = GetColliderType(kind, mask);
            tileData.gameObject = GetGameObject(kind, mask);
            tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
        }
    }
}
