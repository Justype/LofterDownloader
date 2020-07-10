using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LofterDownloader.Services;
using Microsoft.Win32;
using Xamarin.Forms;

using Ookii.Dialogs.Wpf;

[assembly: Dependency(typeof(LofterDownloader.WPF.Services.ChooseFolderService))]
namespace LofterDownloader.WPF.Services
{
    class ChooseFolderService : IChooseFolderService
    {
        public string GetFolder()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if(dialog.ShowDialog() ==true)
            {
                return dialog.SelectedPath;
            }
            return "";
        }
    }
}
