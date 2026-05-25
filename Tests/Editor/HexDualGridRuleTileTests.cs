using NUnit.Framework;
using UnityEngine;
using skner.DualGrid.Utils;

namespace skner.DualGrid.Tests
{
    public class HexDualGridRuleTileTests
    {
        [Test]
        public void GetSprite_ReturnsAssignedUpSpriteForMask()
        {
            var tile = ScriptableObject.CreateInstance<HexDualGridRuleTile>();
            var s = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            tile.UpTriangleSprites[3] = s;

            Assert.AreSame(s, tile.GetSprite(TriangleKind.Up, 3));
        }

        [Test]
        public void GetSprite_ReturnsAssignedDownSpriteForMask()
        {
            var tile = ScriptableObject.CreateInstance<HexDualGridRuleTile>();
            var s = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            tile.DownTriangleSprites[5] = s;

            Assert.AreSame(s, tile.GetSprite(TriangleKind.Down, 5));
        }

        [Test]
        public void GetSprite_Mask0_ReturnsNull()
        {
            var tile = ScriptableObject.CreateInstance<HexDualGridRuleTile>();
            Assert.IsNull(tile.GetSprite(TriangleKind.Up, 0));
            Assert.IsNull(tile.GetSprite(TriangleKind.Down, 0));
        }

        [Test]
        public void GetSprite_OutOfRange_ReturnsNull()
        {
            var tile = ScriptableObject.CreateInstance<HexDualGridRuleTile>();
            Assert.IsNull(tile.GetSprite(TriangleKind.Up, -1));
            Assert.IsNull(tile.GetSprite(TriangleKind.Up, 8));
        }
    }
}
