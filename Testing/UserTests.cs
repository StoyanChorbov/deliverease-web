using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Model;
using Model.DTO.User;
using Moq;
using Repository;
using Repository.Context;
using Service;

namespace Testing;

[TestFixture]
public class Tests
{
    private UserService _userService;
    private DelivereaseDbContext _context;
    private TokenRepository _tokenRepository;
    private UserRepository _userRepository;
    private Mock<IConfiguration> _configurationMock;
    private Mock<UserManager<User>> _userManagerMock;

    [SetUp]
    public void SetUp()
    {
        // In-memory DB setup
        var options = new DbContextOptionsBuilder<DelivereaseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        _context = new DelivereaseDbContext(options);
        _userRepository = new UserRepository(_context);
        _tokenRepository = new TokenRepository(_context);

        // UserManager is tricky, use helper method to mock
        _userManagerMock = MockUserManager();

        // IConfiguration mock
        _configurationMock = new Mock<IConfiguration>();

        // Instantiate the service with real repos and mocked services
        _userService = new UserService(
            _userRepository,
            _tokenRepository,
            _userManagerMock.Object,
            _configurationMock.Object
        );
    }

    private Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    [Test]
    public async Task GetByUsernameAsync_ReturnsUser_WhenUserExists()
    {
        var username = "testuser";
        var normalizedUsername = "TESTUSER";

        var testUser = new User { UserName = username };

        // Setup NormalizeName to return uppercase version
        _userManagerMock.Setup(um => um.NormalizeName(username))
            .Returns(normalizedUsername);

        // Setup FindByNameAsync to return the test user for normalized username
        _userManagerMock.Setup(um => um.FindByNameAsync(normalizedUsername))
            .ReturnsAsync(testUser);

        var result = await _userService.GetUserByUsernameAsync(username);

        Assert.IsNotNull(result);
        Assert.That(result.UserName, Is.EqualTo(username));
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
}