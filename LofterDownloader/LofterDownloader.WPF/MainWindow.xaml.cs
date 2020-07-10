using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace LofterDownloader.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();

            Forms.Init();
            LoadApplication(new LofterDownloader.App());

            
        }
    }
}
