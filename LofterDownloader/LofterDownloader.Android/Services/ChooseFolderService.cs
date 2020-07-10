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
            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;
        }
    }
}