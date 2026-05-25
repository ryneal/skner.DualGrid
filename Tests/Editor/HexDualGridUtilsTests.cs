using NUnit.Framework;
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
    }
}
