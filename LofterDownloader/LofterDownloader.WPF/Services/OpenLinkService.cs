using LofterDownloader.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(LofterDownloader.WPF.Services.OpenLinkService))]
namespace LofterDownloader.WPF.Services
{
    class OpenLinkService : IOpenLinkService
    {
        public bool OpenLink(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
