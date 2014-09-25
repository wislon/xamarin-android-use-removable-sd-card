### xamarin-android-use-removable-sd-card
Demonstrates how to how to access the 'external', __REMOVABLE__ SD card plugged into your Android device.

### Background
There's been a lot of questions on StackOverflow (and other places) about how to access the 'External Storage' on an Android device. Most people seem to want to know how to read from, and write to, __the SD card__ they just bought and put into their device. 

This sample project shows you how to do just that.

_Sadly, due to the gamification and 'first post!' mentality of some SO posts, a lot of developers go charging off, half-cocked, and answer the question they think is being asked, instead of the one __actually__ being asked. That's why there's so many duplicates of this type of question. And so most of the answers appear to get the concept of 'External' and 'Removable' mixed up, and then start preaching about how the 'External Storage' isn't actually EXTERNAL storage, it's just storage separate from the inaccessible, INTERNAL storage of the device. And sometimes that maybe the questioner is being a moron for getting the two concepts confused, and this may be a duplicate of that question over there. So most of those answers are essentially identical in implementation, and completely have the wrong end of the stick. Or the wrong stick._

Just about all Android devs understand that the 'External' storage does NOT refer to the SD card, even tho the file system sometimes refers to it as such. For the purposes of this post, I am going to refer to this as 'removable' storage, and it will refer to a removable (and usually micro) SD card, which a person has physically plugged into the device. And this is true of the Xamarin.Android developers as well, given that the Xamarin.Android C# 'engine' is so closely tied to the Android OS.

Most developers building for Android are also aware, on some level, that the operating system underneath the Dalvik VM is a Linux based one. 

That means that even if there's not a direct Xamarin or Android call to do what you need to do, there may sometimes be a more Linux-y way of doing it.

The first question we've got is: how do we determine whether we even have an removable SD card plugged into the device? 

This is both simultaneously easy to answer, and hard to solve. Easy, because we can make a single call to see what file systems we have mounted. And hard, because those damned device manufacturers haven't seen fit to settle on a standardised naming convention for a _removable_ SD card which is plugged into the device. 

To make matters worse, some manufacturers have hijacked the name of the _internal_ storage, and often called it something stupid like `sdcard0`.

On most Linux systems (which Android systems are based on), there's a file in the `/proc/` directory called `mounts`, which contains a (sometimes very long) list of all the file systems mounted on the device. 

It's a text file. And it doesn't require any special user permissions or device rooting to read it (though writing to it is another matter!).

Here's an example, from my ye anciente Samsung Galaxy S2, which has an SD card slot in it, and a micro-SD card plugged into it (the list is shortened substantially):

```text
rootfs / rootfs ro,relatime 0 0
tmpfs /dev tmpfs rw,nosuid,relatime,mode=755 0 0
devpts /dev/pts devpts rw,relatime,mode=600 0 0
blah blah more mounted stuff blah
:
/dev/block/mmcblk0p10 /data ext4 rw,nosuid,nodev,noatime,barrier=1,
/dev/block/mmcblk0p4 /mnt/.lfs j4fs rw,relatime 0 0
/dev/block/vold/259:3 /storage/sdcard0 vfat rw,dirsync,nosuid,nodev ...
/dev/block/vold/179:9 /storage/extSdCard vfat rw,dirsync,nosuid,nodev ...
:
:
```

Notice the two last lines there. Both of those are tagged as 'sdcard', and we know one of them has to be our removable one. My money is on the `extSdCard`...

Can you see where we're going with this? 

Good.

Remember to turn on `READ_EXTERNAL_STORAGE` and `WRITE_EXTERNAL_STORAGE` permissions in your `Manifest.xml`, or you won't get the results you expect.

Now we can do a simple call to `System.IO.File.ReadAllText("/proc/mounts")` to read the text out of this file, and store it in a string. We can then parse the string to look for things like  `storage`, `sdcard`, `vfat`, `ext` and anything else we think would be a good indicator. And if we can find __all of that on one file-system line__, then the file system mounted there is very likely a good candidate for our _removable_ SD card.

From the example in this repo:

So, after some hacky string parsing (because quick-and-dirty-hacks are how we roll):

