using System.Security.Claims;

namespace ApiGateway.DelegatingHandlers;

public class UserContextHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context?.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
            var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                request.Headers.Add("X-User-Id", userId);
            }

            if (!string.IsNullOrEmpty(userRole))
            {
                request.Headers.Add("X-User-Role", userRole);
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                request.Headers.Add("X-User-Email", userEmail);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
