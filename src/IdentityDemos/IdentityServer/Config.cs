using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static class Policies
    {
        public const string Admin = "admin";
    }

    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResources.Phone(),
        new IdentityResources.Address(),
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedScopes = { "scope1" }
            },

            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client.jwt",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret
                    {
                        Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                        Value = """
                                {
                                    "kty": "RSA",
                                    "e": "AQAB",
                                    "kid": "RShvkZpYVtWT2SDG68A-83ht_S-3BjHanC42JaLmfZg",
                                    "alg": "RS256",
                                    "n": "hb6AccxcRXdSJqF86eLPbB62jEaIwDnlDPQKfBnCADFzf413NnUZQDo7YunWW6Jp0mWxamgfI0PycUwQnPZdRKASBOCFlGqBYST3_vhwTaW8XYmupWXxv7jipjhxRP3ajTPyyPoswZr7D43Sa7RPqGVgwPuJQC_GMrOxlfcsD3XmsLuIKxEgH7CM704-fiCjKuCNw3hGxmuwstqaOnF-5Am3yr0qZbeMERRsHZxhDnNhhU4zUnV48jDOFsqhKDumL1KxaBPSqATAZHAvt_aXdWWdh_WHV7GLVI7u7QtDLGC-RKfJfWJCtK4HJotCnmnF574bxNEiF4rt1zlOTWDBAQ"
                                }
                                """
                    }
                },

                AllowedScopes = { "scope1" }
            },

            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client.dpop",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("secret".Sha256()) },

                RequireDPoP = true,
                DPoPValidationMode = DPoPTokenExpirationValidationMode.IatAndNonce,

                AllowedScopes = { "scope1" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope2" }
            },

            // interactive client for server-side application
            new Client
            {
                ClientId = "web-ui",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                AllowOfflineAccess = true,
                RequireClientSecret = true,
                RequirePkce = false,

                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7163/signin-oidc",
                    "http://localhost:5163/signin-oidc",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:7163/signout-callback-oidc",
                    "http://localhost:5163/signout-callback-oidc",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "scope1",
                }
            },

            // interactive client for server-side application
            new Client
            {
                ClientId = "web-ui-pkce",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                AllowOfflineAccess = true,
                RequirePkce = true,
                RequireClientSecret = true,

                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7163/signin-oidc",
                    "http://localhost:5163/signin-oidc",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:7163/signout-callback-oidc",
                    "http://localhost:5163/signout-callback-oidc",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "scope1",
                }
            },

            // interactive client for server-side application
            new Client
            {
                ClientId = "web-ui-par",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
                AllowOfflineAccess = true,
                RequirePkce = true,
                RequireClientSecret = true,
                PushedAuthorizationLifetime = 300,
                RequirePushedAuthorization = true,

                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7163/signin-oidc",
                    "http://localhost:5163/signin-oidc",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:7163/signout-callback-oidc",
                    "http://localhost:5163/signout-callback-oidc",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "scope1",
                }
            },

            // implicit client
            new Client
            {
                ClientId = "web-ui-implicit",
                AllowedGrantTypes = GrantTypes.Implicit,
                RequireConsent = false,
                RequireClientSecret = false,
                AllowAccessTokensViaBrowser = true,

                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7163/signin-oidc",
                    "http://localhost:5163/signin-oidc",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:7163/signout-callback-oidc",
                    "http://localhost:5163/signout-callback-oidc",
                },

                AlwaysIncludeUserClaimsInIdToken = true,

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "scope1",
                }
            },

            // resource owner password grant client
            new Client
            {
                ClientId = "password-login",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                RequireConsent = false,
                AllowOfflineAccess = true,

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "scope1",
                }
            },

            // interactive reference client for server-side application
            new Client
            {
                ClientId = "web-ui-ref",

                AllowedGrantTypes = GrantTypes.Code,
                AccessTokenType = AccessTokenType.Reference,
                RequireClientSecret = false,

                // where to redirect to after login
                RedirectUris =
                {
                    "https://localhost:7163/signin-oidc",
                    "http://localhost:5163/signin-oidc",
                },

                // where to redirect to after logout
                PostLogoutRedirectUris =
                {
                    "https://localhost:7163/signout-callback-oidc",
                    "http://localhost:5163/signout-callback-oidc",
                },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "scope1",
                }
            },
        };
}