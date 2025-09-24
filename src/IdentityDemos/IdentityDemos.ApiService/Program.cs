using Duende.AspNetCore.Authentication.JwtBearer.DPoP;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("services:identity:https:0");
        options.TokenValidationParameters.ValidateAudience = false;
        options.MapInboundClaims = false;
    })
    .AddJwtBearer("PoP", options =>
     {
         options.Authority = builder.Configuration.GetValue<string>("services:identity:https:0");
         options.TokenValidationParameters.ValidateAudience = false;
         options.MapInboundClaims = false;
     });
builder.Services.ConfigureDPoPTokensForScheme("PoP", opt =>
{
    opt.ValidationMode = ExpirationValidationMode.IssuedAt;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "scope1");
    });
    options.AddPolicy("ApiScopeDPoP", policy =>
    {
        policy.AddAuthenticationSchemes("PoP");
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "scope1");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/api/echo/{message?}", (ClaimsPrincipal user, string? message = null) =>
{
    var claimsList = user.Claims.Select(c => $"{c.Type} = {c.Value}").ToList();
    return Results.Ok(new
    {
        Message = message ?? "Hello",
        Timestamp = DateTime.UtcNow,
        Authenticated = user.Identity?.IsAuthenticated ?? false,
        Claims = claimsList
    });
}).RequireAuthorization("ApiScope");

app.MapGet("/api/echo-pop/{message?}", (ClaimsPrincipal user, string? message = null) =>
{
    var claimsList = user.Claims.Select(c => $"{c.Type} = {c.Value}").ToList();
    return Results.Ok(new
    {
        Message = message ?? "Hello",
        Timestamp = DateTime.UtcNow,
        Authenticated = user.Identity?.IsAuthenticated ?? false,
        Claims = claimsList
    });
}).RequireAuthorization("ApiScopeDPoP");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
