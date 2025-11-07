using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using backend.Services;
using backend.Configuration;
using Microsoft.AspNetCore.Http;
using backend.Model.Auth;
using Microsoft.VisualBasic;

namespace backend.Tests.Services;

/// <summary>
/// Unit tests for the <see cref="AuthService"/> class.
/// Tests cover user registration, authentication, and JWT token generation scenarios.
/// </summary>
public class AuthServiceTests
{
    /// <summary>
    /// Creates a mock <see cref="UserManager{TUser}"/> instance for testing.
    /// Configures the UserManager with the minimum required dependencies for authentication operations.
    /// </summary>
    /// <returns>A mocked <see cref="UserManager{IdentityUser}"/> instance ready for test scenarios.</returns>
    private static Mock<UserManager<IdentityUser>> CreateMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();

        var userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object,
            null!, // IOptions<IdentityOptions>
            null!, // IPasswordHasher<IdentityUser>
            null!, // IEnumerable<IUserValidator<IdentityUser>>
            null!, // IEnumerable<IPasswordValidator<IdentityUser>>
            null!, // ILookupNormalizer 
            null!, // IdentityErrorDescriber
            null!, // IServiceProvider
            null! // ILogger<UserManager<IdentityUser>>
        )
        { CallBase = true };

        return userManagerMock;
    }

    /// <summary>
    /// Creates a mock <see cref="SignInManager{TUser}"/> instance for testing.
    /// Configures the SignInManager with required dependencies including UserManager, HttpContext, and Claims factory.
    /// </summary>
    /// <param name="userManager">The mocked UserManager instance to be used by SignInManager.</param>
    /// <returns>A mocked <see cref="SignInManager{IdentityUser}"/> instance ready for test scenarios.</returns>
    private static Mock<SignInManager<IdentityUser>> CreateMockSignInManager(Mock<UserManager<IdentityUser>> userManager)
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();

        var signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            userManager.Object,
            httpContextAccessorMock.Object,
            userClaimsPrincipalFactoryMock.Object,
            null!, // IOptions<IdentityOptions>
            null!, // ILogger<SignInManager<IdentityUser>>
            null!, // IAuthenticationSchemeProvider
            null!  // IUserConfirmation<IdentityUser>
        );

        return signInManagerMock;
    }

    /// <summary>
    /// Creates a mock logger instance for dependency injection in test scenarios.
    /// </summary>
    /// <returns>A mocked <see cref="ILogger{T}"/> instance for <see cref="AuthService"/>.</returns>
    private static ILogger<AuthService> CreateMockLogger()
    {
        return Mock.Of<ILogger<AuthService>>();
    }

    /// <summary>
    /// Creates JWT settings configuration for testing purposes.
    /// Uses secure test values that meet minimum requirements for JWT token generation.
    /// </summary>
    /// <returns>A <see cref="JwtSettings"/> instance configured with test values.</returns>
    private static JwtSettings CreateTestJwtSettings()
    {
        return new JwtSettings
        {
            SecretKey = "3z%yBtGh7F6Ar%$w1^ZNU7a!%*wh7!QI", //test key
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };
    }

    /// <summary>
    /// Creates all required dependencies for testing <see cref="AuthService"/>.
    /// Each test receives fresh mock instances to prevent test interference.
    /// </summary>
    /// <returns>
    /// A tuple containing all dependencies needed to instantiate <see cref="AuthService"/>:
    /// - UserManager mock instance
    /// - SignInManager mock instance  
    /// - Logger mock instance
    /// - JWT settings configuration
    /// </returns>
    private static (Mock<UserManager<IdentityUser>> userManager, Mock<SignInManager<IdentityUser>> signInManager, ILogger<AuthService> logger, JwtSettings jwtSettings)
     CreateAuthServiceDependencies()
    {
        var userManagerMock = CreateMockUserManager();
        var signInManagerMock = CreateMockSignInManager(userManagerMock);
        var logger = CreateMockLogger();
        var jwtSettings = CreateTestJwtSettings();

        return (userManagerMock, signInManagerMock, logger, jwtSettings);
    }

    /// <summary>
    /// Tests successful user registration with valid data.
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(["User"]);

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, jwtSettings, logger);

        var registerModel = new RegisterModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123"
        };

        // Act
        var (succeeded, response, errors) = await authService.RegisterUserAsync(registerModel);

        // Assert
        succeeded.Should().BeTrue();
        response.Should().NotBeNull();
        response!.Token.Should().NotBeEmpty();
        response.Username.Should().Be("testuser");
        response.Email.Should().Be("test@example.com");
        response.Roles.Should().Contain("User");
        errors.Should().BeNull();
        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Once);
        userManagerMock.Verify(x=> x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Tests user registration failure when email already exists.
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_WithDuplicateEmail_ShouldReturnError()
    {
        // Arrange
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        var error = new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" };

        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(error));

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, jwtSettings, logger);

        var registerModel = new RegisterModel
        {
            UserName = "testuser",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        // Act
        var (succeeded, response, errors) = await authService.RegisterUserAsync(registerModel);

        // Assert
        succeeded.Should().BeFalse();
        response.Should().BeNull();
        errors.Should().NotBeNull();
        errors.Should().ContainSingle();
        errors!.First().Code.Should().Be("DuplicateEmail");
    }

    /// <summary>
    /// Tests successful login with valid credentials.
    /// </summary>
    [Fact]
    public async Task LoginUserAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        var testUser = new IdentityUser
        {
            Id = "456",
            UserName = "loginuser",
            Email = "login@example.com"
        };

        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(testUser);

        signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false)).ReturnsAsync(SignInResult.Success);

        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string> { "User" });

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, jwtSettings, logger);

        var loginModel = new LoginModel
        {
            Email = "login@example.com",
            Password = "CorrectPassword123!"
        };

        // Act
        var (succeeded, response) = await authService.LoginUserAsync(loginModel);

        // Assert
        succeeded.Should().BeTrue();
        response.Should().NotBeNull();
        response!.Token.Should().NotBeEmpty();
        response.Email.Should().Be("login@example.com");
        response.Username.Should().Be("loginuser");

        userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false), Times.Once);
    }

    /// <summary>
    /// Tests login failure when user does not exist.
    /// </summary>
    [Fact]
    public async Task LoginUserAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser?)null);

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, jwtSettings, logger);

        var loginModel = new LoginModel
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        // Act
        var (succeeded, response) = await authService.LoginUserAsync(loginModel);

        // Assert
        succeeded.Should().BeFalse();
        response.Should().BeNull();

        userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false), Times.Never);
    }

    /// <summary>
    /// Tests login failure when password is incorrect.
    /// </summary>
    [Fact]
    public async Task LoginUserAsync_WithWrongPassword_ShouldReturnFalse()
    {
        // Arrange
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        var testUser = new IdentityUser
        {
            Id = "789",
            UserName = "wrongpassuser",
            Email = "wrongpass@example.com"
        };

        userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(testUser);

        signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false)).ReturnsAsync(SignInResult.Failed);

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, jwtSettings, logger);

        var loginModel = new LoginModel
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var (succeeded, response) = await authService.LoginUserAsync(loginModel);

        // Assert
        succeeded.Should().BeFalse();
        response.Should().BeNull();

        userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false), Times.Once);
    }
}