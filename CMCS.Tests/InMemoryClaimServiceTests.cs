// In CMCS.Tests/InMemoryClaimServiceTests.cs

using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;

namespace CMCS.Tests
{
    [TestClass]
    public class InMemoryClaimServiceTests
    {
        // Test 1: Ensure adding a claim increases the total count
        [TestMethod]
        public void AddClaim_ShouldIncreaseClaimCount_AndSetStatusToPending()
        {
            // Arrange: Set up the test
            var service = new InMemoryClaimService();
            var initialCount = service.GetAllClaims().Count;
            var newClaim = new Claim { HoursWorked = 10, HourlyRate = 100 };

            // Act: Perform the action to be tested
            service.AddClaim(newClaim);
            var allClaims = service.GetAllClaims();

            // Assert: Verify the outcome
            Assert.AreEqual(initialCount + 1, allClaims.Count);
            // Also verify that the new claim's status is correctly set to Pending
            var addedClaim = allClaims.First(c => c.Id == newClaim.Id);
            Assert.AreEqual(ClaimStatus.Pending, addedClaim.Status);
        }

        // Test 2: Ensure we can retrieve a specific claim by its ID
        [TestMethod]
        public void GetClaimById_WithValidId_ShouldReturnCorrectClaim()
        {
            // Arrange
            var service = new InMemoryClaimService();
            var claimToFind = service.GetAllClaims().First(); // Get the first sample claim

            // Act
            var result = service.GetClaimById(claimToFind.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(claimToFind.Id, result.Id);
            Assert.AreEqual(claimToFind.TotalAmount, result.TotalAmount);
        }

        // Test 3: Ensure searching for a non-existent ID returns null
        [TestMethod]
        public void GetClaimById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var service = new InMemoryClaimService();

            // Act
            var result = service.GetClaimById(999); // An ID that doesn't exist

            // Assert
            Assert.IsNull(result);
        }

        // Test 4: Ensure the UpdateClaim method correctly changes a claim's status
        [TestMethod]
        public void UpdateClaim_ShouldChangeClaimStatus()
        {
            // Arrange
            var service = new InMemoryClaimService();
            var claimToUpdate = service.GetClaimById(1); // Get the first claim
            Assert.IsNotNull(claimToUpdate);
            claimToUpdate.Status = ClaimStatus.Rejected; // Change its status

            // Act
            service.UpdateClaim(claimToUpdate);
            var updatedClaim = service.GetClaimById(1);

            // Assert
            Assert.IsNotNull(updatedClaim);
            Assert.AreEqual(ClaimStatus.Rejected, updatedClaim.Status);
        }

        // Test 5: Ensure new claims get a unique, auto-incremented ID
        [TestMethod]
        public void AddClaim_ShouldAssignUniqueId()
        {
            // Arrange
            var service = new InMemoryClaimService();
            var claim1 = new Claim { HoursWorked = 1, HourlyRate = 1 };
            var claim2 = new Claim { HoursWorked = 2, HourlyRate = 2 };

            // Act
            service.AddClaim(claim1);
            service.AddClaim(claim2);

            // Assert
            Assert.AreNotEqual(claim1.Id, claim2.Id);
            Assert.IsTrue(claim2.Id > claim1.Id);
        }
    }
}