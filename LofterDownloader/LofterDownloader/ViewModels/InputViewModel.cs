using LofterDownloader.Models;
using LofterDownloader.Services;
using LofterDownloader.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LofterDownloader.ViewModels
{
    class InputViewModel : BaseViewModel
    {
        #region 输入相关的属性
        string tags = string.Empty;
        string firstTag = string.Empty;
        public string Tags
        {
            get => tags;
            set
            {
                tags = value.ToLower();
                firstTag = tags.Split(' ')[0];
                OnPropertyChanged();
                DownloadPath = Path.Combine(mainPath, firstTag);
                OnPropertyChanged("DownloadPath");
            }
        }
        string mainPath;
        public int MinHot { get; set; } = 0;
        string ignoreTags = string.Empty;
        public string IgnoreTags
        {
            get => ignoreTags;
            set
            {
                ignoreTags = value.ToLower();
                OnPropertyChanged();
            }
        }
        public int MinBlogLength { get; set; } = 0;
        bool isDownloadBlogImg;
        public bool IsDownloadBlogImg
        {
            get => isDownloadBlogImg;
            set
            {
                isDownloadBlogImg = value;
                App.Current.Properties["IsDownloadBlogImg"] = value;
            }
        }
        bool isDownloadLinkImg;
        public bool IsDownloadLinkImg
        {
            get => isDownloadLinkImg;
            set
            {
                isDownloadLinkImg = value;
                App.Current.Properties["IsDownloadLinkImg"] = value;
            }
        }
        bool isDownloadBlogContent;
        public bool IsDownloadBlogContent
        {
            get => isDownloadBlogContent;
            set
            {
                isDownloadBlogContent = value;
                App.Current.Properties["IsDownloadBlogContent"] = value;
            }
        }
        bool isDownloadBlogWhileItHasImg;
        public bool IsDownloadBlogWhileItHasImg
        {
            get => isDownloadBlogWhileItHasImg;
            set
            {
                isDownloadBlogWhileItHasImg = value;
                App.Current.Properties["IsDownloadBlogWhileItHasImg"] = value;
            }
        }
        bool isDownloadLongBlogImg;
        public bool IsDownloadLongBlogImg
        {
            get => isDownloadLongBlogImg;
            set
            {
                isDownloadLongBlogImg = value;
                App.Current.Properties["IsDownloadLongBlogImg"] = value;
                OnPropertyChanged();
            }
        }
        int longBlogLength;
        public int LongBlogLength
        {
            get => longBlogLength;
            set
            {
                longBlogLength = value;
                App.Current.Properties["LongBlogLength"] = value;
            }
        }
        bool isSortByAuthor;
        public bool IsSortByAuthor
        {
            get => isSortByAuthor;
            set
            {
                isSortByAuthor = value;
                App.Current.Properties["IsSortByAuthor"] = value;
            }
        }
        public string DownloadPath { get; private set; }
        public DateTime StartDate { get; set; } = DateTime.Now.Date.AddDays(1);
        public DateTime EndDate { get; set; } = new DateTime(2012, 3, 1);
        #endregion

        public Command OpenFolderCommand { get; }
        public Command StartDownloadCommand { get; }

        #region 下载时的字段
        TagDwrDownloader downloader;
        ParallelOptions downloadParallelOptions;
        #endregion

        #region 下载相关的属性
        static readonly Regex imgRegex = new Regex("https?://.+?\\.(png|jpg|jpeg|gif)");
        List<DownloadFile> ReadyToDownloadFiles { get; } = new List<DownloadFile>();
        public ObservableCollection<DownloadFile> DownloadingFiles { get; } = new ObservableCollection<DownloadFile>();
        public List<DownloadFile> FailToDownloadFiles { get; } = new List<DownloadFile>();
        #endregion

        string promptText;
        public string PromptText
        {
            get => promptText;
            set => SetProperty(ref promptText, value);
        }

        public InputViewModel()
        {
            LoadSettings();

            #region 读取Property
            // 由于装箱拆箱降低性能，减少读取次数
            isDownloadBlogImg = (bool)App.Current.Properties["IsDownloadBlogImg"];
            isDownloadLinkImg = (bool)App.Current.Properties["IsDownloadLinkImg"];
            isDownloadBlogContent = (bool)App.Current.Properties["IsDownloadBlogContent"];
            isDownloadBlogWhileItHasImg = (bool)App.Current.Properties["IsDownloadBlogWhileItHasImg"];
            isSortByAuthor = (bool)App.Current.Properties["IsSortByAuthor"];
            isDownloadLongBlogImg = (bool)App.Current.Properties["IsDownloadLongBlogImg"];
            longBlogLength = (int)App.Current.Properties["LongBlogLength"];
            #endregion

            #region DWR下载配置
            downloader = new TagDwrDownloader
            {
                RequestNum = 100
            };
            downloader.DownloadStarted += (sender, e) =>
            {
                PromptText = e.Tag + "：开始下载";
            };
            downloader.DownloadEnded += (sender, e) =>
            {
                PromptText = e.Tag + "：DWR读取结束";
            };
            downloader.Downloading += (sender, e) =>
            {
                PromptText = $"开始请求DWR：时间{ValueConvert.TicksToDate(e.PublishTime):F}";
            };
            downloader.DownloadError += (sender, e) =>
            {
                PromptText = "DWR请求失败";
            };
            #endregion


            #region Commands
            OpenFolderCommand = new Command(() =>
            {
                // 尝试打开具体的目录，如果失败，选择主目录
                if (!DependencyService.Get<IOpenFolderService>().OpenFolder(DownloadPath))
                    DependencyService.Get<IOpenFolderService>().OpenFolder(mainPath);
            });

            StartDownloadCommand = new Command(async () =>
            {
                if (DownloadPath == string.Empty)
                    return;
                if (firstTag == string.Empty)
                {
                    await Page.DisplayAlert("Tags为空", "请输入想下载的Tag", "确定");
                    return;
                }
#if WPF
                // WPF 不需要请求权限
#else
                if (PermissionStatus.Granted != await Permissions.CheckStatusAsync<Permissions.StorageWrite>())
                    if (PermissionStatus.Granted != await Permissions.RequestAsync<Permissions.StorageWrite>())
                        return;
#endif

                Task.Run(StartDownloading);
            });
            #endregion

            // 监听设置的变化
            MessagingCenter.Subscribe<SettingViewModel, string>(this, "SettingChanged", (sender, e) =>
            {
                LoadSettings();
            });
        }
        /// <summary>
        /// 读取基本设置
        /// </summary>
        private void LoadSettings()
        {
            mainPath = Application.Current.Properties["MainPath"] as string;
            DownloadPath = Path.Combine(mainPath, firstTag);
            OnPropertyChanged("DownloadPath");
        }


        private void StartDownloading()
        {
            IsBusy = true;
            DownloadingFiles.Clear();
            FailToDownloadFiles.Clear();
            downloader.TagName = firstTag;
            downloader.StartDate = StartDate;
            downloader.EndDate = EndDate;

            bool isMoreThanOneTag = firstTag != tags;
            string[] tagsArray = isMoreThanOneTag ? tags.Split(' ') : null;
            bool hasIgnoreTag = IgnoreTags != string.Empty;

            downloadParallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = (int)Application.Current.Properties["MaxDegreeOfParallelism"]
            };
            Parallel.ForEach(downloader.IterateDwrResult(), downloadParallelOptions, blog =>
            {
                #region 判断
                if (MinHot > int.Parse(blog.Hot))
                    return;
                if (isMoreThanOneTag)
                {
                    string blogTags = blog.Tags.ToLower();
                    foreach (string tag in tagsArray)
                        if (!blogTags.Contains(tag))
                            return;
                }
                if (hasIgnoreTag)
                {
                    foreach (string tag in blog.Tags.ToLower().Split(','))
                        if (hasIgnoreTag && ignoreTags.Contains(tag))
                            // 如果不想要的tags包含，跳过
                            return;
                }
                #endregion
                #region 保存博客
                string directoryPath = IsSortByAuthor ? Path.Combine(DownloadPath, IOTools.ValidateFileName(blog.BlogNickName)) : DownloadPath;
                string savePath = Path.Combine(directoryPath, blog.FileName);
                string filePath = savePath + ".txt";
                if (IsDownloadBlogContent       // 是否下载博客
                    && !File.Exists(filePath)       // 是否已经保存过
                    && blog.Content.Length > MinBlogLength  // 长度满足要求
                    && (IsDownloadBlogWhileItHasImg || blog.PhotoLinks.Count == 0)) // 跳过有图片的博客
                {
                    IOTools.CheckDirectory(directoryPath);
                    File.WriteAllText(filePath, blog.FullTxt);
                }
                #endregion
                if (!IsDownloadLongBlogImg && blog.Content.Length > LongBlogLength)
                    return;
                #region 将博客图片加入下载队列
                if (IsDownloadBlogImg && blog.PhotoLinks.Count != 0)
                {
                    IOTools.CheckDirectory(directoryPath);
                    for (int i = 0; i < blog.PhotoLinks.Count; i++)
                    {
                        string photoPath = savePath + " " + i + Path.GetExtension(blog.PhotoLinks[i]);
                        if (File.Exists(photoPath))
                            continue;
                        // 将博客图片加入下载队列
                        ReadyToDownloadFiles.Add(new DownloadFile
                        {
                            FilePath = photoPath,
                            Url = blog.PhotoLinks[i].Replace("https", "http")
                        });
                    }
                }
                #endregion
                #region 将外链图片加入下载队列
                if (IsDownloadLinkImg && blog.HerfLinks.Count != 0)
                {
                    int counter = 0;
                    IOTools.CheckDirectory(directoryPath);
                    foreach (var herf in blog.HerfLinks)
                    {
                        Match match = imgRegex.Match(herf.Value);
                        if (!match.Success)
                            continue;
                        string herfName = herf.Key;
                        if (herfName.Length > 10)
                        {
                            herfName = "外链图片" + counter;
                            counter++;
                        }
                        else
                            herfName = IOTools.ValidateFileName(herfName);
                        string photoPath = savePath + " " + herfName + "." + match.Groups[1].Value;
                        if (File.Exists(photoPath))
                            continue;
                        // 将博客图片加入下载队列
                        ReadyToDownloadFiles.Add(new DownloadFile
                        {
                            FilePath = photoPath,
                            Url = match.Value.Replace("https", "http")
                        });
                    }
                }
                #endregion
            });

            StartDownloadFiles();
            foreach (DownloadFile file in FailToDownloadFiles)
                DownloadingFiles.Add(file);
            PromptText = Tags + "：下载结束," + FailToDownloadFiles.Count + "下载失败";
            IsBusy = false;
        }
        /// <summary>
        /// 多线程下载图片
        /// </summary>
        private void StartDownloadFiles()
        {
            Parallel.For(0, ReadyToDownloadFiles.Count, downloadParallelOptions,
            index =>
            {
                if (File.Exists(ReadyToDownloadFiles[index].FilePath))
                    return;
                // WebClient 线程不安全，所以每次下载新建实例
                using (WebClient client = new WebClient())
                {
                    var file = ReadyToDownloadFiles[index];

                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        file.Progress = e.ProgressPercentage / 100d;
                    };
                    client.DownloadFileCompleted += (sender, e) =>
                    {
                        DownloadingFiles.Remove(file);
                    };
                    try
                    {
                        DownloadingFiles.Insert(0, file);
                        client.DownloadFileTaskAsync(new Uri(file.Url), file.FilePath).Wait();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("下载错误\n" + e);
                        IOTools.DeleteFile(file.FilePath);
                        DownloadingFiles.Remove(file);
                        FailToDownloadFiles.Add(file);
                    }
                }
            });

            // 清空集合
            ReadyToDownloadFiles.Clear();
            DownloadingFiles.Clear();
        }

    }
}
