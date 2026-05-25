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

        // ---------------------------------------------------------------------
        // Geometry derivation (pointy-top)
        //
        // We work internally in axial coordinates because hex vertex math is
        // parity-free there. We convert to and from Unity's offset coordinates
        // (odd-r for pointy-top, odd-q for flat-top) at the API boundary.
        //
        // Axial convention for pointy-top: +q = east, +r = north.
        // Hex (q, r) has 6 vertices: N, NE, SE, S, SW, NW.
        // Each vertex is shared by 3 hexes; the 3 form either an up-pointing
        // triangle (apex north) or a down-pointing triangle (apex south).
        //
        // Up triangles (centroids) at vertices: NE, S, NW.
        // Down triangles (centroids) at vertices: N, SE, SW.
        //
        // We define render cell coordinates so that:
        //   UpRender(q, r)   = the up triangle with data neighbors
        //                      (q, r), (q, r+1), (q+1, r)
        //                      -- this is the NE vertex of data (q, r).
        //   DownRender(q, r) = the down triangle with data neighbors
        //                      (q, r), (q+1, r), (q+1, r-1)
        //                      -- this is the SE vertex of data (q, r).
        //
        // Under this convention, a data cell (a, b) contributes to:
        //   UP   render cells: (a, b), (a, b-1), (a-1, b)
        //   DOWN render cells: (a, b), (a-1, b+1), (a-1, b)
        // (derived by listing each of its 3 up- and 3 down-vertices and
        // re-indexing each as the NE/SE vertex of one of the 3 sharing hexes).
        //
        // The canonical neighbor ordering returned by
        // GetDataNeighborsForRenderCell is lowest-y, then clockwise:
        //   Up   (q, r): [(q, r), (q, r+1), (q+1, r)]   -- BL, Apex, BR
        //   Down (q, r): [(q+1, r-1), (q, r), (q+1, r)] -- Apex, TL, TR
        // ---------------------------------------------------------------------

        internal static Vector3Int OffsetToAxialPointy(Vector3Int offset)
        {
            int q = offset.x - ((offset.y - (offset.y & 1)) / 2);
            return new Vector3Int(q, offset.y, 0);
        }

        internal static Vector3Int AxialToOffsetPointy(Vector3Int axial)
        {
            int x = axial.x + ((axial.y - (axial.y & 1)) / 2);
            return new Vector3Int(x, axial.y, 0);
        }

        public static RenderCellSet GetRenderCellsForDataCell(Vector3Int dataCell, HexOrientation orientation)
        {
            if (orientation == HexOrientation.PointyTop)
                return GetRenderCellsForDataCell_PointyTop(dataCell);
            return GetRenderCellsForDataCell_FlatTop(dataCell);
        }

        private static RenderCellSet GetRenderCellsForDataCell_PointyTop(Vector3Int dataCell)
        {
            Vector3Int a = OffsetToAxialPointy(dataCell);

            Vector3Int[] upAxial =
            {
                new Vector3Int(a.x,     a.y,     0),
                new Vector3Int(a.x,     a.y - 1, 0),
                new Vector3Int(a.x - 1, a.y,     0),
            };
            Vector3Int[] downAxial =
            {
                new Vector3Int(a.x,     a.y,     0),
                new Vector3Int(a.x - 1, a.y + 1, 0),
                new Vector3Int(a.x - 1, a.y,     0),
            };

            var up = new Vector3Int[3];
            var down = new Vector3Int[3];
            for (int i = 0; i < 3; i++)
            {
                up[i] = AxialToOffsetPointy(upAxial[i]);
                down[i] = AxialToOffsetPointy(downAxial[i]);
            }
            return new RenderCellSet(up, down);
        }

        // ---------------------------------------------------------------------
        // Geometry derivation (flat-top)
        //
        // Axial convention for flat-top: +q = east, +r = "slanted north" so that
        // axial (q, r+1) is the due-N neighbor and axial (q+1, r) is the NE
        // neighbor. Vertices of a flat-top hex are at TR, R, BR, BL, L, TL.
        // Render-cell assignments (the BR vertex defines the up-render origin,
        // the BL vertex defines the down-render origin):
        //   UpRender(q, r)   = up triangle with neighbors (q, r), (q, r-1), (q+1, r-1)
        //   DownRender(q, r) = down triangle with neighbors (q, r), (q, r-1), (q-1, r)
        //
        // Under this convention, a data cell (a, b) contributes to:
        //   UP   render cells: (a, b), (a, b+1), (a-1, b+1)
        //   DOWN render cells: (a, b), (a+1, b), (a, b+1)
        // ---------------------------------------------------------------------

        internal static Vector3Int OffsetToAxialFlat(Vector3Int offset)
        {
            int r = offset.y - ((offset.x - (offset.x & 1)) / 2);
            return new Vector3Int(offset.x, r, 0);
        }

        internal static Vector3Int AxialToOffsetFlat(Vector3Int axial)
        {
            int y = axial.y + ((axial.x - (axial.x & 1)) / 2);
            return new Vector3Int(axial.x, y, 0);
        }

        private static RenderCellSet GetRenderCellsForDataCell_FlatTop(Vector3Int dataCell)
        {
            Vector3Int a = OffsetToAxialFlat(dataCell);

            Vector3Int[] upAxial =
            {
                new Vector3Int(a.x,     a.y,     0),
                new Vector3Int(a.x,     a.y + 1, 0),
                new Vector3Int(a.x - 1, a.y + 1, 0),
            };
            Vector3Int[] downAxial =
            {
                new Vector3Int(a.x,     a.y,     0),
                new Vector3Int(a.x + 1, a.y,     0),
                new Vector3Int(a.x,     a.y + 1, 0),
            };

            var up = new Vector3Int[3];
            var down = new Vector3Int[3];
            for (int i = 0; i < 3; i++)
            {
                up[i] = AxialToOffsetFlat(upAxial[i]);
                down[i] = AxialToOffsetFlat(downAxial[i]);
            }
            return new RenderCellSet(up, down);
        }

        // ---------------------------------------------------------------------
        // Reverse mapping: which 3 data cells determine the variant on a given
        // render cell. Canonical order is lowest-y world position, then
        // clockwise. Derivation lives next to the forward mapping above.
        //
        // Pointy-top:
        //   Up   (q, r) axial: [(q, r), (q, r+1), (q+1, r)]
        //   Down (q, r) axial: [(q+1, r-1), (q, r), (q+1, r)]
        // Flat-top:
        //   Up   (q, r) axial: [(q, r-1), (q, r), (q+1, r-1)]
        //   Down (q, r) axial: [(q, r-1), (q-1, r), (q, r)]
        // ---------------------------------------------------------------------

        public static Vector3Int[] GetDataNeighborsForRenderCell(
            Vector3Int renderCell, TriangleKind kind, HexOrientation orientation)
        {
            if (orientation == HexOrientation.PointyTop)
            {
                Vector3Int a = OffsetToAxialPointy(renderCell);
                Vector3Int[] axial = kind == TriangleKind.Up
                    ? new[]
                    {
                        new Vector3Int(a.x,     a.y,     0),
                        new Vector3Int(a.x,     a.y + 1, 0),
                        new Vector3Int(a.x + 1, a.y,     0),
                    }
                    : new[]
                    {
                        new Vector3Int(a.x + 1, a.y - 1, 0),
                        new Vector3Int(a.x,     a.y,     0),
                        new Vector3Int(a.x + 1, a.y,     0),
                    };
                var result = new Vector3Int[3];
                for (int i = 0; i < 3; i++) result[i] = AxialToOffsetPointy(axial[i]);
                return result;
            }
            else
            {
                Vector3Int a = OffsetToAxialFlat(renderCell);
                Vector3Int[] axial = kind == TriangleKind.Up
                    ? new[]
                    {
                        new Vector3Int(a.x,     a.y - 1, 0),
                        new Vector3Int(a.x,     a.y,     0),
                        new Vector3Int(a.x + 1, a.y - 1, 0),
                    }
                    : new[]
                    {
                        new Vector3Int(a.x,     a.y - 1, 0),
                        new Vector3Int(a.x - 1, a.y,     0),
                        new Vector3Int(a.x,     a.y,     0),
                    };
                var result = new Vector3Int[3];
                for (int i = 0; i < 3; i++) result[i] = AxialToOffsetFlat(axial[i]);
                return result;
            }
        }
    }
}
