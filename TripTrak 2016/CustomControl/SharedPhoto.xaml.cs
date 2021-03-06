﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TripTrak_2016.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TripTrak_2016.CustomControl
{
    public sealed partial class SharedPhoto : UserControl
    {
        public SharedPhoto()
        {
            this.InitializeComponent();
            PhotoNameTb.DataContextChanged += PhotoNameTb_DataContextChanged;
        }

        public async void getImageSource()
        {
            var source = await PhotoHelper.getImageSource(PhotoNameTb.Text);
            if (source != null)
                PhotoImg.Source = source;
        }

        private void PhotoNameTb_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            getImageSource();
        }
    }
}
