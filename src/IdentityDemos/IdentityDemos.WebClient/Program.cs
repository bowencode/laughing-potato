using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.DPoP;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Duende.IdentityModel;

namespace IdentityDemos.WebClient;

public class Program
{
    public const AuthClientType AuthenticationMode = AuthClientType.NoPkce;

    public const string IdentityUrl = "https+http://identity";
    public const string IdentityUrlConfig = "services:identity:https:0";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.AddServiceDefaults();

        ConfigureAuthentication(builder);

        builder.Services.AddHttpClient("ApiService", client =>
        {
            client.BaseAddress = new Uri("https+http://apiservice");
        });
        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // added to enable authentication
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        app.MapDefaultEndpoints();

        app.Run();
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        builder.Services.AddHttpClient("IdentityServer", client =>
        {
            client.BaseAddress = new Uri(IdentityUrl);
        });

        string? identityUrl = builder.Configuration.GetValue<string>(IdentityUrlConfig);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = identityUrl;

                switch (AuthenticationMode)
                {
                    case AuthClientType.NoPkce:
                        ConfigureAuthCodeNoPkce(options);
                        break;
                    case AuthClientType.AuthCodePar:
                        ConfigureAuthCodePar(options);
                        break;
                    case AuthClientType.Implicit:
                        ConfigureImplicit(options);
                        break;
                    case AuthClientType.ReferenceToken:
                        ConfigureCodeReference(options);
                        break;
                    case AuthClientType.AuthCode:
                    default:
                        ConfigureAuthCodePkce(options);
                        break;
                }

                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("scope1");
                options.GetClaimsFromUserInfoEndpoint = true;
                
                options.SaveTokens = true;

                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "role";
            });

        builder.Services.AddClientCredentialsTokenManagement()
            .AddClient("m2m.client.dpop", client =>
            {
                client.TokenEndpoint = new Uri($"{identityUrl}/connect/token");
                client.DPoPJsonWebKey = DPoPProofKey.ParseOrDefault(SecurityExtensions.RsaJwk);
                client.ClientId = ClientId.Parse("m2m.client.dpop");
                client.ClientSecret = ClientSecret.Parse("secret");
                client.Scope = Scope.ParseOrDefault("scope1");
            });

        builder.Services.AddClientCredentialsHttpClient("DPoP-API", ClientCredentialsClientName.Parse("m2m.client.dpop"), client =>
        {
            client.BaseAddress = new Uri("https+http://apiservice");
        });

        builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, StepUpAuthorizationMiddlewareResultHandler>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireMfa", policy =>
            {
                policy.AuthenticationSchemes.Add("oidc");
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("amr", "mfa");
            });
        });
    }

    private static void ConfigureAuthCodePkce(OpenIdConnectOptions options)
    {
        options.ClientId = "web-ui-pkce";
        options.ClientSecret = "secret";
        options.ResponseType = "code";

        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("scope1");
        options.Scope.Add("offline_access");
        options.GetClaimsFromUserInfoEndpoint = true;

        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

        options.ClaimActions.Remove("acr");

        options.Events.OnRedirectToIdentityProvider = ctx =>
        {
            if (ctx.Properties.Items.TryGetValue("acr_values", out var acrValues))
            {
                ctx.ProtocolMessage.AcrValues = acrValues;
            }

            return Task.CompletedTask;
        };

        options.Events.OnRemoteFailure = ctx =>
        {
            if (ctx.Failure?.Data.Contains("error") ?? false)
            {
                var error = ctx.Failure.Data["error"] as string;
                switch (error)
                {
                    case OidcConstants.AuthorizeErrors.UnmetAuthenticationRequirements:
                        ctx.HandleResponse();
                        ctx.Response.Redirect("/MfaFailed");
                        break;
                }
            }

            return Task.CompletedTask;
        };
    }

    private static void ConfigureImplicit(OpenIdConnectOptions options)
    {
        options.ClientId = "web-ui-implicit";
        options.ResponseType = "token id_token";
        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
    }

    private static void ConfigureAuthCodeNoPkce(OpenIdConnectOptions options)
    {
        options.ClientId = "web-ui";
        options.ClientSecret = "secret";
        options.ResponseType = "code";

        options.UsePkce = false;
        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

        options.Scope.Add("offline_access");

        options.Events.OnAuthorizationCodeReceived = ctx =>
        {
            var code = ctx.ProtocolMessage.Code;
            return Task.CompletedTask;
        };
    }

    private static void ConfigureCodeReference(OpenIdConnectOptions options)
    {
        options.ClientId = "web-ui-ref";
        options.ResponseType = "code";
    }

    private static void ConfigureAuthCodePar(OpenIdConnectOptions options)
    {
        options.ClientId = "web-ui-par";
        options.ClientSecret = "secret";
        options.ResponseType = "code";

        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Require;

        options.Scope.Add("offline_access");
    }
}

public enum AuthClientType
{
    AuthCode,
    NoPkce,
    AuthCodePar,
    Implicit,
    ReferenceToken,
}