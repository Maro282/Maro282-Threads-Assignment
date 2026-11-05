

namespace OrderProcessing.Exceptions;

public class DataFileCorruptedException : Exception
{

    public string? FileName { get; }
    public DataFileCorruptedException(string? fileName, Exception? innerException = null)
        : base($"Data file '{fileName}' contains invalid JSON.", innerException)
    {
        FileName = fileName;
    }
}
