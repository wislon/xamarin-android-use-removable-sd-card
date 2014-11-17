using System;
using System.Linq;
using System.Text;
using Android.OS;
using Android.Text;
using Android.Util;
using Java.IO;

namespace TestExternalSd.StorageClasses
{
  /*

  Be  aware there's some android platform deprecation stuff you need to fiddle with, with 'StatFS'.

  http://developer.android.com/reference/android/os/StatFs.html
  http://stackoverflow.com/questions/12806676/determine-the-free-space-of-a-mounted-external-sd-card
  http://stackoverflow.com/questions/2941552/how-can-i-check-how-much-free-space-an-sd-card-mounted-on-an-android-device-has
  http://stackoverflow.com/questions/16834964/how-to-get-an-external-storage-sd-card-size-with-mounted-sd-card

  */

  public class ExternalSdStorageHelper
  {

    /// <summary>
    /// Remember to turn on the READ_EXTERNAL_STORAGE permission, or this just comes back empty
    /// Tries to establish whether there's an external SD card present. It's
    /// a little hacky; reads /proc/mounts and looks for /storage/ references,
    /// and iterates over those looking for things like 'ext' and 'sdcard' in
    /// the same line, e.g. /storage/extSdCard or /storage/externalSd or similar.
    /// For the moment, the existence of 'ext' as part of the path (not the file system type) 
    /// is a crucial flag. If it doesn't see that, it'll bail out and assume there 
    /// isn't one (even if there is and it's  named something else). 
    /// We may have to build a list over time. 
    /// Returns: The root of the mounted directory (with no trailing '/', or empty string if there isn't one)
    /// </summary>
    /// <returns>The root of the mounted directory (with NO trailing '/'), or empty string if there isn't one)</returns>
    public static string GetExternalSdCardPath()
    {
      string procMounts = ReadProcMounts();
      string sdCardEntry = ParseProcMounts(procMounts);

      // note that isWriteable may fail if the disk is mounted elsewhere
      if (!string.IsNullOrWhiteSpace(sdCardEntry) && IsWriteable(sdCardEntry))
      {
        return sdCardEntry;
      }
      return string.Empty;
    }

    /// <summary>
    /// Just returns the contents of /proc/mounts as a string.
    /// Note that you MAY need to wrap this call up in a try/catch if you
    /// anticipate permissions issues, but generally just reading from 
    /// this file is OK
    /// </summary>
    /// <returns></returns>
    public static string GetProcMountsContents()
    {
      return ReadProcMounts();
    }

    /// <summary>
    /// This is an expensive operation to call, because it physically tries to write to the media.
    /// Remember to turn on the WRITE_EXTERNAL_STORAGE permission, or this will always return false.
    /// </summary>
    /// <param name="pathToTest">The root path of the alleged SD card (e.g. /storage/externalSd), 
    /// or anywhere else you want to test (WITHOUT the trailing '/'). If you try to write to somewhere 
    /// you're not allowed to, you may get eaten by a dragon.</param>
    /// <returns>True if it could write to it, false if not</returns>
    public static bool IsWriteable(string pathToTest)
    {
      bool result = false;
      const string someTestText = "some test text";
      try
      {
        string testFile = string.Format("{0}/{1}.txt", pathToTest, Guid.NewGuid());
        Log.Info("ExternalSDStorageHelper", "Trying to write some test data to {0}", testFile);
        System.IO.File.WriteAllText(testFile, someTestText);
        Log.Info("ExternalSDStorageHelper", "Success writing some test data to {0}!", testFile);
        System.IO.File.Delete(testFile);
        Log.Info("ExternalSDStorageHelper", "Cleaned up test data file {0}", testFile);
        result = true;
      }
      catch (Exception ex) // shut up about it and move on, we obviously can't have it, so it's dead to us, we can't use it.
      {
        Log.Error("ExternalSDStorageHelper", string.Format("Exception: {0}\r\nMessage: {1}\r\nStack Trace: {2}", ex, ex.Message, ex.StackTrace));
      }
      return result;
    }

    /// <summary>
    /// example entries from /proc/mounts on a Samsung Galaxy S2 looks like:
    /// dev/block/dm-1 /mnt/asec/com.touchtype.swiftkey-2 ext4 ro,dirsync,nosuid,nodev,blah
    /// dev/block/dm-2 /mnt/asec/com.mobisystems.editor.office_registered-2 ext4 ro,dirsync,nosuid, blah
    /// dev/block/vold/259:3 /storage/sdcard0 vfat rw,dirsync, blah (this is NOT an external SD card)
    /// dev/block/vold/179:9 /storage/extSdCard vfat rw,dirsync,nosuid, blah (this IS an external SD card)
    /// </summary>
    /// <param name="procMounts"></param>
    /// <returns></returns>
    private static string ParseProcMounts(string procMounts)
    {
      string sdCardEntry = string.Empty;
      if (!string.IsNullOrWhiteSpace(procMounts))
      {
        var candidateProcMountEntries = procMounts.Split('\n', '\r').ToList();
        candidateProcMountEntries.RemoveAll(s => s.IndexOf("storage", StringComparison.OrdinalIgnoreCase) < 0);
        var bestCandidate = candidateProcMountEntries
          .FirstOrDefault(s => s.IndexOf("ext", StringComparison.OrdinalIgnoreCase) >= 0
                               && s.IndexOf("sd", StringComparison.OrdinalIgnoreCase) >= 0
                               && s.IndexOf("fat", StringComparison.OrdinalIgnoreCase) >= 0); // you can have things like fat, vfat, exfat, texfat, etc.

        // e.g. /dev/block/vold/179:9 /storage/extSdCard vfat rw,dirsync,nosuid, blah
        if (!string.IsNullOrWhiteSpace(bestCandidate))
        {
          var sdCardEntries = bestCandidate.Split(' ');
          sdCardEntry = sdCardEntries.FirstOrDefault(s => s.IndexOf("/storage/", System.StringComparison.OrdinalIgnoreCase) >= 0);
          return !string.IsNullOrWhiteSpace(sdCardEntry) ? string.Format("{0}", sdCardEntry) : string.Empty;
        }
      }
      return sdCardEntry;
    }

