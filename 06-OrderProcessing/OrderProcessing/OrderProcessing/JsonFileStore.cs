
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderProcessing;

public class JsonFileStore
{
    // Shared Jason Serializer object
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };





}