```csharp

var candidateProcMountEntries = procMounts.Split('\n', '\r').ToList();
candidateProcMountEntries.RemoveAll(s => s.IndexOf("storage", StringComparison.OrdinalIgnoreCase) < 0);
var bestCandidate = candidateProcMountEntries
  .FirstOrDefault(s => s.IndexOf("ext", StringComparison.OrdinalIgnoreCase) >= 0
                       && s.IndexOf("sd", StringComparison.OrdinalIgnoreCase) >= 0
                       && s.IndexOf("vfat", StringComparison.OrdinalIgnoreCase) >= 0);

// e.g. /dev/block/vold/179:9 /storage/extSdCard vfat rw,dirsync,nosuid, blah
if (!string.IsNullOrWhiteSpace(bestCandidate))
{
  var sdCardEntries = bestCandidate.Split(' ');
  sdCardEntry = sdCardEntries.FirstOrDefault(s => s.IndexOf("/storage/", System.StringComparison.OrdinalIgnoreCase) >= 0);
  return !string.IsNullOrWhiteSpace(sdCardEntry) ? string.Format("{0}", sdCardEntry) : string.Empty;
}

```

...we end up with `/storage/extSdCard`.

It's quite likely that this may not be quite good enough, because the manufacturers of an HTC or Xiaomi phone might have called it something different (for example if they'd mounted it at `/storage/bob` we'd be pretty much screwed). But that being said, they can't be too free-spirited with it, or the Android OS wouldn't be able to identify it either, and you wouldn't be able to copy your music or movies to it, and then play them. 

OK, so now we've established how to locate the SD card (assuming we have one plugged in!). 

How do we know if it's writeable? We could get all complex and try and figure out what type of user permissions we have, look at attributes on the file system and get all hard-core rock-star-coder complicated. Or we could simply just try and write to that file system (this could involve writing a file, creating a directory, deleting a file, etc.). 


```csharp
public static bool IsWriteable(string pathToTest)
{
  bool result = false;
  const string someTestText = "some test text";
  try
  {
    string testFile = string.Format("{0}/{1}.txt", pathToTest, Guid.NewGuid());
    System.IO.File.WriteAllText(testFile, someTestText);
    System.IO.File.Delete(testFile);
    result = true;
  }
  catch (Exception ex) // argh! did we do something stupid? no? then it's not writeable
  {
    Log.Error("ExternalSDStorageHelper", string.Format("Exception: {0}\r\nMessage: {1}\r\nStack Trace: {2}", ex, ex.Message, ex.StackTrace));
  }
  return result;
}

```


OK, that works. But this is on an Android 4.1.2 device (not a KitKat one). We'll get to the KitKat part later. For now, it's OK, we have a removable SD card, and we can write to it. Hooray!

We're not quite done yet though. The standard Xamarin/Android calls for determining how much total/available/usable/free space is available are limited to only those mount points that Android would let you access. Like the inappropriately and stupidly named `ExternalStorageDirectory`, which isn't what we want.

Back to Linux we go. There's a `statvfs()` call we can make to get some basic info about the mounted file system. But luckily for us, there's an object called `Android.OS.StatFS` which wraps that call for us, which makes it really easy to get to.

I'm interested in total space, available space and free space (the last two aren't always interchangeable), so I made an object called `FileSystemBlockInfo`, which I'll be using to store these bits of information

```csharp
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
```

Nothing special. Moving on... 

Using the StatFS object in `ExternalSdStorageHelper.GetFileSystemBlockInfo()`

```csharp
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
        // you may want to disable warning about obsoletes, earlier versions of Android are using the deprecated versions
        fsbi.BlockSizeBytes = (long)statFs.BlockSize;
        fsbi.TotalSizeBytes = (long)statFs.BlockCount * (long)statFs.BlockSize;
        fsbi.FreeSizeBytes = (long)statFs.FreeBlocks * (long)statFs.BlockSize;
        fsbi.AvailableSizeBytes = (long) statFs.AvailableBlocks * (long)statFs.BlockSize;
      }
      return fsbi;
    }
```

_We have to do the check for the Android version, because the OS calls being made have been deprecated for older versions. The 'old style', pre Android level 18 didn't use the `Long` suffixes, so if you try and call use those on anything below Android 4.3, it'll crash on you, telling you that that those methods are unavailable. Viva fragmentation!_

And that gives us the info we need to be able to track how much space we have, and have available to play with.

One caveat though: __Android KitKat (aka 4.4, aka level 19)__ won't let you __write__ to this removable disk. Ironically, earlier, more primitive versions of Android do. Why did they do this? I'm not really sure. Maybe it's an 'everything you store on our non-removable-poorly-named-externally-available-internal-storage are belong to us!'. Maybe they're tired of removable SD cards. Maybe it's a cost-cutting thing. Maybe they want to try and force people to shift their stuff into Google's cloud. I dunno... maybe I've got the wrong end of the stick this time too.

It's my foot, I should be able to shoot myself in it if I want to, right?


