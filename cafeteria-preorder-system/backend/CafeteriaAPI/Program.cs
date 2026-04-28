using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using CafeteriaAPI.Data;
using CafeteriaAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add NSwag for Swagger
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Cafeteria API";
    config.Version = "v1";
    config.Description = "Cafeteria Food Pre-ordering System API";
});

// Configure PostgreSQL
builder.Services.AddDbContext<CafeteriaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/v1/swagger.json";
    });
}

app.UseHttpsRedirection();

// Use CORS before authentication
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CafeteriaDbContext>();
    db.Database.Migrate();
    DbSeeder.SeedData(db);
}

// Open Swagger URL on startup (in development)
if (app.Environment.IsDevelopment())
{
    var swaggerUrl = "http://localhost:5000/swagger/index.html?url=/swagger/v1/swagger.json";
    try
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = swaggerUrl,
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
    }
    catch
    {
        // Ignore if browser can't be opened automatically
    }
}

app.Run();
