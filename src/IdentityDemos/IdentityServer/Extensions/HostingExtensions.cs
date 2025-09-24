using Duende.IdentityServer;
using Duende.IdentityServer.ResponseHandling;
using IdentityServer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Filters;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityServer.Extensions;

internal static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        // Write most logs to the console but diagnostic data to a file.
        // See https://docs.duendesoftware.com/identityserver/diagnostics/data
        builder.Host.UseSerilog((ctx, lc) =>
        {
            lc.WriteTo.Logger(consoleLogger =>
            {
                consoleLogger.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                    formatProvider: CultureInfo.InvariantCulture);
                if (builder.Environment.IsDevelopment())
                {
                    consoleLogger.Filter.ByExcluding(Matching.FromSource("Duende.IdentityServer.Diagnostics.Summary"));
                }
            });
            if (builder.Environment.IsDevelopment())
            {
                lc.WriteTo.Logger(fileLogger =>
                {
                    fileLogger
                        .WriteTo.File("./diagnostics/diagnostic.log", rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 1024 * 1024 * 10, // 10 MB
                            rollOnFileSizeLimit: true,
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                            formatProvider: CultureInfo.InvariantCulture)
                        .Filter
                        .ByIncludingOnly(Matching.FromSource("Duende.IdentityServer.Diagnostics.Summary"));
                }).Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration);
            }
        });
        return builder;
    }

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddRazorPages()
            .AddRazorRuntimeCompilation();

        // Configure the JwtSecurityTokenHandler to not map inbound claims automatically
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var isBuilder = builder.Services
            .AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            options.ServerSideSessions.UserDisplayNameClaimType = "name";

            // Use a large chunk size for diagnostic logs in development where it will be redirected to a local file
            if (builder.Environment.IsDevelopment())
            {
                options.Diagnostics.ChunkSize = 1024 * 1024 * 10; // 10 MB
            }
        })
            // enable Private JWT Secrets
            .AddJwtBearerClientAuthentication()
            .AddTestUsers(TestUsers.Users)
            .AddLicenseSummary();

        // in-memory, code config
        isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
        isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
        isBuilder.AddInMemoryClients(Config.Clients);


        // if you want to use server-side sessions: https://blog.duendesoftware.com/posts/20220406_session_management/
        // then enable it
        //isBuilder.AddServerSideSessions();
        //
        // and put some authorization on the admin/management pages
        //builder.Services.AddAuthorization(options =>
        //       options.AddPolicy("admin",
        //           policy => policy.RequireClaim("sub", "1"))
        //   );
        //builder.Services.Configure<RazorPagesOptions>(options =>
        //    options.Conventions.AuthorizeFolder("/ServerSideSessions", "admin"));


        builder.Services.AddAuthentication()
            .AddOpenIdConnect("oidc", "Sign-in with demo.duendesoftware.com", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                options.SaveTokens = true;

                options.Authority = "https://demo.duendesoftware.com";
                options.ClientId = "interactive.confidential";
                options.ClientSecret = "secret";
                options.ResponseType = "code";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        builder.Services.AddTransient<IAuthorizeInteractionResponseGenerator, StepUpInteractionResponseGenerator>();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Content Security Policy options
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-src 'none';");
            await next();
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            },
            ServeUnknownFileTypes = false,
            ContentTypeProvider = new FileExtensionContentTypeProvider
            {
                Mappings =
                {
                    [".woff2"] = "font/woff2",
                    [".woff"] = "font/woff"
                }
            }
        });

        app.UseRouting();
        app.UseIdentityServer();

            app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
