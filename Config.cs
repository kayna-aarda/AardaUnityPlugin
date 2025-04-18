using System;
using Newtonsoft.Json;

public class Config
{
    public string HttpApiUrl { get; set; }
    public string WsApiUrl { get; set; }
    public string ApiUsername { get; set; }
    public string ApiPassword { get; set; }

    /// <summary>
    /// Deserializes a JSON string into a Config object.
    /// </summary>
    /// <param name="json">The JSON string representing the configuration.</param>
    /// <returns>A Config object populated with data from the JSON string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the json string is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when deserialization fails.</exception>
    public static Config LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentNullException(nameof(json), "JSON string cannot be null or empty.");

        try
        {
            return JsonConvert.DeserializeObject<Config>(json);
        }
        catch (JsonException ex)
        {
            // Optionally log the exception or handle it as needed
            throw new JsonException("Failed to deserialize JSON to Config object.", ex);
        }
    }
}
