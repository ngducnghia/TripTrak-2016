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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TripTrak_2016.CustomControl
{
    public partial class MyTripsItem : UserControl
    {
        Trip tripItem = null;
        public HomeViewModel ViewModel { get; set; }
        LocalDataStorage localData = new LocalDataStorage();
        private ObservableCollection<LocationPin> allPins = new ObservableCollection<LocationPin>();
        public event RoutedEventHandler EndTripClick;

        public MyTripsItem()
        {
            this.InitializeComponent();
            this.ViewModel = new HomeViewModel();
            this.Loaded += MyTripsItem_Loaded;
            endTripBtn.Click += EndTripBtn_Click;
        }

        public void EndTripBtn_Click(object sender, RoutedEventArgs e)
        {
            //bubble the event up to the parent
            if (this.EndTripClick != null)
                this.EndTripClick(tripItem, e);
        }

        private async void MyTripsItem_Loaded(object sender, RoutedEventArgs e)
        {
            tripItem = ((MyTripsItem)sender).DataContext as Trip;
            if (tripItem != null)
            {
                DateTime startTime = tripItem.StartPin.DateCreated.DateTime;
                DateTime endTime = DateTime.Now;
                if (tripItem.EndPin != null)
                    endTime = tripItem.EndPin.DateCreated.DateTime;
                bool result = await GetPinsForGivenDate(startTime, endTime);
                if(string.IsNullOrEmpty(tripItem.StartPin.Name))
                {
                    await LocationHelper.TryUpdateMissingLocationInfoAsync(tripItem.StartPin, null);
                }
            }
        }

        private async Task<bool> GetPinsForGivenDate(DateTime startTime, DateTime endTime)
        {
            bool ret = false;
            while (startTime.Date.Date <= endTime.Date.Date)
            {
                var pins = await localData.GetLocationPinsByDate(startTime);
                if (pins != null && pins.Count > 0)
                {
                    foreach (LocationPin pin in pins)
                    {
                        if (pin.IsCheckPoint)
                        {
                            this.ViewModel.CheckedLocations.Add(pin);
                        }
                    }
                }
                startTime = startTime.AddDays(1);
            }

            var numberOfPolylines = ViewModel.drawPolylines(this.ViewModel.CheckedLocations, this.InputMap);
            await ViewModel.setViewOnMap(numberOfPolylines, this.InputMap);
            return ret;
        }
    }
}
