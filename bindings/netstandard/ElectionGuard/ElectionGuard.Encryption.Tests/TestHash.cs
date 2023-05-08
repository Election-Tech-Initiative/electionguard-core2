using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    public class TestTash
    {
        public void Test_HashElems_With_Interface_Array_Succeeds()
        {
            // Arrange
            const string prefix = "prefix";
            var elementModP = new ElementModP(1UL);
            var elementModQ = new ElementModQ(1UL);

            // Act
            var result = Hash.HashElems(prefix, elementModP, elementModQ);

            // Assert
            Assert.IsNotNull(result);


            // Clean up
            elementModP.Dispose();
            elementModQ.Dispose();
            result.Dispose();
        }
    }
}
