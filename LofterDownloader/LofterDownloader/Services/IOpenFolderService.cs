using System;
using System.Collections.Generic;
using System.Text;

namespace LofterDownloader.Services
{
    public interface IOpenFolderService
    {
        bool OpenFolder(string path);
    }
}