    /// <summary>
    /// This doesn't require you to add any permissions in your Manifest.xml, but you'll
    /// need to add READ_EXTERNAL_STORAGE at the very least to be able to determine if the external
    /// SD card is available and usable.
    /// </summary>
    /// <returns></returns>
    private static string ReadProcMounts()
    {
      Log.Info("ExternalSDStorageHelper", "Attempting to read '/proc/mounts' to see if there's an external SD card reference");
      try
      {
        string contents = System.IO.File.ReadAllText("/proc/mounts");
        return contents;
      }
      catch (Exception ex) // shut up about it and move on, we obviously can't have it, we can't use it.
      {
        Log.Error("ExternalSDStorageHelper", string.Format("Exception: {0}\r\nMessage: {1}\r\nStack Trace: {2}", ex, ex.Message, ex.StackTrace));
      }

      return string.Empty; // expect to fail by default
    }

    /// <summary>
    /// Uses the Linux Android.OS.StatFS wrapper call to try and see how many blocks are
    /// free/used/available. Results are returned in bytes.
    /// </summary>
    /// <param name="path">The path to use for the basis of the size check</param>
    /// <returns># of bytes free if successful, 0 if none</returns>
    public static FileSystemBlockInfo GetFileSystemBlockInfo(string path)
    {
      var statFs = new StatFs(path);
      var fsbi = new FileSystemBlockInfo();
      if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBeanMr2)
      {
        fsbi.Path = path;
        fsbi.BlockSizeBytes = statFs.BlockSizeLong;
        fsbi.TotalSizeBytes = statFs.BlockCountLong*statFs.BlockSizeLong;
        fsbi.AvailableSizeBytes = statFs.AvailableBlocksLong*statFs.BlockSizeLong;
        fsbi.FreeSizeBytes = statFs.FreeBlocksLong*statFs.BlockSizeLong;
      }
      else // this was deprecated in API level 18 (Android 4.3), so if your device is below level 18, this is what will be used instead.
      {
        fsbi.Path = path;
        // disable warning about obsoletes, since earlier versions of Android are using the deprecated versions
        // ReSharper disable CSharpWarnings::CS0618 
        fsbi.BlockSizeBytes = (long)statFs.BlockSize;
        fsbi.TotalSizeBytes = (long) statFs.BlockCount*(long) statFs.BlockSize;
        fsbi.FreeSizeBytes = (long) statFs.FreeBlocks*(long) statFs.BlockSize;
        fsbi.AvailableSizeBytes = (long) statFs.AvailableBlocks*(long) statFs.BlockSize;
        // ReSharper restore CSharpWarnings::CS0618
      }
      return fsbi;
    }

    /// <summary>
    /// Extended SD Card path location for KitKat (Android 19 / 4.4) and upwards.
    /// Must be called only devices >= KitKat or it'll crash, since some of these
    /// OS/API calls were only introduced in Android SDK level 19. 
    /// See http://developer.android.com/reference/android/content/Context.html#getExternalFilesDirs%28java.lang.String%29
    /// for more about GetExternalFilesDirs() - for an SD card it forces us to only write into that directory, we can't 
    /// write outside it. On the flip-side, we don't need write permission any more on >= KitKat.
    /// </summary>
    /// <returns></returns>
    public static string GetExternalSdCardPathEx()
    {
      File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
      // Array.ForEach(externalFilesDirs, efd => Log.Debug("ExternalSDStorageHelper", "Path: {0}\r\nMount State: {1}", efd.AbsolutePath, Android.OS.Environment.GetStorageState(efd)));
      // D/ExternalSDStorageHelper(31949): Path: /storage/emulated/0/Android/data/TestExternalSSD.TestExternalSSD/files
      // D/ExternalSDStorageHelper(31949): Mount State: mounted
      // D/ExternalSDStorageHelper(31949): Path: /storage/external_SD/Android/data/TestExternalSSD.TestExternalSSD/files
      // D/ExternalSDStorageHelper(31949): Mount State: mounted

      // if there are any items, the first will always be INTERNAL storage. Any subsequent items will be removable storage which
      // is permanently mounted (like inside the case, in an SD card slot). "Transient" storage like external USB drives is 
      // ignored, you won't see it in these results.
      if (externalFilesDirs.Any())
      {
        // var internalPath = externalFilesDirs[0].AbsolutePath.Split('/');
        // return string.Format("/{0}/{1}/{2}", internalPath[1], internalPath[2], internalPath[3]);
        // return externalFilesDirs.Length > 1 ? externalFilesDirs[1].AbsolutePath : externalFilesDirs[0].AbsolutePath;
        return externalFilesDirs.Length > 1 ? externalFilesDirs[1].AbsolutePath : string.Empty; // we only want the external drive, otherwise nothing!
      }
      return string.Empty;
    }
  }
}