using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LofterDownloader.Models;
using LofterDownloader.Tools;
using Xamarin.Forms;

namespace LofterDownloader.Tools
{
    public class TagDwrDownloader
    {
        private readonly HttpClient client;

        #region Events
        public event EventHandler<DownloadStartArgs> DownloadStarted;
        public event EventHandler<DownloadEndArgs> DownloadEnded;
        public event EventHandler<DownloadingArgs> Downloading;
        public event EventHandler<DoanlodErrorArgs> DownloadError;
        private void OnDownloadStarted()
        {
            DownloadStarted?.Invoke(this, new DownloadStartArgs { Tag = TagName });
        }
        private void OnDownloadEnded()
        {
            DownloadEnded?.Invoke(this, new DownloadEndArgs { Tag = TagName });
        }
        private void OnDownloading(string publishTime)
        {
            Downloading?.Invoke(this, new DownloadingArgs
            {
                Tag = TagName,
                PublishTime = publishTime
            });
        }
        private void OnDownloadError(string errorType)
        {
            DownloadError?.Invoke(this, new DoanlodErrorArgs
            {
                Tag = TagName,
                ErrorType = errorType
            });
        }
        #endregion


        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName { get; set; }
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MinValue;
        public int RequestNum { get; set; } = 0;

        const string url = "http://www.lofter.com/dwr/call/plaincall/TagBean.search.dwr";

        #region 正则匹配
        /// <summary>
        /// 博客所有内容的正则
        /// </summary>
        static readonly Regex blogRegex = new Regex("^s[0-9]+\\.afterPostId.+?\\.blogInfo=(s[0-9]+).+?\\.blogPageUrl=\"(.+?)\".+?\\.content=\"(.*?)\";.+?\\.hot=([0-9]+)(.+?)\\.publishTime=([0-9]+).+?\\.tag=\"(.+?)\".+?\\.title=\"(.*?)\"", RegexOptions.Multiline);
        /// <summary>
        /// 原图的正则
        /// </summary>
        static readonly Regex photoLinkRegex = new Regex("raw\\\\\":\\\\\"(.+?)\\\\");
        /// <summary>
        /// 博客信息：博客昵称的正则
        /// </summary>
        static readonly Regex blogNickNameRegex = new Regex("(s[0-9]+)\\.blogNickName=\\\"(.+?)\\\"");
        /// <summary>
        /// 错误博客图片Url的正则
        /// </summary>
        static readonly Regex WrongUrlRegex = new Regex("//nos.netease.com/(imglf[0-9])/");
        #endregion

