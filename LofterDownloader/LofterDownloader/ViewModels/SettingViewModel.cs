using LofterDownloader.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace LofterDownloader.ViewModels
{
    class SettingViewModel : BaseViewModel
    {
        public string MainPath
        {
            get => Application.Current.Properties["MainPath"] as string;
        }
        /// <summary>
        /// 下载线程数
        /// </summary>
        public int MaxDegreeOfParallelism
        {
            get => (int)Application.Current.Properties["MaxDegreeOfParallelism"];
            set
            {
                Application.Current.Properties["MaxDegreeOfParallelism"] = value;
                OnPropertyChanged();
            }
        }
        public Command OpenRepositoryCommand { get; }
        public Command ChangeMainPathCommand { get; }
        public SettingViewModel()
        {
            ChangeMainPathCommand = new Command(() =>
            {
                string folderPath = DependencyService.Get<IChooseFolderService>().GetFolder();
                if (folderPath == "")
                    return;
                Application.Current.Properties["MainPath"] = Path.Combine(folderPath, "乐乎下载");
                OnSettingChanged("MainPath");
            });
            OpenRepositoryCommand = new Command(() =>
            {
                DependencyService.Get<IOpenLinkService>().OpenLink("https://github.com/justype/LofterDownloader");
            });
        }

        private void OnSettingChanged(string name)
        {
            MessagingCenter.Send(this, "SettingChanged", name);
            OnPropertyChanged(name);
        }
    }
}
