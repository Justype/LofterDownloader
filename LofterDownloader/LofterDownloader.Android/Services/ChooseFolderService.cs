using Android.OS;

using LofterDownloader.Services;
using Xamarin.Forms;

[assembly:Dependency(typeof(LofterDownloader.Droid.Services.ChooseFolderService))]
namespace LofterDownloader.Droid.Services
{
    public class ChooseFolderService : IChooseFolderService
    {
        public string GetFolder()
        {
            string path = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;
            path = System.IO.Path.Combine(path, "乐乎下载");
            return path;
        }
    }
}