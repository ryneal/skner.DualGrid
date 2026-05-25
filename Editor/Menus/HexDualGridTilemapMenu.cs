using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using skner.DualGrid.Utils;

namespace skner.DualGrid.Editor.Menus
{
    public static class HexDualGridTilemapMenu
    {
        [MenuItem("GameObject/2D Object/Hex Dual Grid Tilemap/Pointy-Top", false, 10)]
        public static void CreatePointyTop(MenuCommand cmd) => Create(cmd, HexOrientation.PointyTop);

        [MenuItem("GameObject/2D Object/Hex Dual Grid Tilemap/Flat-Top", false, 10)]
        public static void CreateFlatTop(MenuCommand cmd) => Create(cmd, HexOrientation.FlatTop);

        private static void Create(MenuCommand cmd, HexOrientation orientation)
        {
            var gridGO = new GameObject("Grid");
            GameObjectUtility.SetParentAndAlign(gridGO, cmd.context as GameObject);

            var grid = gridGO.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Hexagon;
            grid.cellSwizzle = orientation == HexOrientation.PointyTop
                ? GridLayout.CellSwizzle.XYZ
                : GridLayout.CellSwizzle.YXZ;

            var data = new GameObject("Tilemap");
            data.transform.SetParent(gridGO.transform);
            data.AddComponent<Tilemap>();
            var dataRenderer = data.AddComponent<TilemapRenderer>();
            dataRenderer.enabled = false;
            var module = data.AddComponent<HexDualGridTilemapModule>();
            module.Orientation = orientation;

            var up = new GameObject(HexDualGridTilemapModule.UpRenderTilemapName);
            up.transform.SetParent(data.transform);
            var upTilemap = up.AddComponent<Tilemap>();
            upTilemap.tileAnchor = HexDualGridUtils.GetRenderTilemapAnchor(TriangleKind.Up, orientation);
            up.AddComponent<TilemapRenderer>();

            var down = new GameObject(HexDualGridTilemapModule.DownRenderTilemapName);
            down.transform.SetParent(data.transform);
            var downTilemap = down.AddComponent<Tilemap>();
            downTilemap.tileAnchor = HexDualGridUtils.GetRenderTilemapAnchor(TriangleKind.Down, orientation);
            down.AddComponent<TilemapRenderer>();

            Undo.RegisterCreatedObjectUndo(gridGO, $"Create Hex Dual Grid Tilemap ({orientation})");
            Selection.activeGameObject = gridGO;
        }
    }
}
