using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripTrak_2016.Model;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace TripTrak_2016.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        private LocationPin pinDisplayInformation = new LocationPin();
        public LocationPin PinDisplayInformation
        {
            get { return this.pinDisplayInformation; }
            set { this.SetProperty(ref this.pinDisplayInformation, value); }
        }

        private ObservableCollection<LocationPin> pinnedLocations = new ObservableCollection<LocationPin>();
        /// <summary>
        /// Gets or sets the locations represented on the map; this is a superset of Locations, and 
        /// includes the current location and any locations being added but not yet saved. 
        /// </summary>
        public ObservableCollection<LocationPin> PinnedLocations
        {
            get { return this.pinnedLocations; }
            set
            {
                this.SetProperty(ref this.pinnedLocations, value);
            }
        }


        private object selectedLocation;
        /// <summary>
        /// Gets or sets the LocationData object corresponding to the current selection in the locations list. 
        /// </summary>
        public object SelectedLocation
        {
            get { return this.selectedLocation; }
            set
            {
                if (this.selectedLocation != value)
                {
                    var oldValue = this.selectedLocation as LocationPin;
                    var newValue = value as LocationPin;
                    if (oldValue != null)
                    {
                        oldValue.IsSelected = false;
                    }
                    if (newValue != null)
                    {
                        newValue.IsSelected = true;
                    }
                    this.selectedLocation = newValue;
                }
            }
        }


        private ObservableCollection<LocationPin> checkedLocations = new ObservableCollection<LocationPin>();
        /// <summary>
        /// This is Locations where users Check-in or take photo
        /// </summary>
        public ObservableCollection<LocationPin> CheckedLocations
        {
            get
            {
                return this.checkedLocations;
            }
            set
            {
                this.SetProperty(ref this.checkedLocations, value);
            }
        }


        private ObservableCollection<LocationPin> mileStoneLocations = new ObservableCollection<LocationPin>();
        /// <summary>
        /// This is Locations where users stop for a while or checked in.
        /// </summary>
        public ObservableCollection<LocationPin> MileStoneLocations
        {
            get
            {
                return this.mileStoneLocations;
            }
            set
            {
                this.SetProperty(ref this.mileStoneLocations, value);
            }
        }

        public  int drawPolylines(LocationPin breakColorPin, bool isAddMilestone, MapControl map)
        {
            int ret = 0;
            //remove all current polylines on map
            map.MapElements.Clear();

            //Order points by DateCreated
            var simpleGeoInDateOrder = this.PinnedLocations.OrderBy(x => x.DateCreated).ToList();
            var color = Colors.Blue;
            double thickness = 3;
            var Coords = new List<BasicGeoposition>();
            bool breakColor = false;
            //Query Points list to draw Polylines
            for (int i = 0; i < simpleGeoInDateOrder.Count; i++)
            {
                if (simpleGeoInDateOrder[i].IsCheckPoint == true && isAddMilestone)
                    this.MileStoneLocations.Add(simpleGeoInDateOrder[i]);
                if (Coords.Count == 0)
                    Coords.Add(simpleGeoInDateOrder[i].Position);
                else if (!breakColor && simpleGeoInDateOrder[i].DateCreated > breakColorPin.DateCreated)
                {
                    //define polyline
                    MapPolyline mapPolyline = new MapPolyline();
                    mapPolyline.StrokeColor = color;
                    mapPolyline.StrokeThickness = thickness;
                    mapPolyline.StrokeDashed = true;
                    mapPolyline.Path = new Geopath(Coords);

                    //draw polyline on map
                    map.MapElements.Add(mapPolyline);
                    ret++;
                    //Clear Coords.
                    Coords.Clear();
                    if (simpleGeoInDateOrder[i].IsCheckPoint == false && isAddMilestone)
                        this.MileStoneLocations.Add(simpleGeoInDateOrder[i]);
                    breakColor = !breakColor;
                    color = Colors.CornflowerBlue;
                    thickness = 2;
                }
                else if (simpleGeoInDateOrder[i].DateCreated - simpleGeoInDateOrder[i - 1].DateCreated < TimeSpan.FromMinutes(5) && Coords.Count < 200)
                {
                    Coords.Add(simpleGeoInDateOrder[i].Position);
                }
                else
                {
                    //define polyline
                    MapPolyline mapPolyline = new MapPolyline();
                    mapPolyline.StrokeColor = color;
                    mapPolyline.StrokeThickness = thickness;
                    mapPolyline.StrokeDashed = true;
                    mapPolyline.Path = new Geopath(Coords);

                    //draw polyline on map
                    map.MapElements.Add(mapPolyline);
                    ret++;
                    //Clear Coords.
                    Coords.Clear();
                    if (simpleGeoInDateOrder[i].IsCheckPoint == false && isAddMilestone)
                        this.MileStoneLocations.Add(simpleGeoInDateOrder[i]);
                }
            }
            //draw last Polyline on map
            if (Coords.Count > 1)
            {
                MapPolyline lastPolyline = new MapPolyline();
                lastPolyline.StrokeColor = color;
                lastPolyline.StrokeThickness = thickness;
                lastPolyline.StrokeDashed = true;
                lastPolyline.Path = new Geopath(Coords);

                //draw polyline on map
                map.MapElements.Add(lastPolyline);
                ret++;
            }
            return ret;
        }

        public List<BasicGeoposition> drawPolylines(ObservableCollection<LocationPin> pins, MapControl map)
        {
            //remove all current polylines on map
            map.MapElements.Clear();

            //Order points by DateCreated
            var simpleGeoInDateOrder = pins.OrderBy(x => x.DateCreated).ToList();
            var color = Colors.Blue;
            double thickness = 3;
            var Coords = new List<BasicGeoposition>();
            for (int i = 0; i < simpleGeoInDateOrder.Count; i++)
            {
                Coords.Add(simpleGeoInDateOrder[i].Position);
            }

            //define polyline
            MapPolyline mapPolyline = new MapPolyline();
            mapPolyline.StrokeColor = color;
            mapPolyline.StrokeThickness = thickness;
            mapPolyline.StrokeDashed = true;
            mapPolyline.Path = new Geopath(Coords);


            //draw polyline on map
            map.MapElements.Add(mapPolyline);

            return Coords;
        }

        public async Task setViewOnMap(List<BasicGeoposition> positions, MapControl map)
        {
            if (positions.Count == 0)
                return;
            var bounds = GeoboundingBox.TryCompute(positions);
            double viewWidth = ApplicationView.GetForCurrentView().VisibleBounds.Width;
            var margin = new Thickness((viewWidth >= 500 ? 300 : 10), 10, 10, 10);
            bool isSuccessful = await map.TrySetViewBoundsAsync(bounds, margin, MapAnimationKind.Default);
            if (isSuccessful && positions.Count < 2)
                map.ZoomLevel = 15;
            else if (!isSuccessful && positions.Count > 0)
            {
                map.Center = new Geopoint(positions[0]);
                map.ZoomLevel = 15;
            }
        }
    }
}
