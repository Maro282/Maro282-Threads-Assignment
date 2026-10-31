
using OrderProcessing.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderProcessing;

public class JsonFileStore
{
    // Shared Jason Serializer object
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };


    // Lock on operation 
    // used because semaphoreslim allow me to use await
    private static readonly SemaphoreSlim Gate = new(1, 1);



    // ================================== Reading =======================================

    // ASync
    public static async Task<List<T>> ReadFromJsonFileASync<T>(string fileName, CancellationToken ct)
    {
        string folderPath = DataFiles.DataFolder;
        string filePath = Path.Combine(folderPath, fileName);

        await Gate.WaitAsync(ct);

        try
        {
            if (!File.Exists(filePath)) new List<T>();
            string json = await File.ReadAllTextAsync(filePath, ct);

            try
            {
                return JsonSerializer.Deserialize<List<T>>(json, _options) ??
                    throw new DataFileCorruptedException(fileName);
            }
            catch (JsonException ex)
            {
                throw new DataFileCorruptedException(fileName, ex);
            }
        }
        finally
        {
            Gate.Release();
        }

    }


    //Sync
    public static List<T> ReadFromJsonFile<T>(string fileName)
    {
        string folderPath = DataFiles.DataFolder;
        string filePath = Path.Combine(folderPath, fileName);

        Gate.Wait();

        try
        {
            if (!File.Exists(filePath)) new List<T>();
            string json = File.ReadAllText(filePath);

            try
            {
                return JsonSerializer.Deserialize<List<T>>(json, _options) ??
                    throw new DataFileCorruptedException(fileName);
            }
            catch (JsonException ex)
            {
                throw new DataFileCorruptedException(fileName, ex);
            }
        }
        finally
        {
            Gate.Release();
        }

    }

    // ================================== Writing =======================================

    // ASync
    public static async Task WriteToJsonFileASync<T>(string fileName, List<T> items, CancellationToken ct)
    {
        string folderPath = DataFiles.DataFolder;
        string filePath = Path.Combine(folderPath, fileName);
        Directory.CreateDirectory(filePath);

        await Gate.WaitAsync(ct);

        try
        {
            string json = JsonSerializer.Serialize(items, _options);
            string temp = Path.Combine(folderPath, fileName);
            try
            {

                await File.WriteAllTextAsync(temp, json, ct);
                File.Move(temp, filePath, overwrite: true);
            }
            catch (JsonException ex)
            {
                if (File.Exists(temp)) File.Delete(temp);
                throw;
            }

        }
        finally
        {
            Gate.Release();
        }



    }

    //Sync
    public static void WriteToJsonFile<T>(string fileName, List<T> items)
    {
        string folderPath = DataFiles.DataFolder;
        string filePath = Path.Combine(folderPath, fileName);
        Directory.CreateDirectory(filePath);

        Gate.Wait();

        try
        {
            string json = JsonSerializer.Serialize(items, _options);
            string temp = Path.Combine(folderPath, fileName);
            try
            {

                File.WriteAllText(temp, json);
                File.Move(temp, filePath, overwrite: true);
            }
            catch
            {
                if (File.Exists(temp)) File.Delete(temp);
                throw;
            }
        }
        finally
        {
            Gate.Release();
        }
    }





}