        public TagDwrDownloader()
        {
            client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip });
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        /// <returns>博客内容</returns>
        public IEnumerable<Blog> IterateDwrResult()
        {
            if (TagName == "" || StartDate == DateTime.MinValue || EndDate == DateTime.MinValue || RequestNum == 0)
                yield break;
            string publishTime = ValueConvert.DateToTicks(StartDate).ToString();
            int requestNum = RequestNum;
            OnDownloadStarted();
            while (true)
            {
                HttpRequestMessage requestMessage = GetNewHttpRequestMessage(publishTime, requestNum);
                OnDownloading(publishTime);
                string text = "";
                try
                {
                    var response = client.SendAsync(requestMessage).Result;
                    text = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("===============网络错误？");
                    Debug.WriteLine(e);
                    OnDownloadError(e.ToString());
                }
                var blogMatch = blogRegex.Match(text);

                // 构建 BlogInfo : NickName 键值对
                Dictionary<string, string> nickNamesDict = new Dictionary<string, string>();
                if (blogMatch.Success)
                {
                    var nicknameMatches = blogNickNameRegex.Matches(text);
                    foreach (Match match in nicknameMatches)
                        nickNamesDict.Add(match.Groups[1].Value, match.Groups[2].Value);
                }
                else
                {
                    OnDownloadEnded();
                    yield break;    // 退出迭代
                }
                while (true)
                {
                    if (!blogMatch.Success)
                        break;  // 继续下次DWR请求

                    // 时间小于规定值，直接退出
                    DateTime blogPublishTime = ValueConvert.TicksToDate(blogMatch.Groups[6].Value);
                    if (blogPublishTime < EndDate)
                        yield break;

                    #region 博客图片
                    MatchCollection photoLinksMatches = photoLinkRegex.Matches(blogMatch.Groups[5].Value);
                    List<string> photoLinks = new List<string>();
                    foreach (Match match in photoLinksMatches)
                    {
                        if (!photoLinks.Contains(match.Groups[1].Value))
                            photoLinks.Add(match.Groups[1].Value);
                    }
                    for (int i = 0; i < photoLinks.Count; i++)
                        photoLinks[i] = GetConnectableBlogPhotoUrl(photoLinks[i]);
                    #endregion
                    #region 解析HTML
                    string content;
                    Dictionary<string, string> herfLinks = new Dictionary<string, string>();
                    if (blogMatch.Groups[3].Value == "")
                        content = "";
                    else
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(Regex.Unescape(blogMatch.Groups[3].Value));
                        content = doc.DocumentNode.InnerText;
                        var aNodes = doc.DocumentNode.SelectNodes("//a");
                        if (aNodes != null)
                        {
                            int counter = 0;
                            foreach (var aNode in aNodes)
                            {
                                string key = aNode.InnerText;
                                if (herfLinks.ContainsKey(key))
                                {
                                    key += counter;
                                }
                                try
                                {
                                    if (aNode.Attributes["href"] == null)
                                        continue;
                                    herfLinks.Add(key, aNode.Attributes["href"].Value);
                                    counter++;
                                }
                                catch (NullReferenceException)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    #endregion
                    Blog blog = new Blog
                    {
                        BlogNickName = Regex.Unescape(nickNamesDict[blogMatch.Groups[1].Value]),
                        BlogPageUrl = blogMatch.Groups[2].Value,
                        Content = content,
                        Hot = blogMatch.Groups[4].Value,
                        HerfLinks = herfLinks,
                        PhotoLinks = photoLinks,
                        PublishTime = blogPublishTime,
                        Tags = Regex.Unescape(blogMatch.Groups[7].Value),
                        Title = Regex.Unescape(blogMatch.Groups[8].Value)
                    };
                    publishTime = blogMatch.Groups[6].Value;
                    yield return blog;
                    blogMatch = blogMatch.NextMatch();  // 向下匹配Blog
                }
            }
        }

        /// <summary>
        /// DWR返回的网易图库的路径可能有问题
        /// </summary>
        /// <param name="url">网易图库URL</param>
        /// <returns>目前能访问的URL</returns>
        private string GetConnectableBlogPhotoUrl(string url)
        {
            // 先换为 HTTP
            url = url.Replace("https:", "http:").Replace(".nos.netease.com", ".nosdn.127.net");
            Match match = WrongUrlRegex.Match(url);
            if (match.Success)
                url = WrongUrlRegex.Replace(url, "//" + match.Groups[1].Value + ".nosdn.127.net/");
            return url;
        }

        /// <summary>
        /// 构造请求信息
        /// </summary>
        /// <param name="publishTime">发布时间</param>
        /// <param name="order">请求位置</param>
        /// <param name="requestNum">请求个数</param>
        /// <returns></returns>
        private HttpRequestMessage GetNewHttpRequestMessage(string publishTime, int requestNum)
        {
            return new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post,
                Headers =
                {
                    { "Accept", "*/*" },
                    { "Accept-Encoding", "gzip" },
                    { "Accept-Language","zh-CN,zh;q=0.9"},
                    { "Connection","keep-alive"},
                    { "Host","www.lofter.com"},
                    { "Origin","http://www.lofter.com"},
                    { "Referer","http://www.lofter.com/tag/ggad?tab=archive"},
                    { "Sec-Fetch-Dest","empty"},
                    { "Sec-Fetch-Mode","cors"},
                    { "Sec-Fetch-Site","same-origin"},
                    { "User-Agent","Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36" }
                },
                Content = new StringContent("callCount=1\n" +
                                            "scriptSessionId=${scriptSessionId}187\n" +
                                            "httpSessionId=\n" +
                                            "c0-scriptName=TagBean\n" +
                                            "c0-methodName=search\n" +
                                            "c0-id=0\n" +
                                           $"c0-param0=string:{TagName}\n" +
                                            "c0-param1=number:0\n" +
                                            "c0-param2=string:\n" +
                                            "c0-param3=string:new\n" +
                                            "c0-param4=boolean:false\n" +
                                            "c0-param5=number:0\n" +
                                           $"c0-param6=number:{requestNum}\n" +
                                           $"c0-param7=number:10000\n" +
                                           $"c0-param8=number:{publishTime}\n" +
                                            "batchId=123456") // 六位随机数)
            };
        }
    }

    public class DownloadStartArgs : EventArgs
    {
        public string Tag { get; set; }
    }

    public class DownloadingArgs : EventArgs
    {
        public string Tag { get; set; }
        public string PublishTime { get; set; }
    }
    public class DownloadEndArgs : EventArgs
    {
        public string Tag { get; set; }
    }
    public class DoanlodErrorArgs : EventArgs
    {
        public string Tag { get; set; }
        public string ErrorType { get; set; }
    }
}
