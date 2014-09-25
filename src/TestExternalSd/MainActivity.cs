using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using TestExternalSd.StorageClasses;

namespace TestExternalSd
{
  [Activity(Label = "TestExternalSSD", MainLauncher = true, Icon = "@drawable/icon")]
  public class MainActivity : Activity
  {
    private Button _btnCheckDrives;
    private TextView _txtExtSdCardPath;
    private TextView _txtProcMounts;
    private TextView _txtExtSdCardUsage;

    protected override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);

      // Set our view from the "main" layout resource
      SetContentView(Resource.Layout.Main);

      _btnCheckDrives = FindViewById<Button>(Resource.Id.btnCheckDrives);
      _txtExtSdCardPath = FindViewById<TextView>(Resource.Id.txtExtSDCardPath);
      _txtProcMounts = FindViewById<TextView>(Resource.Id.txtProcMounts);
      _txtExtSdCardUsage = FindViewById<TextView>(Resource.Id.txtExtSdCardUsage);
      _btnCheckDrives.Click += _btnCheckDrives_Click;
    }

    void _btnCheckDrives_Click(object sender, EventArgs e)
    {
      bool externalSdCardExists = ExternalSdCardInfo.ExternalSdCardExists;
      if (externalSdCardExists)
      {
        _txtExtSdCardPath.Text = ExternalSdCardInfo.Path;
        var fsbi = ExternalSdCardInfo.FileSystemBlockInfo;
        const long gigabytes = 1024*1024*1024;
        string externalSdUsage =
          new StringBuilder().AppendFormat("Total Size: {0:##.00}GB", fsbi.TotalSizeBytes/gigabytes).AppendLine()
            .AppendFormat("Available Size: {0:###.00}GB", fsbi.AvailableSizeBytes/gigabytes).AppendLine()
            .AppendFormat("Free Size: {0:###.00}GB", fsbi.FreeSizeBytes/gigabytes).AppendLine()
            .ToString();

        string isWriteable = ExternalSdCardInfo.IsWriteable.ToString();

        _txtExtSdCardUsage.Text = string.Format("Is writeable: {0}\r\nUsage:{1}", isWriteable, externalSdUsage);
      }
      else
      {
        _txtExtSdCardPath.Text = "(No external SD card found, sorry. Doesn't mean there isn't one, we just couldn't find it based on our criteria)";
      }

      string procmounts = ExternalSdStorageHelper.GetProcMountsContents();
      _txtProcMounts.Text = string.IsNullOrWhiteSpace(procmounts) ? "Couldn't read /proc/mounts" : procmounts;
    }
  }
}

