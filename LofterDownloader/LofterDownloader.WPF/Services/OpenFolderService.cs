using System;
using System.Diagnostics;

using LofterDownloader.Services;
using Xamarin.Forms;


[assembly: Dependency(typeof(LofterDownloader.WPF.Services.OpenFolderService))]
namespace LofterDownloader.WPF.Services
{
    class OpenFolderService : IOpenFolderService
    {
        public bool OpenFolder(string path)
        {
            try
            {
                Process.Start(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("打开文件夹错误" + e);
                return false;
            }
        }
    }
}
