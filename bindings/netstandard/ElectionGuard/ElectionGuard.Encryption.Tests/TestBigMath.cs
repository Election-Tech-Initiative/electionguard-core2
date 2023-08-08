using System.Collections.Generic;
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
            var result1 = Hash.HashElems(two, three);
            var result2 = Hash.HashElems(three, two);
            var result3 = Hash.HashElems(two, three);

            // Assert
            Assert.AreNotEqual(result1.ToHex(), result2.ToHex());
            Assert.AreEqual(result1.ToHex(), result3.ToHex());
        }

        [Test]
        public void HashElemsArray()
        {
            // Arrange
            var two = new ElementModP(2);
            var three = new ElementModP(3);

            var dataList = new List<ElementModP>();
            dataList.Add(two);
            dataList.Add(three);

            // Act
            var result1 = Hash.HashElems(two, three);
            var result2 = Hash.HashElems(dataList);

            // Assert
            Assert.AreNotEqual(result1.ToHex(), result2.ToHex());
        }

    }
}
