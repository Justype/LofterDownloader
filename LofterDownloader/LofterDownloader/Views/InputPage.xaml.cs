using LofterDownloader.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LofterDownloader.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InputPage : ContentPage
    {
        public InputPage()
        {
            InitializeComponent();
            BindingContext = new InputViewModel
            {
                Page = this
            };
        }
    }
}