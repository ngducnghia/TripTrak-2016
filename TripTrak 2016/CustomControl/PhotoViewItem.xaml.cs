using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TripTrak_2016.Helpers;
using TripTrak_2016.Model;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TripTrak_2016.CustomControl
{
    public sealed partial class PhotoViewItem : UserControl
    {
        LocationPin pinPhoto = null;
        public PhotoViewItem()
        {
            this.InitializeComponent();
            this.Loaded += PhotoViewItem_Loaded;
        }

        private async void PhotoViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            pinPhoto = ((PhotoViewItem)sender).DataContext as LocationPin;
            if (pinPhoto != null && pinPhoto.Photo!=null)
            {
                var source = await PhotoHelper.getImageSource(pinPhoto.Photo.ImageName);
                if (source != null)
                    PhotoImg.Source = source;
            }
        }
    }
}
