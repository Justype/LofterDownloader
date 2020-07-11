using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(LofterDownloader.Droid.Renderers.SelectAllEntryRenderer))]
namespace LofterDownloader.Droid.Renderers
{
    class SelectAllEntryRenderer : EntryRenderer
    {
        public SelectAllEntryRenderer(Context context) : base(context) { }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if(e.OldElement == null)
            {
                Control.SetSelectAllOnFocus(true);
            }
        }
    }
}