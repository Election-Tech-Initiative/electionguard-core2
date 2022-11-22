namespace ElectionGuard.UI.Test.Services
{
    internal class DbAvailabilityServiceTest
    {
        [Test]
        public void GivenDatabaseIsOnline_WhenIsDbAvailable_ThenItReturnsTrue()
        {
            // ARRANGE
            var dbAvailabilityService = new DbAvailabilityService();

            // ACT
            var isDbAvailable = dbAvailabilityService.IsDbAvailable();

            // ASSERT
            Assert.IsTrue(isDbAvailable);
        }
    }
}
