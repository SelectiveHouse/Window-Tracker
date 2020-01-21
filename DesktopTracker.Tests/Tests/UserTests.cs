using DesktopTracker.Infra.Models;
using System.Collections.ObjectModel;
using Xunit;

namespace DesktopTracker.Tests.Tests
{
    public class UserTests
    {
        [Fact]
        public void UserCollectionShouldAddObject()
        {
            // Arrange
            Collection<UserData> users = new Collection<UserData>();

            UserData user = new UserData
            {
                UserName = "Hello"
            };

            // Act
            users.Add(user);

            // Assert
            Assert.Contains(users, chk => chk.UserName == "Hello");
        }

        [Fact]
        public void UserCollectionShouldDeleteObject()
        {
            // Arrange
            Collection<UserData> users = new Collection<UserData>();

            UserData user = new UserData
            {
                UserName = "Hello"
            };

            // Act
            users.Add(user);
            users.Remove(user);

            // Assert
            Assert.DoesNotContain(users, chk => chk.UserName == "Hello");
        }
    }
}
