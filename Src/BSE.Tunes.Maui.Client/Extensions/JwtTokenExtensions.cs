using System.Text;
using System.Text.Json;

namespace BSE.Tunes.Maui.Client.Extensions
{
    public static class JwtTokenExtensions
    {
        public static JwtTokenProperties ParseJwtToken(this string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid JWT token format", nameof(token));
            }

            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var json = Encoding.UTF8.GetString(jsonBytes);
            
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            return new JwtTokenProperties
            {
                Subject = root.TryGetProperty("sub", out var sub) ? sub.GetString() : null,
                Issuer = root.TryGetProperty("iss", out var iss) ? iss.GetString() : null,
                Audience = root.TryGetProperty("aud", out var aud) ? aud.GetString() : null,
                ExpirationTime = root.TryGetProperty("exp", out var exp) ? DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) : null,
                IssuedAt = root.TryGetProperty("iat", out var iat) ? DateTimeOffset.FromUnixTimeSeconds(iat.GetInt64()) : null,
                NotBefore = root.TryGetProperty("nbf", out var nbf) ? DateTimeOffset.FromUnixTimeSeconds(nbf.GetInt64()) : null,
                Claims = ParseClaims(root)
            };
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
        }

        private static Dictionary<string, object> ParseClaims(JsonElement root)
        {
            var claims = new Dictionary<string, object>();
            foreach (var property in root.EnumerateObject())
            {
                claims[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString(),
                    JsonValueKind.Number => property.Value.GetInt64(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => property.Value.ToString()
                };
            }
            return claims;
        }
    }

    public class JwtTokenProperties
    {
        public string? Subject { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public DateTimeOffset? ExpirationTime { get; set; }
        public DateTimeOffset? IssuedAt { get; set; }
        public DateTimeOffset? NotBefore { get; set; }
        public Dictionary<string, object> Claims { get; set; } = [];
        
        public bool IsExpired => ExpirationTime.HasValue && ExpirationTime.Value < DateTimeOffset.UtcNow;
    }
}