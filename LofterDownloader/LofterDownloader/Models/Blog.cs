using LofterDownloader.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LofterDownloader.Models
{
    public class Blog
    {
        /// <summary>
        /// 博客地址
        /// </summary>
        public string BlogPageUrl { get; set; }
        /// <summary>
        /// 博客标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 博客内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 博客发布时间
        /// </summary>
        public DateTime PublishTime { get; set; }
        /// <summary>
        /// 博客的所有图片
        /// </summary>
        public List<string> PhotoLinks { get; set; }
        /// <summary>
        /// 外链的键值对
        /// </summary>
        public Dictionary<string, string> HerfLinks { get; set; }
        /// <summary>
        /// 博客热度
        /// </summary>
        public string Hot { get; set; }
        /// <summary>
        /// 博客作者的昵称
        /// </summary>
        public string BlogNickName { get; set; }
        /// <summary>
        /// 博客的Tags
        /// </summary>
        public string Tags { get; set; }

        public override string ToString()
        {
            return $@"标题：{Title}
昵称：{BlogNickName}
热度：{Hot}
Tag：{Tags}
发布时间：{PublishTime:F}
Url：{BlogPageUrl}";
        }
        public string FileName
        {
            get => IOTools.ValidateFileName(BlogNickName) +
                   "_" + IOTools.ValidateFileName(Title) +
                   "_" + PublishTime.ToString("yyyy-MM-dd-HHmmss");
        }
        public string FullTxt
        {
            get
            {

                List<string> herfStrings = new List<string>();
                foreach (var herf in HerfLinks)
                {
                    herfStrings.Add(herf.Key);
                    herfStrings.Add(herf.Value);
                }
                return $@"标题：{Title}
昵称：{BlogNickName}
热度：{Hot}
Tag：{Tags}
发布时间：{PublishTime:F}
Url：{BlogPageUrl}
内容：
{Content}
==================
外链：
{string.Join("\n", herfStrings)}
博客图像：
{string.Join("\n", PhotoLinks)}";
            }
        }
    }
}
