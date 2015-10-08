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
    public sealed partial class MyTripsItem : UserControl
    {
        Trip tripItem = null;
        public HomeViewModel ViewModel { get; set; }
        LocalDataStorage localData = new LocalDataStorage();
        private ObservableCollection<LocationPin> allPins = new ObservableCollection<LocationPin>();
        public MyTripsItem()
        {
            this.InitializeComponent();
            this.ViewModel = new HomeViewModel();
            this.Loaded += MyTripsItem_Loaded;
        }

        private async void MyTripsItem_Loaded(object sender, RoutedEventArgs e)
        {
            tripItem = ((MyTripsItem)sender).DataContext as Trip;
            DateTime startTime = tripItem.StartPin.DateCreated.DateTime;
            DateTime endTime = DateTime.Now;
            bool result = await GetPinsForGivenDate(startTime, endTime);
            //     var hasData = await GetPinsForGivenDate();
        }

        private async Task<bool> GetPinsForGivenDate(DateTime startTime, DateTime endTime)
        {
            bool ret = false;

                var pins = allPins.Concat(await localData.GetLocationPinsByDate(startTime));
                allPins = new ObservableCollection<LocationPin>(pins);
             //   startTime.Date.AddDays(1);

            if (allPins != null && allPins.Count > 0)
            {
                foreach (LocationPin pin in allPins)
                {
                    if (pin.IsCheckPoint)
                    {
                        this.ViewModel.CheckedLocations.Add(pin);
                    }
                }
                var numberOfPolylines = ViewModel.drawPolylines(allPins, this.InputMap);

                //if (currentLocation != null)
                //    positions.Insert(0, currentLocation.Position);
                await ViewModel.setViewOnMap(numberOfPolylines, this.InputMap);
            }

            return ret;
        }
    }
}
