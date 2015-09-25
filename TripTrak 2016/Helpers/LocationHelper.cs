using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TripTrak_2016.Model;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace TripTrak_2016.Helpers
{
    public static class LocationHelper
    {
        /// <summary>
        /// Gets the Geolocator singleton used by the LocationHelper.
        /// </summary>
        public static Geolocator Geolocator { get; } = new Geolocator();

        /// <summary>
        /// Gets or sets the CancellationTokenSource used to enable Geolocator.GetGeopositionAsync cancellation.
        /// </summary>
        private static CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Initializes the LocationHelper. 
        /// </summary>
        static LocationHelper()
        {
            // TODO Replace the placeholder string below with your own Bing Maps key from https://www.bingmapsportal.com
            MapService.ServiceToken = "0KEyuXXhxVaOAIXwgvLA~doIeC1quhF2yDcbmlTYc6Q~AqLMibILNUu-ftoKwCf3whcS97LHNcNfVxjhwPQUQiMtbTSuQJHkbZeYm-z-05UY";
        }

        public static async Task<MapRoute> getRoute(LocationPin destinationPin)
        {
            var currenLoc = await GetCurrentLocationAsync();
            var routeResultTask = MapRouteFinder.GetDrivingRouteAsync(
               currenLoc.Geopoint, destinationPin.Geopoint,
               MapRouteOptimization.TimeWithTraffic, MapRouteRestrictions.None);
            MapRouteFinderResult routeResult = await routeResultTask;

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                return routeResult.Route;
            }
            return null;
        }

        public static BasicGeoposition getRandomLocation(BasicGeoposition inputLocation)
        {
            int latitudeRange = 36000;
            int longitudeRange = 53000;
            var random = new Random();
            Func<int, double, double> getCoordinate = (range, midpoint) =>
                (random.Next(range) - (range / 2)) * 0.00001 + midpoint;

            BasicGeoposition ret = new BasicGeoposition
            {
                Latitude = getCoordinate(latitudeRange, inputLocation.Latitude),
                Longitude = getCoordinate(longitudeRange, inputLocation.Longitude)
            };
            return ret;
        }

        /// <summary>
        /// Gets the current location if the geolocator is available.
        /// </summary>
        /// <returns>The current location.</returns>
        public static async Task<LocationPin> GetCurrentLocationAsync()
        {
            try
            {
                // Request permission to access the user's location.
                var accessStatus = await Geolocator.RequestAccessAsync();

                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:

                        LocationHelper.CancellationTokenSource = new CancellationTokenSource();
                        var token = LocationHelper.CancellationTokenSource.Token;

                        Geoposition position = await Geolocator.GetGeopositionAsync().AsTask(token);
                        return new LocationPin { Position = position.Coordinate.Point.Position };

                    case GeolocationAccessStatus.Denied:
                    case GeolocationAccessStatus.Unspecified:
                    default:
                        return null;
                }
            }
            catch (TaskCanceledException)
            {
                // Do nothing.
            }
            finally
            {
                LocationHelper.CancellationTokenSource = null;
            }
            return null;
        }

        public static async Task<BasicGeoposition> GetRandomGeoposition()
        {
            var center = (await LocationHelper.GetCurrentLocationAsync())?.Position ??
                new BasicGeoposition { Latitude = 47.640068, Longitude = -122.129858 };
            int latitudeRange = 36000;
            int longitudeRange = 53000;
            var random = new Random();
            Func<int, double, double> getCoordinate = (range, midpoint) =>
                (random.Next(range) - (range / 2)) * 0.00001 + midpoint;

            BasicGeoposition Position = new BasicGeoposition
            {
                Latitude = getCoordinate(latitudeRange, center.Latitude),
                Longitude = getCoordinate(longitudeRange, center.Longitude)
            };
            return Position;
        }

        /// <summary>
        /// Cancels any waiting GetGeopositionAsync call.
        /// </summary>
        public static void CancelGetCurrentLocation()
        {
            if (LocationHelper.CancellationTokenSource != null)
            {
                LocationHelper.CancellationTokenSource.Cancel();
                LocationHelper.CancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Launches the Maps app and displays the route from the current location
        /// to the specified location.
        /// </summary>
        /// <param name="location">The location to display the route to.</param>
        public static async Task ShowRouteToLocationInMapsAppAsync(LocationPin location, LocationPin currentLocation)
        {
            var mapUri = new Uri("bingmaps:?trfc=1&rtp=" +
                $"pos.{Math.Round(currentLocation.Position.Latitude, 6)}_{Math.Round(currentLocation.Position.Longitude, 6)}~" +
                $"pos.{location.Position.Latitude}_{location.Position.Longitude}");
            await Windows.System.Launcher.LaunchUriAsync(mapUri);
        }

        /// <summary>
        /// Shows the specified text in a toast notification if notifications are enabled.
        /// </summary>
        /// <param name="text">The text to show.</param>
        private static void ShowToast(string text)
        {
            var toastXml = new XmlDocument();
            toastXml.LoadXml("<toast duration='short'><visual><binding template='ToastText01'>" +
                $"<text id='1'>{text}</text></binding></visual></toast>");
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }


        /// <summary>
        /// Attempts to update either the address or the coordinates of the specified location 
        /// if the other value is missing, using the specified current location to provide 
        /// context for prioritizing multiple locations returned for an address.  
        /// </summary>
        /// <param name="location">The location to update.</param>
        /// <param name="currentLocation">The current location.</param>
        public static async Task<bool> TryUpdateMissingLocationInfoAsync(LocationPin location, LocationPin currentLocation)
        {
            bool hasNoAddress = String.IsNullOrEmpty(location.Address);
            if (location.Photo == null)
                location.Photo = new SharedPhoto();
            if (!hasNoAddress)
                return true;
            else if (location.Position.Latitude == 0 && location.Position.Longitude == 0)
            {
                return false;
            }

            var results = await MapLocationFinder.FindLocationsAtAsync(location.Geopoint);


            if (results.Status == MapLocationFinderStatus.Success && results.Locations.Count > 0)
            {
                var result = results.Locations.First();

                //This will re-allocate the Position to that particular address.
                //   location.Position = result.Point.Position;
                location.Address = result.Address.FormattedAddress;
                if (String.IsNullOrEmpty(location.Name)) location.Name = result.Address.Town;

                // Sometimes the returned address is poorly formatted. This fixes one of the issues.
                if (location.Address.Trim().StartsWith(",")) location.Address = location.Address.Trim().Substring(1).Trim();
                return true;
            }
            else
            {
                return false;
            }

        }



    }
}
