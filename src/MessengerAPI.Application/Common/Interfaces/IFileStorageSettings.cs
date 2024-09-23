namespace MessengerAPI.Application.Common.Interfaces;

public interface IFileStorageSettings
{
    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public long MaxFileSize { get; }
}
