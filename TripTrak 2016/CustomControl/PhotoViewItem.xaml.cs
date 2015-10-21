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
        public PhotoViewItem()
        {
            this.InitializeComponent();
            PhotoNameTb.DataContextChanged += PhotoNameTb_DataContextChanged;
        }

        private void PhotoNameTb_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            loadPhoto();
        }

        async void loadPhoto()
        {
            var source = await PhotoHelper.getImageSource(PhotoNameTb.Text);
            if (source != null)
                PhotoImg.Source = source;
        }
    }
}
