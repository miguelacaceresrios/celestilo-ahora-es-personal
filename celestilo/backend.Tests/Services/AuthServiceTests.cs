using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using backend.Services;
using backend.Configuration;
using Microsoft.AspNetCore.Http;
using backend.Model.Auth;

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
            null!, // ILogger<UserManager<IdentityUser>>
            null!  // IUserConfirmation<IdentityUser>
        );

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
    /// Test básico que muestra cómo un mock simula un comportamiento exitoso.
    /// El mock de UserManager simula que crea un usuario exitosamente.
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange: Set up test data and dependencies
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        // Aquí está la "magia" del mock: Setup() configura qué debe devolver cuando se llame a CreateAsync
        // Simula que el usuario se creó exitosamente (Success)
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Setup: Simula que AddToRoleAsync agrega exitosamente el rol "User"
        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        // Setup: Simula que GetRolesAsync devuelve "User"
        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string> { "User" });

        var authService = new AuthService(userManagerMock.Object, signInManagerMock.Object, jwtSettings, logger);

        var registerModel = new RegisterModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act: Ejecutar el método de registro
        var (succeeded, response, errors) = await authService.RegisterUserAsync(registerModel);

        // Assert: Verificar que el mock funcionó correctamente
        succeeded.Should().BeTrue();
        response.Should().NotBeNull();
        response!.Token.Should().NotBeEmpty();
        response.Username.Should().Be("testuser");
        response.Email.Should().Be("test@example.com");
        response.Roles.Should().Contain("User");

        // Verificar que el mock fue llamado exactamente una vez
        userManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Test que muestra cómo un mock simula un error.
    /// El mock simula que la creación de usuario falla (por ejemplo, email duplicado).
    /// </summary>
    [Fact]
    public async Task RegisterUserAsync_WithDuplicateEmail_ShouldReturnError()
    {
        // Arrange
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        // Setup: Simula que la creación falla con un error de email duplicado
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

        // Assert: Verificar que el mock simuló correctamente el error
        succeeded.Should().BeFalse();
        response.Should().BeNull();
        errors.Should().NotBeNull();
        errors.Should().ContainSingle();
        errors!.First().Code.Should().Be("DuplicateEmail");
    }

    /// <summary>
    /// Test que muestra cómo un mock simula un login exitoso.
    /// El mock simula encontrar el usuario y verificar la contraseña correctamente.
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

        // Setup: Simula encontrar el usuario por email
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(testUser);

        // Setup: Simula que la verificación de contraseña es exitosa
        signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false)).ReturnsAsync(SignInResult.Success);

        // Setup: Simula obtener los roles del usuario
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

        // Verificar que los mocks fueron llamados
        userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false), Times.Once);
    }

    /// <summary>
    /// Test que muestra cómo un mock simula un login fallido (usuario no encontrado).
    /// El mock simula que FindByEmailAsync devuelve null.
    /// </summary>
    [Fact]
    public async Task LoginUserAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var (userManagerMock, signInManagerMock, logger, jwtSettings) = CreateAuthServiceDependencies();

        // Setup: Simula que NO encuentra ningún usuario (devuelve null)
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

        // Verificar que FindByEmailAsync fue llamado
        userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        // Verificar que CheckPasswordSignInAsync NO fue llamado (porque no encontró usuario)
        signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false), Times.Never);
    }

    /// <summary>
    /// Test que muestra cómo un mock simula una contraseña incorrecta.
    /// El mock encuentra al usuario pero simula que la contraseña es incorrecta.
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

        // Setup: Encuentra el usuario
        userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(testUser);

        // Setup: Simula que la contraseña es incorrecta
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

        // Verificar que ambos mocks fueron llamados
        userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Once);
        signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), false), Times.Once);
    }
}