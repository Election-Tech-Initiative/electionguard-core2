using System;
using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    internal class TestBigMath
    {
        [Test]
        public void GPowPTakesArgToPower()
        {
            var elementModQ = Constants.TWO_MOD_Q;
            var result = BigMath.GPowP(elementModQ);
            const string gDoubled = "CB9B95A3B2BB20A189F71117D74B88F39E52514EF5EADCD4C0D41219D36CDD68F9E06C049F354A6CCB508B100B90B7A268D1C1EA5CE075A6B3DE58E5209AB80512C41F6C3C3170C70D2119198E679668277EED1361D2A378530892A05364D811D0FD97D1EEF4A354F9199C48EF4E35452DB557CCBD01CF0CC5EC5B0AB116D03E9385E42A299A947BC137697E6CEB70837FDA8A20D5AFB53517B6378BF0AE8003F1C052D976236C1CC6BC07BA8ED340E469249E3875ADB7E5B081A21A3E02B7C04E004D4C31923F3641428D894D06B66A60C13337FFEAE29850E2FF3D00032799B17DD79861F3827B5F8DA0070349578655560CD21B0355E862BCFED16B0A9B5C8F8C13F4641EBAE161E9DDB81C04A31215A8473F4926ACA2A9604A61394469815EDEF5F46443F1FE7B8A1760EEEF72E3B70FDF80EDA92C566343F04BF48C74179CE1B4A5A900F1E569A11118650C7266303C25CAF3891B4070BBA2D6F4E1C7A584241ABCAA1244245CBCC76130E293E66A54F7B0BB3F63288613D66B8CF3313AFCB1DA99BD35CCDE00D3128CC02003E9907F8F5C8672CDF50880E37820F0BF68D38E2AB8BB2AFDD3292B897689DC6CC923DC902782681486B25172406A012103FB4C5636439E9C1C7B11482A52D11F9FC0639EE7DABED8B37CA6B3E9B0F4472B67347E05444635C0BFA82C5ABA5CA7F78569A81F7A47666313A598D92C2F6342";
            Assert.AreEqual(gDoubled, result.ToHex());
        }

        [Test]
        public void RandQIsRandom()
        {
            var elementModQ1 = BigMath.RandQ();
            var elementModQ2 = BigMath.RandQ();
            Assert.IsNotNull(elementModQ1);
            Assert.AreNotEqual(elementModQ1.ToHex(), elementModQ2.ToHex());
        }

        [Test]
        public void AddModPs()
        {
            var elementModP1 = Constants.ONE_MOD_P;
            var result = BigMath.AddModP(elementModP1, elementModP1);
            Assert.IsNotNull(result);
            Assert.AreEqual(Constants.TWO_MOD_P.ToHex(), result.ToHex());
        }

        [Test]
        public void AddModPInt()
        {
            var elementModP1 = Constants.ONE_MOD_P;
            var result = BigMath.AddModP(elementModP1, 1);
            Assert.IsNotNull(result);
            Assert.AreEqual(Constants.TWO_MOD_P.ToHex(), result.ToHex());
        }
        [Test]
        public void AddModQs()
        {
            var elementModQ1 = Constants.ONE_MOD_Q;
            var result = BigMath.AddModQ(elementModQ1, elementModQ1);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ToHex(), Constants.TWO_MOD_Q.ToHex());
        }

        [Test]
        public void AddModQInt()
        {
            var elementModQ1 = Constants.ONE_MOD_Q;
            var result = BigMath.AddModQ(elementModQ1, 1);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ToHex(), Constants.TWO_MOD_Q.ToHex());
        }

        [Test]
        public void APlusBMulCModQ()
        {
            // Arrange
            var one = new ElementModQ(1);
            var two = new ElementModQ(2);
            var three = new ElementModQ(3);
            var seven = new ElementModQ(7);

            // Act
            var result = BigMath.APlusBMulCModQ(one, two, three);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(seven.ToHex(), result.ToHex());
        }

        [Test]
        public void MultModP()
        {
            // Arrange
            var two = new ElementModP(2);
            var three = new ElementModP(3);
            var six = new ElementModP(6);

            // Act
            var result = BigMath.MultModP(two, three);

            // Assert
            Assert.AreEqual(six.ToHex(), result.ToHex());
        }

        [Test]
        public void PowModP()
        {
            // Arrange
            var two = new ElementModP(2);
            var three = new ElementModP(3);
            var eight = new ElementModP(8);

            // Act
            var result = BigMath.PowModP(two, three);

            // Assert
            Assert.AreEqual(eight.ToHex(), result.ToHex());
        }

        [Test]
        public void HashElems()
        {
            // Arrange
            var two = new ElementModP(2);
            var three = new ElementModP(3);

            // Act
            var result1 = BigMath.HashElems(two, three);
            var result2 = BigMath.HashElems(three, two);
            var result3 = BigMath.HashElems(two, three);

            // Assert
            Assert.AreNotEqual(result1.ToHex(), result2.ToHex());
            Assert.AreEqual(result1.ToHex(), result3.ToHex());
        }

    }
}
