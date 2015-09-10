using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.UI.Xaml.Media.Imaging;

namespace TripTrak_2016.Model
{
    /// <summary>
    /// Represents a saved location for use in tracking travel time, distance, and routes. 
    /// </summary>
    public class LocationPin : BindableBase
    {
        private string name;
        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.SetProperty(ref this.name, value); }
        }

        private Double? speed;
        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public Double? Speed
        {
            get { return this.speed; }
            set { this.SetProperty(ref this.speed, value); }
        }

        
        private string address;
        /// <summary>
        /// Gets or sets the address of the location.
        /// </summary>
        public string Address
        {
            get { return this.address; }
            set { this.SetProperty(ref this.address, value); }
        }

        private SharedPhoto photo;
        /// <summary>
        /// Gets or sets the address of the location.
        /// </summary>
        public SharedPhoto Photo
        {
            get { return this.photo; }
            set { this.SetProperty(ref this.photo, value); }
        }


        private bool isCheckPoint = false;
        /// <summary>
        /// Gets or sets a value that indicates whether the location is 
        /// the Check point in the list of saved locations.
        /// </summary>
        public bool IsCheckPoint
        {
            get { return this.isCheckPoint; }
            set
            {
                this.SetProperty(ref this.isCheckPoint, value);
            }
        }

        private BasicGeoposition position;
        /// <summary>
        /// Gets the geographic position of the location.
        /// </summary>
        public BasicGeoposition Position
        {
            get { return this.position; }
            set
            {

                this.SetProperty(ref this.position, value);
                this.OnPropertyChanged(nameof(Geopoint));
            }
        }

        private DateTimeOffset dateCreated = DateTimeOffset.Now;
        /// <summary>
        /// Gets or sets a value that indicates when the travel info was last updated. 
        /// </summary>
        public DateTimeOffset DateCreated
        {
            get
            {
                if (IsCurrentLocation)
                    return DateTimeOffset.Now;
                else
                    return this.dateCreated;
            }
            set
            {
                this.SetProperty(ref this.dateCreated, value);
            }
        }

        /// <summary>
        /// Gets a Geopoint representation of the current location for use with the map service APIs.
        /// </summary>
        public Geopoint Geopoint => new Geopoint(this.Position);

        private bool isCurrentLocation;
        /// <summary>
        /// Gets or sets a value that indicates whether the location represents the user's current location.
        /// </summary>
        [IgnoreDataMember]
        public bool IsCurrentLocation
        {
            get { return this.isCurrentLocation; }
            set
            {
                this.SetProperty(ref this.isCurrentLocation, value);
                this.OnPropertyChanged(nameof(NormalizedAnchorPoint));
            }
        }

        private bool isSelected;
        /// <summary>
        /// Gets or sets a value that indicates whether the location is 
        /// the currently selected one in the list of saved locations.
        /// </summary>
        [IgnoreDataMember]
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                this.SetProperty(ref this.isSelected, value);
            }
        }

        private Point centerpoint = new Point(0.5, 0.5);
        private Point pinpoint = new Point(0.5, 0.9778);
        /// <summary>
        /// Gets a value for the MapControl.NormalizedAnchorPoint attached property
        /// to reflect the different map icon used for the user's current location. 
        /// </summary>
        [IgnoreDataMember]
        public Point NormalizedAnchorPoint => IsCurrentLocation ? pinpoint : centerpoint;

        private MapRoute fastestRoute;
        /// <summary>
        /// Gets or sets the route with the shortest travel time to the 
        /// location from the user's current position.
        /// </summary>
        [IgnoreDataMember]
        public MapRoute FastestRoute
        {
            get { return this.fastestRoute; }
            set { this.SetProperty(ref this.fastestRoute, value); }
        }

        private int currentTravelTimeWithoutTraffic;
        /// <summary>
        /// Gets or sets the number of minutes it takes to drive to the location,
        /// without taking traffic into consideration.
        /// </summary>
        [IgnoreDataMember]
        public int CurrentTravelTimeWithoutTraffic
        {
            get { return this.currentTravelTimeWithoutTraffic; }
            set { this.SetProperty(ref this.currentTravelTimeWithoutTraffic, value); }
        }

        private int currentTravelTime;
        /// <summary>
        /// Gets or sets the number of minutes it takes to drive to the location,
        /// taking traffic into consideration.
        /// </summary>
        [IgnoreDataMember]
        public int CurrentTravelTime
        {
            get { return this.currentTravelTime; }
            set
            {
                this.SetProperty(ref this.currentTravelTime, value);
                this.OnPropertyChanged(nameof(FormattedCurrentTravelTime));
            }
        }

        /// <summary>
        /// Gets a display-string representation of the current travel time. 
        /// </summary>
        public string FormattedCurrentTravelTime =>
            this.CurrentTravelTime == 0 ? "??:??" :
            new TimeSpan(0, this.CurrentTravelTime, 0).ToString("hh\\:mm");

        private double currentTravelDistance;
        /// <summary>
        /// Gets or sets the current driving distance to the location, in miles.
        /// </summary>
        [IgnoreDataMember]
        public double CurrentTravelDistance
        {
            get { return this.currentTravelDistance; }
            set
            {
                this.SetProperty(ref this.currentTravelDistance, value);
                this.OnPropertyChanged(nameof(FormattedCurrentTravelDistance));
            }
        }

        /// <summary>
        /// Gets a display-string representation of the lat and lon.
        /// </summary>
        [IgnoreDataMember]
        public string FormattedLatLon => 1 > 0 ?
            $"{String.Format("{0:0.######}", this.Position.Latitude)}, {String.Format("{0:0.######}", this.Position.Longitude)}, {String.Format("{0:0.######}", this.Position.Altitude)}" : this.Name;

        /// <summary>
        /// Gets a display-string representation of the current travel distance.
        /// </summary>
        [IgnoreDataMember]
        public string FormattedCurrentTravelDistance =>
            this.CurrentTravelDistance == 0 ? "?? miles" :
            this.CurrentTravelDistance + " miles";


        private DateTimeOffset timestamp;
        /// <summary>
        /// Gets or sets a value that indicates when the travel info was last updated. 
        /// </summary>
        [IgnoreDataMember]
        public DateTimeOffset Timestamp
        {
            get { return this.timestamp; }
            set
            {
                this.SetProperty(ref this.timestamp, value);
                this.OnPropertyChanged(nameof(FormattedTimeStamp));
            }
        }

        /// <summary>
        /// Raises a change notification for the timestamp in order to update databound UI.   
        /// </summary>
        public void RefreshFormattedTimestamp() => this.OnPropertyChanged(nameof(FormattedTimeStamp));

        /// <summary>
        /// Gets a display-string representation of the freshness timestamp. 
        /// </summary>
        [IgnoreDataMember]
        public string FormattedTimeStamp
        {
            get
            {
                double minutesAgo = this.Timestamp == DateTimeOffset.MinValue ? 0 :
                    Math.Floor((DateTimeOffset.Now - this.Timestamp).TotalMinutes);
                return $"{minutesAgo} minute{(minutesAgo == 1 ? "" : "s")} ago";
            }
        }

        private bool isMonitored;
        /// <summary>
        /// Gets or sets a value that indicates whether this location is 
        /// being monitored for an increase in travel time due to traffic. 
        /// </summary>
        [IgnoreDataMember]
        public bool IsMonitored
        {
            get { return this.isMonitored; }
            set { this.SetProperty(ref this.isMonitored, value); }
        }

        /// <summary>
        /// Returns the name of the location, or the geocoordinates if there is no name. 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.IsNullOrEmpty(this.Name) ?
            $"{this.Position.Latitude}, {this.Position.Longitude}" : this.Name;

    }

    public class SharedPhoto
    {
        public string ImageName { get; set; }
        public string ShareWith { get; set; }
        public string Description { get; set; }
        [IgnoreDataMember]
        public BitmapImage ImageSource { get; set; }
    }
}
