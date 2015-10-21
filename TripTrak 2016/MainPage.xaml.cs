using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TripTrak_2016.Helpers;
using TripTrak_2016.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TripTrak_2016
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage(Frame frame)
        {
            this.InitializeComponent();
            InitSettings();
            this.ShellSplitView.Content = frame;
            frame.Navigated += Frame_Navigated;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            PageTitleTbl.Text = App.PageName;
            Type pageName = e.SourcePageType;
        }

        private void InitSettings()
        {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            // Read data from a setting in a container
            bool hasGeoSettings = localSettings.Containers.ContainsKey("GeolocatorSettings");

            if (hasGeoSettings)
            {
                LocationHelper.Geolocator.MovementThreshold = Convert.ToDouble(localSettings.Containers["GeolocatorSettings"].Values["MovementThreshold"]);
                LocationHelper.Geolocator.DesiredAccuracyInMeters = Convert.ToUInt32(localSettings.Containers["GeolocatorSettings"].Values["DesiredAccuracyInMeters"]);
                LocationHelper.Geolocator.ReportInterval = Convert.ToUInt32(localSettings.Containers["GeolocatorSettings"].Values["ReportInterval"]);
            }
            else
            {
                LocationHelper.Geolocator.DesiredAccuracy = Windows.Devices.Geolocation.PositionAccuracy.High;
                LocationHelper.Geolocator.ReportInterval = 5000;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            ShellSplitView.IsPaneOpen = false;

            if (ShellSplitView.Content != null)
            {
                App.PageName = radioButton.Content.ToString();
                switch (App.PageName)
                {
                    case "Home":
                        App.PageName = "TripTrak";
                        ((Frame)ShellSplitView.Content).Navigate(typeof(Home));
                        break;
                    case "My Trips":
                        ((Frame)ShellSplitView.Content).Navigate(typeof(MyTrips));
                        break;
                    case "Shared Location":
                        ((Frame)ShellSplitView.Content).Navigate(typeof(About));
                        break;
                    case "About":
                        ((Frame)ShellSplitView.Content).Navigate(typeof(About));
                        break;
                    case "Settings":
                        ((Frame)ShellSplitView.Content).Navigate(typeof(Settings));
                        break;
                    case "Notifications":
                        ((Frame)ShellSplitView.Content).Navigate(typeof(About));
                        break;
                    case "Friends":
                        ((Frame)ShellSplitView.Content).Navigate(typeof(About));
                        break;
                    default:
                        ((Frame)ShellSplitView.Content).Navigate(typeof(About));
                        break;
                }
            }

        }
    }
}
