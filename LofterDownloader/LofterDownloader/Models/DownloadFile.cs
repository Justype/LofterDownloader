using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace LofterDownloader.Models
{
    class DownloadFile : INotifyPropertyChanged
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get => Path.GetFileName(FilePath);
        }
        /// <summary>
        /// 下载的Url
        /// </summary>
        public string Url { get; set; }

        double progress;
        /// <summary>
        /// 下载进度
        /// </summary>
        public double Progress
        {
            get => progress;
            set
            {
                if (progress == value)
                    return;
                progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
