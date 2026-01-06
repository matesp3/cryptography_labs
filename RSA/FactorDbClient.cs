using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RSAcracker
{
    public class FactorDbClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public FactorDbClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://factordb.com/")
            };

            // odporúčaný (nie povinný) User-Agent
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "RSA-Homework/1.0 (academic use)"
            );
        }

        public async Task<FactorDbResponse?> GetFactorizationAsync(BigInteger n)
        {
            string query = $"api?query={n}";
            HttpResponseMessage response = await _httpClient.GetAsync(query);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<FactorDbResponse>(json);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class FactorDbResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        // každý faktor je dvojica [hodnota, násobnosť]
        [JsonPropertyName("factors")]
        public required JsonElement[][] Factors { get; set; }
    }

    public static class FactorDbHelper
    {
        public static List<BigInteger> ExtractPrimeFactors(FactorDbResponse response)
        {
            var result = new List<BigInteger>();

            foreach (var factor in response.Factors)
            {
                // faktor[0] môže byť string číslo alebo "C123"/"P123"
                string valueStr = factor[0].ValueKind switch
                {
                    JsonValueKind.String => factor[0].GetString()!,
                    JsonValueKind.Number => factor[0].GetRawText(), // číslo ako string
                    _ => throw new InvalidOperationException($"Neočekávaný typ faktora: {factor[0].ValueKind}")
                };

                // násobnosť
                int multiplicity = factor[1].ValueKind switch
                {
                    JsonValueKind.Number => factor[1].GetInt32(),
                    JsonValueKind.String => int.Parse(factor[1].GetString()!),
                    _ => throw new InvalidOperationException($"Neočekávaný typ násobnosti: {factor[1].ValueKind}")
                };

                // ak ide o odkaz na factorDB
                if (valueStr.StartsWith("C") || valueStr.StartsWith("P"))
                {
                    throw new InvalidOperationException("Potrebujeme FactorDbClient pre rekurzívny request");
                }

                // klasické číslo
                BigInteger value = BigInteger.Parse(valueStr);

                for (int m = 0; m < multiplicity; m++)
                    result.Add(value);
            }

            return result;
        }
    }
}
