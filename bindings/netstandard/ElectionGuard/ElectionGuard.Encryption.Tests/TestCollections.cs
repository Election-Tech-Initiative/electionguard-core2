using NUnit.Framework;

namespace ElectionGuard.Encryption.Tests
{
    [TestFixture]
    public class TestCollections
    {
        [Test]
        public void Test_Can_Create_Linked_list()
        {
            // Arrange
            var list = new LinkedList();

            // Act
            list.Append("some", "value");
            list.Append("another", "thing");

            var firstValue = list.GetValueAt(0);
            var secondValue = list.GetValueAt(1);

            // Assert
            Assert.That(firstValue == "value");
            Assert.That(secondValue == "thing");
            Assert.That(list["some"] == "value");
            Assert.That(list["another"] == "thing");
            Assert.That(list["notthere"] == null);
        }
    }
}
