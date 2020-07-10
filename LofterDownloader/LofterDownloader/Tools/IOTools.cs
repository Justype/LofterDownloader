using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LofterDownloader.Tools
{
    public static class IOTools
    {
        
        static readonly Regex renameRegex = new Regex("[:<>\\?\\*\\|\\\\/\t\n\r\0]+|[ \\.]+$|^[\\.]+");
        /// <summary>
        /// 去除非法的文件名字符
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>合法的文件名</returns>
        public static string ValidateFileName(string filename)
        {
            return renameRegex.Replace(filename, "_");
        }
        /// <summary>
        /// 检查文件夹，如果不存在，创建
        /// </summary>
        /// <param name="path">文件夹路径</param>
        public static void CheckDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
