using Android.OS;

namespace TestExternalSd.StorageClasses
{
  public static class ExternalSdCardInfo
  {
    private static string _path = null;
    private static bool? _isWriteable;
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
      get { return _path ?? GetExternalSdCardPath(); }
    }

    /// <summary>
    /// Returns whether the external SD card is writeable. The first
    /// call to this is an expensive one, because it actually tries to
    /// write a test file to the external disk
    /// </summary>
    public static bool IsWriteable
    {
      get { return _isWriteable ?? IsExternalCardWriteable(); }
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
      if (!string.IsNullOrWhiteSpace(Path))
      {
        _fileSystemBlockInfo = ExternalSdStorageHelper.GetFileSystemBlockInfo(_path);
        return _fileSystemBlockInfo;
      }
      return null;
    }


    private static bool IsExternalCardWriteable()
    {
      if (string.IsNullOrWhiteSpace(Path))
      {
        return false;
      }

      _isWriteable = ExternalSdStorageHelper.IsWriteable(_path);
      return _isWriteable.Value;
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
      return _path;
    }
  }
}
