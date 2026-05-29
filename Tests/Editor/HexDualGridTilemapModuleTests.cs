using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using skner.DualGrid.Utils;

namespace skner.DualGrid.Tests
{
    public class HexDualGridTilemapModuleTests
    {
        private GameObject _grid;
        private HexDualGridTilemapModule _module;
        private HexDualGridRuleTile _ruleTile;

        [SetUp]
        public void SetUp()
        {
            _grid = new GameObject("Grid");
            var grid = _grid.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Hexagon;

            var data = new GameObject("Data");
            data.transform.SetParent(_grid.transform);
            data.AddComponent<Tilemap>();
            data.AddComponent<TilemapRenderer>();
            _module = data.AddComponent<HexDualGridTilemapModule>();

            var up = new GameObject(HexDualGridTilemapModule.UpRenderTilemapName);
            up.transform.SetParent(data.transform);
            up.AddComponent<Tilemap>();
            up.AddComponent<TilemapRenderer>();

            var down = new GameObject(HexDualGridTilemapModule.DownRenderTilemapName);
            down.transform.SetParent(data.transform);
            down.AddComponent<Tilemap>();
            down.AddComponent<TilemapRenderer>();

            _ruleTile = ScriptableObject.CreateInstance<HexDualGridRuleTile>();
            var s = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            for (int i = 0; i < 8; i++)
            {
                _ruleTile.UpTriangleSprites[i] = s;
                _ruleTile.DownTriangleSprites[i] = s;
            }
            _module.RuleTile = _ruleTile;
            _module.Orientation = HexOrientation.PointyTop;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_grid);
            Object.DestroyImmediate(_ruleTile);
        }

        [Test]
        public void SetDataTile_PopulatesSixRenderCells()
        {
            _module.SetDataTile(new Vector3Int(0, 0, 0));
            Assert.AreEqual(6, CountRendered());
        }

        [Test]
        public void ClearDataTile_RemovesAllRenderCellsWhenNoOtherDataTilesContribute()
        {
            _module.SetDataTile(new Vector3Int(0, 0, 0));
            _module.ClearDataTile(new Vector3Int(0, 0, 0));
            Assert.AreEqual(0, CountRendered());
        }

        [Test]
        public void AdjacentDataTiles_ShareTwoRenderCells()
        {
            _module.SetDataTile(new Vector3Int(0, 0, 0));
            int afterFirst = CountRendered();
            _module.SetDataTile(new Vector3Int(1, 0, 0));
            int afterSecond = CountRendered();

            Assert.AreEqual(6, afterFirst);
            Assert.AreEqual(10, afterSecond, "6 + 6 - 2 shared vertices");
        }

        [Test]
        public void AlignRenderTilemaps_UpCellZero_IsEquidistantFromItsDataNeighbors()
        {
            AssertRenderCellZeroIsCentroid(_module.UpRenderTilemap, TriangleKind.Up);
        }

        [Test]
        public void AlignRenderTilemaps_DownCellZero_IsEquidistantFromItsDataNeighbors()
        {
            AssertRenderCellZeroIsCentroid(_module.DownRenderTilemap, TriangleKind.Down);
        }

        // The centroid of an equilateral triangle (which a regular hex's dual triangle
        // is) is the unique point equidistant from all three vertices. Asserting equal
        // distances proves the render cell sits at the true triangle centroid, without
        // reusing the centroid formula the alignment code itself uses.
        private void AssertRenderCellZeroIsCentroid(Tilemap renderMap, TriangleKind kind)
        {
            _module.AlignRenderTilemaps();

            var neighbors = HexDualGridUtils.GetDataNeighborsForRenderCell(
                Vector3Int.zero, kind, HexOrientation.PointyTop);

            Vector3 center = renderMap.GetCellCenterWorld(Vector3Int.zero);
            float d0 = Vector3.Distance(center, _module.DataTilemap.GetCellCenterWorld(neighbors[0]));
            float d1 = Vector3.Distance(center, _module.DataTilemap.GetCellCenterWorld(neighbors[1]));
            float d2 = Vector3.Distance(center, _module.DataTilemap.GetCellCenterWorld(neighbors[2]));

            Assert.AreEqual(d0, d1, 1e-3f, "render cell center should be equidistant from data neighbors 0 and 1");
            Assert.AreEqual(d1, d2, 1e-3f, "render cell center should be equidistant from data neighbors 1 and 2");
        }

        private int CountRendered()
        {
            int n = 0;
            foreach (var p in _module.UpRenderTilemap.cellBounds.allPositionsWithin)
                if (_module.UpRenderTilemap.HasTile(p)) n++;
            foreach (var p in _module.DownRenderTilemap.cellBounds.allPositionsWithin)
                if (_module.DownRenderTilemap.HasTile(p)) n++;
            return n;
        }
    }
}
