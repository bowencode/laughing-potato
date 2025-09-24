using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityDemos.WebClient;

public static class SecurityExtensions
{
    public static string CreateClientToken(SigningCredentials credential, string clientId, string tokenEndpoint)
    {
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            clientId,
            tokenEndpoint,
            new List<Claim>()
            {
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
                new Claim(JwtClaimTypes.Subject, clientId),
                new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
            },
            now,
            now.AddMinutes(1),
            credential
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    public static async Task<TokenResponse> RequestSignedCredentialTokenAsync(this HttpClient client, string identityUrl, SigningCredentials credential, string clientId, string scope)
    {
        var disco = await client.GetDiscoveryDocumentAsync(identityUrl);
        if (disco.IsError) throw new Exception(disco.Error);

        var clientToken = CreateClientToken(credential, clientId, disco.TokenEndpoint);

        var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            Scope = scope,

            ClientAssertion =
            {
                Type = OidcConstants.ClientAssertionTypes.JwtBearer,
                Value = clientToken
            }
        });

        return response;
    }

    public const string RsaJwk = """
        {
            "p": "wkFyP_8s4qekhYRjn9aSAdw5gKiZgCBXdKOsCgcThcWvSbdDuvaODBFnl9bl-UdMAqPfZa1_MWmYiari8TrIhmDxvVekl8742IT7HtzTosWru_11EPGE7tkEfEsQqj3nx8-SgxSwpom6_BfBg9C6Rdp-i8QCs7n5-qRHsbU1Mnk",
            "kty": "RSA",
            "q": "sEE-Tv5vMlsJwCmoegZB88JcYr0UfsSeulYEt36shGyAJ6tqb60hLpERzK1vaGKpoPgW3sOUf16sGzvNmU8mnGKxxNgBt8SGwCEvP2rdsZRAO3S96KgZ6siSWqbNpRJcJ4WSx06IM0E_mnGtKofvc5HB9eWWbHqsI0tSJwd1IMk",
            "d": "RvZ2RmstRjpyX0bt9Vc_wVlmeHVwV7YgP0g5S_GKyvlyE-qmp0FofXioS-pQr1w0k8sbYJ8QbnC98blvUykxAZmBFg9fMQl1zmqEH0gIplpa4LKN9PprU9OwhouKJhYfYS6AEn0TREKMFYulazpHzczEkoCkKWdv3TUU88IrIaQjhM6TubfRhrnPvuNudhsuNjLjxFHw48Kqaum2LuG9TiTHmKzkrTomou4lEQHnvpUtL-NVP0bXos7IZ9pfx-xHCtrvzhvlIf0owj9fShel7K0WH5OUC5b1rEUQzFfkqBStA84UzHfOT7nC9cnVLlEhDmDtZKXJ8m5_laE5CuiQwQ",
            "e": "AQAB",
            "kid": "RShvkZpYVtWT2SDG68A-83ht_S-3BjHanC42JaLmfZg",
            "qi": "aG_OfBVC4tO2ZdnQn4JpcX6qznzN4V7DhCdrxNGzuM1EUQt55dAC1dybySX-mij0b51jEbyJjyIgOllR3Q0IHzOLp0_oL4FI9iG_vtDBsC-PKxrFqclMZ-jOIEOgH30cLEmLonh9kI94f3Xv3ZB_0qZcXD65GLBHjQDvSekoCV0",
            "dp": "J4gcEQwD_WgyYf2MLKYE9p3zkcw92MY8Jf987ll8TpfpUS4beXvdcnVESLEyAeXRgSQKPW3uWYbsxO9i2calsjseVvL0CvTSzXVaiXBVHWXuygYNgdI0xEqj0AuSq6KHfbEvVr-qJ2oWBGFtPz8F-Vs6r4THK_4n8irmcDK5Tbk",
            "alg": "RS256",
            "dq": "Y9liOhAL4IQQfwtahV5cKIKYIdup67LplPg8DdO2WgK5iz-huYhj_uRyJ_is337dR_KjyikUTB_O8lKrw8lCP1_uA2y3dGriR5_FC3E4DGzHAqe9Gjt5Czf7KV3LrFM9X6pdH4nOwPKa3Jy_lBOURG1zayFYWTYZpJj7_l5MLXk",
            "n": "hb6AccxcRXdSJqF86eLPbB62jEaIwDnlDPQKfBnCADFzf413NnUZQDo7YunWW6Jp0mWxamgfI0PycUwQnPZdRKASBOCFlGqBYST3_vhwTaW8XYmupWXxv7jipjhxRP3ajTPyyPoswZr7D43Sa7RPqGVgwPuJQC_GMrOxlfcsD3XmsLuIKxEgH7CM704-fiCjKuCNw3hGxmuwstqaOnF-5Am3yr0qZbeMERRsHZxhDnNhhU4zUnV48jDOFsqhKDumL1KxaBPSqATAZHAvt_aXdWWdh_WHV7GLVI7u7QtDLGC-RKfJfWJCtK4HJotCnmnF574bxNEiF4rt1zlOTWDBAQ"
        }
        """;
}