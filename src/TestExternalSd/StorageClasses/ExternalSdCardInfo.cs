using Android.OS;

namespace TestExternalSd.StorageClasses
{
  public static class ExternalSdCardInfo
  {
    private static string _path = null;
    private static bool _isWriteable;
    private static FileSystemBlockInfo _fileSystemBlockInfo = null;

    /// <summary>
    /// Quick property you can check after initialising 
    /// </summary>
    public static bool ExternalSdCardExists
    {
      get { return !string.IsNullOrWhiteSpace(Path); }
    }

    /// <summary>
    /// Returns the path to External SD card (if there is one),
    /// otherwise empty string if there isn't
    /// </summary>
    public static string Path
    {
      get
      {
        return _path ?? GetExternalSdCardPath();
      }
    }

    /// <summary>
    /// Returns whether the external SD card is writeable. You need to have
    /// tried to access the <see cref="Path"/> or <see cref="ExternalSdCardExists"/> 
    /// property before the result of this makes any sense (it will always be false).
    /// </summary>
    public static bool IsWriteable
    {
      get { return _isWriteable; }
    }

    /// <summary>
    /// The values in the <see cref="FileSystemBlockInfo"/> object may have
    /// changed depending on what's going on in the file system, so it repopulates relatively
    /// expensively every time you read this property
    /// </summary>
    public static FileSystemBlockInfo FileSystemBlockInfo
    {
      get { return GetFileSystemBlockInfo(); }
    }

    private static FileSystemBlockInfo GetFileSystemBlockInfo()
    {
      if (!string.IsNullOrWhiteSpace(_path))
      {
        _fileSystemBlockInfo = ExternalSdStorageHelper.GetFileSystemBlockInfo(_path);
        return _fileSystemBlockInfo;
      }
      return null;
    }
    
    private static string GetExternalSdCardPath()
    {
      _path = string.Empty;
      if (Android.OS.Build.VERSION.SdkInt <= BuildVersionCodes.JellyBeanMr2)
      {
        _path = ExternalSdStorageHelper.GetExternalSdCardPath();
      }
      else
      {
        _path = ExternalSdStorageHelper.GetExternalSdCardPathEx();
      }
      if (!string.IsNullOrWhiteSpace(_path))
      {
        _isWriteable = ExternalSdStorageHelper.IsWritable(_path);
      }
      return _path;
    }
  }
}
