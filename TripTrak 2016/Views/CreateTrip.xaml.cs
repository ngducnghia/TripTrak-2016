using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class CreateTrip : Page
    {
        public HomeViewModel ViewModel { get; set; }
        LocalDataStorage localData = new LocalDataStorage();
        private ObservableCollection<LocationPin> allPins = null;
        LocationPin oldPin;
        public CreateTrip()
        {
            this.InitializeComponent();
            this.ViewModel = new HomeViewModel();
            HistoryDatePicker.DateChanged += HistoryDatePicker_DateChanged;
            createButton.Click += CreateButton_Click;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var item = new Trip
            {
                Name = NameTb.Text,
                Description = DescTb.Text,
                ShareWith = shareTb.Text,
                Type = "On-going",
                StartPin = oldPin
            };
            await localData.CreateNewTrip(item);
            this.Frame.GoBack();
        }


        /// <summary>
        /// Loads the saved location data on first navigation, and 
        /// attaches a Geolocator.StatusChanged event handler. 
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            bool getPins = await GetPinsForGivenDate();
            if (this.ViewModel.CheckedLocations.Count > 0)
            {
                oldPin = this.ViewModel.CheckedLocations[0];
                oldPin.IsSelected = true;
                selectedImage.Source = await PhotoHelper.getImageSource(this.ViewModel.CheckedLocations[0].Photo.ImageName);
            }
            else
                oldPin = this.ViewModel.PinnedLocations[0];

            if (e.NavigationMode == NavigationMode.New)
            {

            }
        }

        private async void checkedPinBtn_Click(object sender, RoutedEventArgs e)
        {
            LocationPin btnPin = ((Button)sender).DataContext as LocationPin;
            oldPin.IsSelected = false;
            oldPin = btnPin;
            oldPin.IsSelected = true;
            await LocationHelper.TryUpdateMissingLocationInfoAsync(btnPin, null);
            selectedImage.Source = await PhotoHelper.getImageSource(btnPin.Photo.ImageName);
        }

        private async void HistoryDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            HistoryDatePicker.IsHitTestVisible = false;
            HistoryDatePicker.Opacity = 0.5;

            var hasData = await GetPinsForGivenDate();

            HistoryDatePicker.IsHitTestVisible = true;
            HistoryDatePicker.Opacity = 1;
        }

        private async Task<bool> GetPinsForGivenDate()
        {
            bool ret = false;
            this.ViewModel.CheckedLocations.Clear();
            this.ViewModel.PinnedLocations.Clear();
            this.InputMap.MapElements.Clear();
            allPins = await localData.GetLocationPinsByDate(HistoryDatePicker.Date.Date);
            if (allPins != null && allPins.Count > 0)
            {
                foreach (LocationPin pin in allPins)
                {
                    if (pin.DateCreated.Date == HistoryDatePicker.Date.Date)
                    {
                        this.ViewModel.PinnedLocations.Add(pin);
                        if (pin.IsCheckPoint)
                        {
                            this.ViewModel.CheckedLocations.Add(pin);
                        }
                    }
                }
                var numberOfPolylines = ViewModel.drawPolylines(this.ViewModel.PinDisplayInformation, true, this.InputMap);
                if (this.ViewModel.PinnedLocations.Count > 0)
                    ret = !ret;

                // Set the current view of the map control. 
                var positions = this.ViewModel.PinnedLocations.Select(loc => loc.Position).ToList();
                //if (currentLocation != null)
                //    positions.Insert(0, currentLocation.Position);
                await ViewModel.setViewOnMap(positions, this.InputMap);
            }
            return ret;
        }
    }
}
