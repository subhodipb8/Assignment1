using Xunit;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;
using ApiGateway.DelegatingHandlers;

namespace ApiGateway.Tests.DelegatingHandlers;

public class UserContextHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UserContextHandler _handler;

    public UserContextHandlerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new UserContextHandler(_httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task SendAsync_WithAuthenticatedUser_AddsUserContextHeaders()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Role, "student"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.Contains("X-User-Id").Should().BeTrue();
        request.Headers.GetValues("X-User-Id").First().Should().Be("123");
        request.Headers.GetValues("X-User-Role").First().Should().Be("student");
        request.Headers.GetValues("X-User-Email").First().Should().Be("test@example.com");
    }

    [Fact]
    public async Task SendAsync_WithoutAuthentication_DoesNotAddHeaders()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.Contains("X-User-Id").Should().BeFalse();
        request.Headers.Contains("X-User-Role").Should().BeFalse();
        request.Headers.Contains("X-User-Email").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WithNullHttpContext_DoesNotAddHeaders()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.Contains("X-User-Id").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WithPartialClaims_AddsOnlyAvailableHeaders()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "456"),
            new Claim(ClaimTypes.Role, "admin")
            // No email claim
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.Contains("X-User-Id").Should().BeTrue();
        request.Headers.Contains("X-User-Role").Should().BeTrue();
        request.Headers.Contains("X-User-Email").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_PassesRequestToInnerHandler()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "123") };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        var innerHandler = new TestInnerHandler(expectedResponse);

        // Use TestHandlerWrapper to properly invoke the handler chain
        var wrapper = new TestHandlerWrapper(_handler, innerHandler);
        var invoker = new HttpMessageInvoker(wrapper);

        // Act
        var response = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        response.Should().Be(expectedResponse);
        innerHandler.ReceivedRequest.Should().Be(request);
    }

    [Fact]
    public async Task SendAsync_WithOnlyUserIdClaim_AddsOnlyUserIdHeader()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "789") };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.GetValues("X-User-Id").First().Should().Be("789");
        request.Headers.Contains("X-User-Role").Should().BeFalse();
        request.Headers.Contains("X-User-Email").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_PreservesExistingHeaders()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Role, "student")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        request.Headers.Add("X-Existing-Header", "value");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.GetValues("X-Existing-Header").First().Should().Be("value");
        request.Headers.GetValues("X-User-Id").First().Should().Be("123");
    }

    [Theory]
    [InlineData("student")]
    [InlineData("staff")]
    [InlineData("admin")]
    [InlineData("canteen")]
    public async Task SendAsync_HandlesDifferentRoles(string role)
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.GetValues("X-User-Role").First().Should().Be(role);
    }

    [Fact]
    public async Task SendAsync_HandlesEmptyStringClaims()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ""),
            new Claim(ClaimTypes.Role, ""),
            new Claim(ClaimTypes.Email, "")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        // Empty strings should not be added as headers
        request.Headers.Contains("X-User-Id").Should().BeFalse();
        request.Headers.Contains("X-User-Role").Should().BeFalse();
        request.Headers.Contains("X-User-Email").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_HandlesLargeUserId()
    {
        // Arrange
        var largeId = int.MaxValue.ToString();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, largeId) };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var invoker = new HttpMessageInvoker(new TestHandlerWrapper(_handler));
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.GetValues("X-User-Id").First().Should().Be(largeId);
    }

    // Test helper classes
    private class TestHandlerWrapper : DelegatingHandler
    {
        public TestHandlerWrapper(DelegatingHandler handler, HttpMessageHandler? innerHandler = null)
        {
            handler.InnerHandler = innerHandler ?? new DummyInnerHandler();
            InnerHandler = handler;
        }
    }

    private class DummyInnerHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private class TestInnerHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? ReceivedRequest { get; private set; }

        public TestInnerHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ReceivedRequest = request;
            return Task.FromResult(_response);
        }
    }
}
