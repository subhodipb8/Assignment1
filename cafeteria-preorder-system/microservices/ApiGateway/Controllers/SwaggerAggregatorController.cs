using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiGateway.Controllers;

/// <summary>
/// Swagger aggregator controller that combines OpenAPI specifications from all microservices
/// </summary>
[ApiController]
[Route("api/swagger")]
[Produces("application/json")]
public class SwaggerAggregatorController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SwaggerAggregatorController> _logger;

    /// <summary>
    /// Initializes a new instance of the SwaggerAggregatorController
    /// </summary>
    /// <param name="httpClientFactory">HTTP client factory for creating clients</param>
    /// <param name="logger">Logger instance</param>
    public SwaggerAggregatorController(
        IHttpClientFactory httpClientFactory,
        ILogger<SwaggerAggregatorController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    /// <summary>
    /// Get consolidated OpenAPI specification from all microservices
    /// </summary>
    /// <remarks>
    /// Fetches and merges OpenAPI specifications from AuthService, MenuService, and OrderService.
    /// Paths are prefixed with service identifiers to avoid conflicts.
    /// </remarks>
    /// <returns>Combined OpenAPI specification as JSON</returns>
    /// <response code="200">Successfully aggregated OpenAPI specifications</response>
    /// <response code="500">Failed to aggregate specifications</response>
    [HttpGet("consolidated")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetConsolidatedSpec()
    {
        var services = new Dictionary<string, string>
        {
            { "auth", "http://localhost:5001/swagger/v1/swagger.json" },
            { "menu", "http://localhost:5002/swagger/v1/swagger.json" },
            { "orders", "http://localhost:5003/swagger/v1/swagger.json" }
        };

        var consolidatedDoc = new JsonObject
        {
            ["openapi"] = "3.0.1",
            ["info"] = new JsonObject
            {
                ["title"] = "Cafeteria Pre-order System - Consolidated API",
                ["version"] = "v1",
                ["description"] = "Combined API specification for all microservices in the Cafeteria Pre-order System. This document aggregates AuthService, MenuService, and OrderService APIs.",
                ["contact"] = new JsonObject
                {
                    ["name"] = "Cafeteria Support",
                    ["email"] = "support@cafeteria.com"
                }
            },
            ["servers"] = new JsonArray
            {
                new JsonObject { ["url"] = "http://localhost:5000", ["description"] = "API Gateway (Development)" }
            },
            ["paths"] = new JsonObject(),
            ["components"] = new JsonObject
            {
                ["schemas"] = new JsonObject(),
                ["securitySchemes"] = new JsonObject
                {
                    ["Bearer"] = new JsonObject
                    {
                        ["type"] = "http",
                        ["scheme"] = "bearer",
                        ["bearerFormat"] = "JWT",
                        ["description"] = "JWT Authorization header using the Bearer scheme"
                    }
                }
            },
            ["security"] = new JsonArray
            {
                new JsonObject
                {
                    ["Bearer"] = new JsonArray()
                }
            }
        };

        var paths = consolidatedDoc["paths"]!.AsObject();
        var schemas = consolidatedDoc["components"]!["schemas"]!.AsObject();

        foreach (var (serviceName, swaggerUrl) in services)
        {
            try
            {
                _logger.LogInformation("Fetching swagger spec from {Service} at {Url}", serviceName, swaggerUrl);
                var response = await _httpClient.GetStringAsync(swaggerUrl);
                var doc = JsonNode.Parse(response);

                if (doc == null)
                {
                    _logger.LogWarning("Failed to parse swagger spec from {Service}", serviceName);
                    continue;
                }

                // Merge paths
                if (doc["paths"] is JsonObject servicePaths)
                {
                    foreach (var path in servicePaths)
                    {
                        var gatewayPath = MapToGatewayPath(serviceName, path.Key);
                        if (!paths.ContainsKey(gatewayPath))
                        {
                            paths[gatewayPath] = path.Value;
                        }
                    }
                }

                // Merge schemas with service prefix to avoid conflicts
                if (doc["components"]?.AsObject()["schemas"] is JsonObject serviceSchemas)
                {
                    foreach (var schema in serviceSchemas)
                    {
                        var prefixedName = $"{serviceName}_{schema.Key}";
                        if (!schemas.ContainsKey(prefixedName))
                        {
                            schemas[prefixedName] = schema.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch swagger spec from {Service} at {Url}", serviceName, swaggerUrl);
            }
        }

        return Content(consolidatedDoc.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), "application/json");
    }

    /// <summary>
    /// Get list of available API specifications
    /// </summary>
    /// <returns>List of available API documentation endpoints</returns>
    /// <response code="200">List of API specifications</response>
    [HttpGet("services")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetAvailableServices()
    {
        return Ok(new
        {
            services = new[]
            {
                new
                {
                    name = "API Gateway",
                    description = "Gateway and consolidated API documentation",
                    swaggerUrl = "http://localhost:5000/swagger/v1/swagger.json",
                    uiUrl = "http://localhost:5000/swagger"
                },
                new
                {
                    name = "Auth Service",
                    description = "Authentication and user management",
                    swaggerUrl = "http://localhost:5001/swagger/v1/swagger.json",
                    uiUrl = "http://localhost:5001/swagger"
                },
                new
                {
                    name = "Menu Service",
                    description = "Menu item management",
                    swaggerUrl = "http://localhost:5002/swagger/v1/swagger.json",
                    uiUrl = "http://localhost:5002/swagger"
                },
                new
                {
                    name = "Order Service",
                    description = "Order management and processing",
                    swaggerUrl = "http://localhost:5003/swagger/v1/swagger.json",
                    uiUrl = "http://localhost:5003/swagger"
                }
            }
        });
    }

    /// <summary>
    /// Get health status of all microservices
    /// </summary>
    /// <returns>Health status for each service</returns>
    /// <response code="200">Health status information</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthStatus()
    {
        var services = new Dictionary<string, string>
        {
            { "auth", "http://localhost:5001/swagger" },
            { "menu", "http://localhost:5002/swagger" },
            { "orders", "http://localhost:5003/swagger" }
        };

        var healthChecks = new List<object>();

        foreach (var (serviceName, url) in services)
        {
            var status = "unknown";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var response = await _httpClient.GetAsync(url);
                stopwatch.Stop();
                status = response.IsSuccessStatusCode ? "healthy" : "unhealthy";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status = "unhealthy";
                _logger.LogWarning(ex, "Health check failed for {Service}", serviceName);
            }

            healthChecks.Add(new
            {
                service = serviceName,
                status,
                url,
                latency_ms = stopwatch.ElapsedMilliseconds
            });
        }

        return Ok(new
        {
            timestamp = DateTime.UtcNow,
            gateway = "healthy",
            services = healthChecks
        });
    }

    /// <summary>
    /// Get consolidated Swagger UI HTML page
    /// </summary>
    /// <returns>HTML page with Swagger UI displaying consolidated API spec</returns>
    /// <response code="200">Swagger UI HTML page</response>
    [HttpGet("ui")]
    [Produces("text/html")]
    public IActionResult GetSwaggerUi()
    {
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Cafeteria API - Consolidated Documentation</title>
    <link rel=""stylesheet"" type=""text/css"" href=""https://unpkg.com/swagger-ui-dist@5.9.0/swagger-ui.css"" />
    <style>
        html { box-sizing: border-box; overflow: -moz-scrollbars-vertical; overflow-y: scroll; }
        *, *:before, *:after { box-sizing: inherit; }
        body { margin: 0; background: #fafafa; }
        .header { background: #1a1a1a; color: #fff; padding: 20px; text-align: center; }
        .header h1 { margin: 0 0 10px 0; }
        .header p { margin: 0; color: #ccc; }
        .service-links { display: flex; justify-content: center; gap: 20px; margin-top: 15px; flex-wrap: wrap; }
        .service-links a { color: #89bf04; text-decoration: none; }
        .service-links a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Cafeteria Pre-order System API</h1>
        <p>Consolidated documentation for all microservices</p>
        <div class=""service-links"">
            <a href=""http://localhost:5001/swagger"" target=""_blank"">Auth Service</a>
            <a href=""http://localhost:5002/swagger"" target=""_blank"">Menu Service</a>
            <a href=""http://localhost:5003/swagger"" target=""_blank"">Order Service</a>
        </div>
    </div>
    <div id=""swagger-ui""></div>
    <script src=""https://unpkg.com/swagger-ui-dist@5.9.0/swagger-ui-bundle.js""></script>
    <script src=""https://unpkg.com/swagger-ui-dist@5.9.0/swagger-ui-standalone-preset.js""></script>
    <script>
        window.onload = function() {
            window.ui = SwaggerUIBundle({
                url: '/api/swagger/consolidated',
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                ],
                plugins: [
                    SwaggerUIBundle.plugins.DownloadUrl
                ],
                layout: 'StandaloneLayout',
                validatorUrl: null
            });
        };
    </script>
</body>
</html>";
        return Content(html, "text/html");
    }

    private static string MapToGatewayPath(string serviceName, string originalPath)
    {
        // Map service paths to gateway routes
        return serviceName.ToLower() switch
        {
            "auth" => originalPath.Replace("/api/auth", "/api/auth").Replace("/api/users", "/api/users"),
            "menu" => originalPath,
            "orders" => originalPath,
            _ => originalPath
        };
    }
}
