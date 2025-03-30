using JWT_HMAC.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JWT_HMAC.Providers
{
    public static class JwtProvider
    {
        public static string CreateToken<T>(string secretKey, T payload) where T : PayloadBase
        {
            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };

            var headerJson = JsonSerializer.Serialize(header);
            var payloadJson = JsonSerializer.Serialize(payload);

            var headerBytes = Encoding.UTF8.GetBytes(headerJson);
            var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

            var header64 = Convert.ToBase64String(headerBytes);
            var payload64 = Convert.ToBase64String(payloadBytes);

            string unsignedToken = $"{Utils.CleanBase64(header64)}.{Utils.CleanBase64(payload64)}";
            string signature = HmacProvider.ComputeHash(secretKey, unsignedToken, HashAlgorithmName.SHA256);

            return $"{unsignedToken}.{Utils.CleanBase64(signature)}";
        }

        public static T ValidateToken<T>(string token, string secretKey) where T : PayloadBase
        {
            var parts = token.Split('.');
            
            if (parts.Length != 3) 
                return default;

            string header = parts[0];
            string payload = parts[1];
            string signature = parts[2];

            string unsignedToken = $"{header}.{payload}";
            string computedSignature = HmacProvider.ComputeHash(secretKey, unsignedToken, HashAlgorithmName.SHA256);

            // Verifica a assinatura
            if (signature != Utils.CleanBase64(computedSignature))
                return default;

            var payloadJson = Utils.BuildOriginal64(payload);
            var payloadObj = JsonSerializer.Deserialize<T>(payloadJson);

            if (payloadObj == null)
                return default;
       
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Verifica se ja expirou
            if (payloadObj.Exp < now) 
                return default;

            // Retorna o payload
            return payloadObj;
        }
    }
}
