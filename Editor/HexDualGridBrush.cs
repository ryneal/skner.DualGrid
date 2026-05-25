using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    /// <summary>
    /// A palette brush for <see cref="HexDualGridTilemapModule"/>.
    /// Painting writes into the data tilemap and the module updates the
    /// two render tilemaps automatically.
    /// </summary>
    /// <remarks>
    /// Editor preview overlays (the hover-time preview of what a stroke will
    /// paint) are not yet implemented for hex. The rect brush has its own
    /// preview tile machinery; adding the equivalent for the up/down render
    /// pair can come later. Painting/erasing/box/flood-fill all work without
    /// preview.
    /// </remarks>
    [CustomGridBrush(false, true, true, "Hex Dual Grid Brush")]
    public class HexDualGridBrush : GridBrush
    {
        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
        {
            if (brushTarget != null && brushTarget.TryGetComponent(out HexDualGridTilemapModule module))
            {
                SetTilesInBounds(module, module.DataTile, bounds);
            }
            else
            {
                base.BoxFill(gridLayout, brushTarget, bounds);
            }
        }

        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
        {
            if (brushTarget != null && brushTarget.TryGetComponent(out HexDualGridTilemapModule module))
            {
                SetTilesInBounds(module, null, bounds);
            }
            else
            {
                base.BoxErase(gridLayout, brushTarget, bounds);
            }
        }

        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (brushTarget != null && brushTarget.TryGetComponent(out HexDualGridTilemapModule module))
            {
                module.DataTilemap.FloodFill(position, module.DataTile);
                module.RefreshRenderTilemap();
            }
            else
            {
                base.FloodFill(gridLayout, brushTarget, position);
            }
        }

        private static void SetTilesInBounds(HexDualGridTilemapModule module, DualGridDataTile dataTile, BoundsInt bounds)
        {
            var changes = new List<TileChangeData>();
            foreach (var position in bounds.allPositionsWithin)
            {
                changes.Add(new TileChangeData { position = position, tile = dataTile });
            }
            module.DataTilemap.SetTiles(changes.ToArray(), ignoreLockFlags: false);
            foreach (var position in bounds.allPositionsWithin)
            {
                module.RefreshRenderTiles(position);
                module.DataTilemap.RefreshTile(position);
            }
        }
    }
}
