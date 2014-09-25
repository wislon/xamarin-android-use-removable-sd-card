namespace TestExternalSd.StorageClasses
{
  /// <summary>
  /// File system block info for a given path, populated by
  /// <see cref="ExternalSdStorageHelper.GetFileSystemBlockInfo"/> method.
  /// Note that that method call is a relatively expensive one.
  /// </summary>
  public class FileSystemBlockInfo
  {
    /// <summary>
    /// The path you asked to check file allocation blocks for
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The file system block size, in bytes, for the given path
    /// </summary>
    public double BlockSizeBytes { get; set; }

    /// <summary>
    /// Total size of the file system at the given path
    /// </summary>
    public double TotalSizeBytes { get; set; }

    /// <summary>
    /// Available size of the file system at the given path
    /// </summary>
    public double AvailableSizeBytes { get; set; }

    /// <summary>
    /// Total free size of the file system at the given path
    /// </summary>
    public double FreeSizeBytes { get; set; }
  }
}