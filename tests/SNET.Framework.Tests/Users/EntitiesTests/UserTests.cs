using SNET.Framework.Domain.Entities;
using SNET.Framework.Domain;
using FluentAssertions;
using SNET.Framework.Domain.DomainEvents.Users;

namespace SNET.Framework.Tests.Users.EntitiesTests;

public class UserTests
{
    [Fact]
    public void Create_ShouldInitializeUserWithCorrectValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "1234567890";
        var password = "password123";

        // Act
        var user = User.Create(userId, firstName, lastName, email, phoneNumber, password);

        // Assert
        user.Id.Should().Be(userId);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.PhoneNumber.Should().Be(phoneNumber);
        user.PasswordHash.Should().NotBeNullOrEmpty();
        user.StatusId.Should().Be(1);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.Roles.Should().BeEmpty();
        user.DomainEvents.Should().ContainSingle(e => e is UserCreatedDomainEvent);
    }

    [Fact]
    public void Update_ShouldUpdateUserFields()
    {
        // Arrange
        var user = CreateTestUser();
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newEmail = "jane.smith@example.com";
        var newPhoneNumber = "0987654321";

        // Act
        user.Update(newFirstName, newLastName, newEmail, newPhoneNumber);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.Email.Should().Be(newEmail);
        user.PhoneNumber.Should().Be(newPhoneNumber);
        user.DomainEvents.Should().ContainSingle(e => e is UserUpdatedDomainEvent);
    }

    [Fact]
    public void StatusChange_ShouldUpdateStatusId()
    {
        // Arrange
        var user = CreateTestUser();
        var newStatus = 2;

        // Act
        user.StatusChange(newStatus);

        // Assert
        user.StatusId.Should().Be(newStatus);
    }

    [Fact]
    public void AssignRole_ShouldAddRoleToUser()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = Guid.NewGuid();

        // Act
        user.AssignRole(roleId);

        // Assert
        user.Roles.Should().ContainSingle(r => r.RoleId == roleId);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveRoleFromUser()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);

        // Act
        user.RemoveRole(roleId);

        // Assert
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Login_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var password = "password123";
        var user = CreateTestUser(password);

        // Act
        var result = user.Login(password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Autenticado correctamente");
        user.DomainEvents.Should().ContainSingle(e => e is UserLoginDomainEvent);
    }

    [Fact]
    public void Login_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CreateTestUser("correct_password");

        // Act
        var result = user.Login("wrong_password");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Autentication.NotMatchPassword");
    }

    [Fact]
    public void Login_ShouldReturnFailure_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateTestUser();
        user.StatusChange((int)StatusUser.Inactive);

        // Act
        var result = user.Login("password123");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Autentication.NotActive");
    }

    private User CreateTestUser(string password = "password123")
    {
        var userId = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "1234567890";

        return User.Create(userId, firstName, lastName, email, phoneNumber, password);
    }
}
