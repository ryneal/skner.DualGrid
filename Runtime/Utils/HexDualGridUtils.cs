using UnityEngine;

namespace skner.DualGrid.Utils
{
    public enum HexOrientation { PointyTop, FlatTop }

    public enum TriangleKind { Up, Down }

    public readonly struct RenderCellSet
    {
        public readonly Vector3Int[] UpCells;
        public readonly Vector3Int[] DownCells;

        public RenderCellSet(Vector3Int[] upCells, Vector3Int[] downCells)
        {
            UpCells = upCells;
            DownCells = downCells;
        }
    }

    /// <summary>
    /// Pure-function helpers for the hex dual-grid system.
    /// </summary>
    /// <remarks>
    /// All coordinates are Unity tilemap cell positions (offset coordinates).
    /// Pointy-top uses odd-row offset; flat-top uses odd-column offset, matching Unity's hex tilemap defaults.
    /// </remarks>
    public static class HexDualGridUtils
    {
        /// <summary>
        /// Packs three neighbor-filled bools into a 0..7 mask used to index a <see cref="HexDualGridRuleTile"/> variant.
        /// </summary>
        public static int EncodeNeighborMask(bool a, bool b, bool c)
        {
            int mask = 0;
            if (a) mask |= 1 << 0;
            if (b) mask |= 1 << 1;
            if (c) mask |= 1 << 2;
            return mask;
        }
    }
}
