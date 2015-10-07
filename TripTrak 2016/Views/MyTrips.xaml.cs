using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TripTrak_2016.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyTrips : Page
    {
        LocalDataStorage localData = new LocalDataStorage();
        private ObservableCollection<Trip> allTrips = null;
        public MyTrips()
        {
            this.InitializeComponent();
            CreateTripButton.Click += CreateTripButton_Click;
            this.Loaded += MyTrips_Loaded;
        }

        private async void MyTrips_Loaded(object sender, RoutedEventArgs e)
        {
            allTrips = await localData.GetAllTrip();

            if(allTrips!=null && allTrips.Count>0)
                foreach (Trip item in allTrips)
                {
                    MainText.Text = MainText.Text + item.Name + "; ";
                }
        }

        private void CreateTripButton_Click(object sender, RoutedEventArgs e)
        {
            App.PageName = "Create new trip";
            this.Frame.Navigate(typeof(CreateTrip));
        }
    }
}
