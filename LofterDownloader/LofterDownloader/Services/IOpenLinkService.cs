using System;
using System.Collections.Generic;
using System.Text;

namespace LofterDownloader.Services
{
    public interface IOpenLinkService
    {
        bool OpenLink(string url);
    }
}
