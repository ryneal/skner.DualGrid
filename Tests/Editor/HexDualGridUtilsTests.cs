using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using skner.DualGrid.Utils;

namespace skner.DualGrid.Tests
{
    public class HexDualGridUtilsTests
    {
        [Test]
        public void EncodeNeighborMask_AllFalse_Returns0()
        {
            Assert.AreEqual(0, HexDualGridUtils.EncodeNeighborMask(false, false, false));
        }

        [Test]
        public void EncodeNeighborMask_AllTrue_Returns7()
        {
            Assert.AreEqual(7, HexDualGridUtils.EncodeNeighborMask(true, true, true));
        }

        [Test]
        public void EncodeNeighborMask_OnlyFirst_Returns1()
        {
            Assert.AreEqual(1, HexDualGridUtils.EncodeNeighborMask(true, false, false));
        }

        [Test]
        public void EncodeNeighborMask_OnlySecond_Returns2()
        {
            Assert.AreEqual(2, HexDualGridUtils.EncodeNeighborMask(false, true, false));
        }

        [Test]
        public void EncodeNeighborMask_OnlyThird_Returns4()
        {
            Assert.AreEqual(4, HexDualGridUtils.EncodeNeighborMask(false, false, true));
        }

        // ----- Pointy-top render cell mapping ---------------------------------

        // Fixtures derived from the geometric analysis documented in
        // HexDualGridUtils.cs. (0, 0) is an even-row cell; (1, 1) is an
        // odd-row cell — together they exercise the odd-r offset toggle.

        [Test]
        public void GetRenderCellsForDataCell_PointyTop_EvenRow()
        {
            var result = HexDualGridUtils.GetRenderCellsForDataCell(
                new Vector3Int(0, 0, 0), HexOrientation.PointyTop);

            Assert.AreEqual(3, result.UpCells.Length);
            Assert.AreEqual(3, result.DownCells.Length);

            CollectionAssert.AreEquivalent(
                new[] {
                    new Vector3Int(0, 0, 0),
                    new Vector3Int(-1, -1, 0),
                    new Vector3Int(-1, 0, 0),
                },
                result.UpCells);
            CollectionAssert.AreEquivalent(
                new[] {
                    new Vector3Int(0, 0, 0),
                    new Vector3Int(-1, 1, 0),
                    new Vector3Int(-1, 0, 0),
                },
                result.DownCells);
        }

        [Test]
        public void GetRenderCellsForDataCell_PointyTop_OddRow()
        {
            var result = HexDualGridUtils.GetRenderCellsForDataCell(
                new Vector3Int(1, 1, 0), HexOrientation.PointyTop);

            CollectionAssert.AreEquivalent(
                new[] {
                    new Vector3Int(1, 1, 0),
                    new Vector3Int(1, 0, 0),
                    new Vector3Int(0, 1, 0),
                },
                result.UpCells);
            CollectionAssert.AreEquivalent(
                new[] {
                    new Vector3Int(1, 1, 0),
                    new Vector3Int(1, 2, 0),
                    new Vector3Int(0, 1, 0),
                },
                result.DownCells);
        }

        [Test]
        public void GetRenderCellsForDataCell_PointyTop_AdjacentDataCells_ShareTwoRenderCells()
        {
            // (0,0) and (1,0) are east neighbors on an even row. As hex
            // neighbors they share 2 vertices (the 2 endpoints of the shared
            // edge), one of which is an up-triangle centroid and the other a
            // down-triangle centroid -- so the intersection across both
            // render tilemaps has cardinality 2.
            var a = HexDualGridUtils.GetRenderCellsForDataCell(
                new Vector3Int(0, 0, 0), HexOrientation.PointyTop);
            var b = HexDualGridUtils.GetRenderCellsForDataCell(
                new Vector3Int(1, 0, 0), HexOrientation.PointyTop);

            int sharedUp = SharedCount(a.UpCells, b.UpCells);
            int sharedDown = SharedCount(a.DownCells, b.DownCells);

            Assert.AreEqual(2, sharedUp + sharedDown,
                "adjacent hexes share exactly 2 render-cell vertices in total across both tilemaps");
        }

        private static int SharedCount(Vector3Int[] left, Vector3Int[] right)
        {
            var set = new HashSet<Vector3Int>(left);
            int shared = 0;
            foreach (var c in right) if (set.Contains(c)) shared++;
            return shared;
        }
    }
}
