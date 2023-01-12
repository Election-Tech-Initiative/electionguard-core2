using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    public class TestGroup
    {
        [Test]
        public void Test_G_Pow_P_With_random()
        {
            // TODO:
        }

        [Test]
        public void Test_Mod_P_Equal()
        {
            var first = new ElementModP(2);
            Assert.AreEqual(first, Constants.TWO_MOD_P);
        }
        [Test]
        public void Test_Mod_Q_Equal()
        {
            var first = new ElementModQ(1);
            Assert.AreEqual(first, Constants.ONE_MOD_Q);
        }
        [Test]
        public void Test_Mod_P_Not_Equal()
        {
            var first = new ElementModP(2);
            Assert.AreNotEqual(first, Constants.ONE_MOD_P);
        }
        [Test]
        public void Test_Mod_Q_Not_Equal()
        {
            var first = new ElementModQ(1);
            Assert.AreNotEqual(first, Constants.ZERO_MOD_Q);
        }
        [Test]
        public void Test_Mod_PQ_Not_Equal()
        {
            var first = new ElementModQ(1);
            Assert.AreNotEqual(first, Constants.ONE_MOD_P);
        }
    }
}
