using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    internal class TestBigMath
    {
        [Test]
        public void RandQIsRandom()
        {
            var elementModQ1 = BigMath.RandQ();
            var elementModQ2 = BigMath.RandQ();
            Assert.IsNotNull(elementModQ1);
            Assert.AreNotEqual(elementModQ1.ToHex(), elementModQ2.ToHex());
        }
    }
}
