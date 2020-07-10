using Android.Content;
using Android.Net;

using LofterDownloader.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(LofterDownloader.Droid.Services.OpenLinkService))]
namespace LofterDownloader.Droid.Services
{
    class OpenLinkService : IOpenLinkService
    {
        public bool OpenLink(string url)
        {
            try
            {
                Intent intent = new Intent(Intent.ActionView);
                intent.SetData(Uri.Parse(url));

                MainActivity.Instance.StartActivity(intent);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}