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
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TripTrak_2016.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        LocalDataStorage localData = new LocalDataStorage();
        private ObservableCollection<LocationPin> allPins = null;
        public HomeViewModel ViewModel { get; set; }

        public Home()
        {
            this.InitializeComponent();
            this.ViewModel = new HomeViewModel();
            ImageGridView.SelectionChanged += ImageGridView_SelectionChanged;
            HistoryDatePicker.DateChanged += HistoryDatePicker_DateChanged;
            PrevDayButton.Click += PrevDayButton_Click;
            NextDayButton.Click += NextDayButton_Click;
            CheckPointSlider.ValueChanged += CheckPointSlider_ValueChanged;
        }

        private async void CheckPointSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (this.ViewModel.MileStoneLocations.Count > 0)
            {
                var index = Convert.ToInt32(CheckPointSlider.Value);

                if (this.ViewModel.CheckedLocations.Count > 0)
                {
                    var currentLoc = this.ViewModel.CheckedLocations.FirstOrDefault(loc => loc.IsCurrentLocation == true);
                    if (currentLoc != null && currentLoc.IsCurrentLocation)
                        this.ViewModel.CheckedLocations.Remove(currentLoc);
                }
                this.ViewModel.PinDisplayInformation = new LocationPin
                {
                    Position = this.ViewModel.MileStoneLocations[index].Position,
                    IsCurrentLocation = true,
                    DateCreated = this.ViewModel.MileStoneLocations[index].DateCreated
                };
                this.ViewModel.CheckedLocations.Add(this.ViewModel.PinDisplayInformation);
                this.InputMap.Center = this.ViewModel.PinDisplayInformation.Geopoint;
                await LocationHelper.TryUpdateMissingLocationInfoAsync(this.ViewModel.CheckedLocations[this.ViewModel.CheckedLocations.Count - 1], null);
            }
        }

        private void NextDayButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryDatePicker.Date = HistoryDatePicker.Date.AddDays(1);
        }

        private void PrevDayButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryDatePicker.Date = HistoryDatePicker.Date.AddDays(-1);
        }

        private async void HistoryDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            DatePickerSp.IsHitTestVisible = false;
            DatePickerSp.Opacity = 0.5;

            var hasData = await GetPinsForGivenDate();

            DatePickerSp.IsHitTestVisible = true;
            DatePickerSp.Opacity = 1;
        }

        private async Task<bool> GetPinsForGivenDate()
        {
            bool ret = false;
            this.ViewModel.PinnedLocations.Clear();
            this.ViewModel.CheckedLocations.Clear();
            this.ViewModel.MileStoneLocations.Clear();
            CheckPointSlider.Maximum = 0;
            this.ViewModel.PinDisplayInformation = new LocationPin();
            CheckPointSlider.Value = 0;
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
                var numberOfPolylines = await drawPolylines();
                if (this.ViewModel.PinnedLocations.Count > 0)
                    ret = !ret;
                CheckPointSlider.Maximum = this.ViewModel.MileStoneLocations.Count() - 1;
            }
            return ret;
        }

        private async void ImageGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LocationPin loc = (ImageGridView.SelectedItem) as LocationPin;
                await LocationHelper.TryUpdateMissingLocationInfoAsync(loc, null);
                this.ViewModel.PinDisplayInformation = loc;
                this.InputMap.Center = this.ViewModel.PinDisplayInformation.Geopoint;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Loads the saved location data on first navigation, and 
        /// attaches a Geolocator.StatusChanged event handler. 
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            bool getPins = await GetPinsForGivenDate();
            if (e.NavigationMode == NavigationMode.New)
            {

            }
            // Start handling Geolocator and network status changes after loading the data 
            // so that the view doesn't get refreshed before there is something to show.
            LocationHelper.Geolocator.StatusChanged += Geolocator_StatusChanged;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            LocationHelper.Geolocator.PositionChanged += Geolocator_PositionChanged;
            StartLocationExtensionSession();

        }

        /// <summary>
        /// Cancels any in-flight request to the Geolocator, and
        /// disconnects the Geolocator.StatusChanged event handler. 
        /// </summary>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            LocationHelper.CancelGetCurrentLocation();
            LocationHelper.Geolocator.StatusChanged -= Geolocator_StatusChanged;
            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
            LocationHelper.Geolocator.PositionChanged -= Geolocator_PositionChanged;
            StopLocationExtensionSession();
        }

        #region Location Tracking

        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            var _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (args.Position.Coordinate.Accuracy < 55)
                {
                    var item = new LocationPin
                    {
                        Position = args.Position.Coordinate.Point.Position,
                        Speed = args.Position.Coordinate.Speed
                    };
                    this.ViewModel.PinnedLocations.Add(item);
                    await localData.InsertLocationDataAsync(item);
                }
            });
        }


        /// <summary>
        /// Handles the NetworkInformation.NetworkStatusChanged event to refresh the locations 
        /// list if the internet is available, and to display an error message otherwise.
        /// </summary>
        /// <param name="sender"></param>
        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await this.CallOnUiThreadAsync(async () =>
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                bool isNetworkAvailable = profile != null;
                if (isNetworkAvailable) await this.ResetViewAsync();
            });
        }


        /// <summary>
        /// Runs the specified handler on the UI thread at Normal priority. 
        /// </summary>
        private async Task CallOnUiThreadAsync(DispatchedHandler handler) => await
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
        /// <summary>
        /// Handles the Geolocator.StatusChanged event to refresh the map and locations list 
        /// if the Geolocator is available, and to display an error message otherwise.
        /// </summary>

        private async void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            await this.CallOnUiThreadAsync(async () =>
            {
                switch (args.Status)
                {
                    case PositionStatus.Ready:
                        await this.ResetViewAsync();
                        break;
                    case PositionStatus.Initializing:
                        break;
                    case PositionStatus.NoData:
                    case PositionStatus.Disabled:
                    case PositionStatus.NotInitialized:
                    case PositionStatus.NotAvailable:
                    default:
                        await this.ResetViewAsync(false);
                        break;
                }
            });
        }


        private void Session_Revoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            //TODO: clean up session data
            StopLocationExtensionSession();
        }

        private void StopLocationExtensionSession()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }

        }


        private ExtendedExecutionSession session;

        private async void StartLocationExtensionSession()
        {
            session = new ExtendedExecutionSession();
            session.Description = "Location Tracker";
            session.Reason = ExtendedExecutionReason.LocationTracking;
            session.Revoked += Session_Revoked;
            var result = await session.RequestExtensionAsync();
            if (result == ExtendedExecutionResult.Denied)
            {
                //TODO: handle denied
            }
        }
        #endregion
        #region private method
        /// <summary>
        /// Updates the UI to account for the user's current position, if available, 
        /// resetting the MapControl bounds and refreshing the travel info. 
        /// </summary>
        /// <param name="isGeolocatorReady">false if the Geolocator is known to be unavailable; otherwise, true.</param>
        /// <returns></returns>
        private async Task ResetViewAsync(bool isGeolocatorReady = true)
        {
            LocationPin currentLocation = null;
            if (isGeolocatorReady) currentLocation = await this.GetCurrentLocationAsync();
            if (currentLocation != null)
            {
                if (this.ViewModel.CheckedLocations.Count > 0)
                {
                    var currentLoc = this.ViewModel.CheckedLocations.FirstOrDefault(loc => loc.IsCurrentLocation == true);
                    if (currentLoc != null && currentLoc.IsCurrentLocation)
                        this.ViewModel.CheckedLocations.Remove(currentLoc);
                }
                this.ViewModel.PinDisplayInformation = new LocationPin { Position = currentLocation.Position, IsCurrentLocation = true };
                this.ViewModel.CheckedLocations.Add(new LocationPin { Position = currentLocation.Position, IsCurrentLocation = true });
                await LocationHelper.TryUpdateMissingLocationInfoAsync(this.ViewModel.PinDisplayInformation, null);
            }
            // Set the current view of the map control. 
            var positions = this.ViewModel.CheckedLocations.Select(loc => loc.Position).ToList();
            //if (currentLocation != null)
            //    positions.Insert(0, currentLocation.Position);
            await setViewOnMap(positions);
        }

        /// <summary>
        /// Gets the current location if the geolocator is available, 
        /// and updates the Geolocator status message depending on the results.
        /// </summary>
        /// <returns>The current location.</returns>
        private async Task<LocationPin> GetCurrentLocationAsync()
        {
            var currentLocation = await LocationHelper.GetCurrentLocationAsync();
            App.currentLocation = currentLocation;
            return currentLocation;
        }

        private async Task setViewOnMap(List<BasicGeoposition> positions)
        {
            if (positions.Count == 0)
                return;
            var bounds = GeoboundingBox.TryCompute(positions);
            double viewWidth = ApplicationView.GetForCurrentView().VisibleBounds.Width;
            var margin = new Thickness((viewWidth >= 500 ? 300 : 10), 10, 10, 10);
            bool isSuccessful = await this.InputMap.TrySetViewBoundsAsync(bounds, margin, MapAnimationKind.Default);
            if (isSuccessful && positions.Count < 2)
                this.InputMap.ZoomLevel = 15;
            else if (!isSuccessful && positions.Count > 0)
            {
                this.InputMap.Center = new Geopoint(positions[0]);
                this.InputMap.ZoomLevel = 15;
            }
        }

        #endregion

        private async void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;

            string imagePath = "";
            switch (radioButton.Content.ToString())
            {
                case "Arial":
                    this.InputMap.Style = MapStyle.Aerial;
                    MapOptionButton.Flyout.Hide();
                    break;
                case "Road":
                    this.InputMap.Style = MapStyle.Road;
                    MapOptionButton.Flyout.Hide();
                    break;
                case "Take Photo":
                    HistoryDatePicker.Date = DateTime.Now.Date;
                    await this.ResetViewAsync();
                    imagePath = await PhotoHelper.GetPhotoFromCameraLaunch(true);
                    if (imagePath != null)
                    {
                        Dictionary<string, string> LocationInfo = new Dictionary<string, string>();

                        LocationInfo.Add("ImagePath", imagePath);
                        LocationInfo.Add("LocationName", this.ViewModel.PinDisplayInformation.Name);
                        App.PageName = "Post to TripTrak";
                        this.Frame.Navigate(typeof(PostPhoto), LocationInfo);
                    }
                    radioButton.IsChecked = false;
                    CameraButton.Flyout.Hide();
                    break;
                case "Photo Library":
                    imagePath = await PhotoHelper.GetPhotoFromCameraLaunch(false);
                    if (imagePath != null)
                    {
                        Dictionary<string, string> LocationInfo = new Dictionary<string, string>();

                        LocationInfo.Add("ImagePath", imagePath);
                        LocationInfo.Add("LocationName", this.ViewModel.PinDisplayInformation.Name);
                        App.PageName = "Post to TripTrak";
                        this.Frame.Navigate(typeof(PostPhoto), LocationInfo);
                    }
                    radioButton.IsChecked = false;
                    CameraButton.Flyout.Hide();
                    break;
                default:
                    break;
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Tag.ToString())
            {
                case "CurrentLocation":
                    button.IsEnabled = false;
                    if (HistoryDatePicker.Date.Date != DateTime.Now.Date.Date)
                        HistoryDatePicker.Date = DateTime.Now.Date;
                    await this.ResetViewAsync();
                    button.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private async Task<int> drawPolylines()
        {
            int ret = 0;
            //remove all current polylines on map
            this.InputMap.MapElements.Clear();

            //Order points by DateCreated
            var simpleGeoInDateOrder = this.ViewModel.PinnedLocations.OrderBy(x => x.DateCreated).ToList();

            var Coords = new List<BasicGeoposition>();

            //Query Points list to draw Polylines
            for (int i = 0; i < simpleGeoInDateOrder.Count; i++)
            {
                if (simpleGeoInDateOrder[i].IsCheckPoint == true)
                    this.ViewModel.MileStoneLocations.Add(simpleGeoInDateOrder[i]);
                if (Coords.Count == 0)
                    Coords.Add(simpleGeoInDateOrder[i].Position);
                else if (simpleGeoInDateOrder[i].DateCreated - simpleGeoInDateOrder[i - 1].DateCreated < TimeSpan.FromMinutes(5))
                {
                    Coords.Add(simpleGeoInDateOrder[i].Position);
                }
                else
                {
                    //define polyline
                    MapPolyline mapPolyline = new MapPolyline();
                    mapPolyline.StrokeColor = Colors.Black;
                    mapPolyline.StrokeThickness = 2;
                    mapPolyline.StrokeDashed = true;
                    mapPolyline.Path = new Geopath(Coords);

                    //draw polyline on map
                    this.InputMap.MapElements.Add(mapPolyline);
                    ret++;
                    //Clear Coords.
                    Coords.Clear();
                    if (simpleGeoInDateOrder[i].IsCheckPoint == false)
                        this.ViewModel.MileStoneLocations.Add(simpleGeoInDateOrder[i]);
                }
            }
            //draw last Polyline on map
            if (Coords.Count > 1)
            {
                MapPolyline lastPolyline = new MapPolyline();
                lastPolyline.StrokeColor = Colors.Black;
                lastPolyline.StrokeThickness = 2;
                lastPolyline.StrokeDashed = true;
                lastPolyline.Path = new Geopath(Coords);

                //draw polyline on map
                this.InputMap.MapElements.Add(lastPolyline);
                ret++;
            }

            await setViewOnMap(Coords);

            return ret;
        }

        private async void SharedPhoto_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            SharedPhoto tappedPhoto = ((TripTrak_2016.CustomControl.SharedPhoto)sender).DataContext as SharedPhoto;

            if (!string.IsNullOrEmpty(tappedPhoto.ImageName))
            {
                try
                {
                    StorageFile sourcePhoto = await KnownFolders.CameraRoll.GetFileAsync(tappedPhoto.ImageName);
                    IRandomAccessStream stream = await sourcePhoto.OpenAsync(FileAccessMode.Read);

                    if (sourcePhoto != null)
                    {
                        var imgSource = new BitmapImage();
                        imgSource.SetSource(stream);
                        EnlargeImage.Source = imgSource;
                        EnlargeImageGrid.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    //
                }
            }
        }

        private void quitEnlargeButton_Click(object sender, RoutedEventArgs e)
        {
            EnlargeImage.Source = null;
            EnlargeImageGrid.Visibility = Visibility.Collapsed;
        }
    }
}
