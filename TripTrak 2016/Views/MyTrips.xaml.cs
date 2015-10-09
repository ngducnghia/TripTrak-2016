using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TripTrak_2016.Helpers;
using TripTrak_2016.Model;
using TripTrak_2016.ViewModels;
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
        public MyTripsViewModel ViewModel { get; set; }
        public MyTrips()
        {
            this.InitializeComponent();
            this.ViewModel = new MyTripsViewModel();
            CreateTripButton.Click += CreateTripButton_Click;
            this.Loaded += MyTrips_Loaded;

        }

        private async void MyTrips_Loaded(object sender, RoutedEventArgs e)
        {
            var trips = await localData.GetAllTrip();
            foreach (Trip trip in trips)
            {
                if (trip.Type == "On-going")
                    this.ViewModel.OnGoingTrips.Add(trip);
                if (trip.Type == "Completed")
                    this.ViewModel.CompletedTrips.Add(trip);
            }
        }

        private void CreateTripButton_Click(object sender, RoutedEventArgs e)
        {
            App.PageName = "Create new trip";
            this.Frame.Navigate(typeof(CreateTrip));
        }

        private async void MyTripsItem_EndTripClick(object sender, RoutedEventArgs e)
        {
            var tripItem = sender as Trip;

            tripItem.EndPin = new LocationPin
            {
                Name = "endTrip"
            };
            tripItem.Type = "Completed";
            await localData.EditTrip(tripItem);

        }
    }
}
