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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
            App.PageName = "Start Trip";
            this.Frame.Navigate(typeof(CreateTrip));
        }

        private void MyTripsItem_EndTripClick(object sender, RoutedEventArgs e)
        {
            if (!App.PageName.Equals("End Trip"))
            {
                App.PageName = "End Trip";
                this.Frame.Navigate(typeof(CreateTrip), sender);
            }
        }

        private async void MyTripsItem_DelTripClick(object sender, RoutedEventArgs e)
        {
            var tripItem = sender as Trip;
            ViewModel.CompletedTrips.Remove(tripItem);
            await localData.DeleteTrip(tripItem);
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var list = sender as ListView;
            var tripItem = list.SelectedItem as Trip;
            App.PageName = tripItem.Name;
            this.Frame.Navigate(typeof(Home), tripItem);
        }
    }
}
