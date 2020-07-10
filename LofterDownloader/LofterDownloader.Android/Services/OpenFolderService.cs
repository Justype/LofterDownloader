using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.Provider;

using LofterDownloader.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(LofterDownloader.Droid.Services.OpenFolderService))]
namespace LofterDownloader.Droid.Services
{
    class OpenFolderService : IOpenFolderService
    {
        public bool OpenFolder(string path)
        {
            try
            {
                Uri uri = Uri.Parse(path);

                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, DocumentsContract.Document.MimeTypeDir);

                MainActivity.Instance.StartActivity(intent);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine("==============打开文件夹错误" + e);
                return false;
            }
            return true;
        }
    }
}