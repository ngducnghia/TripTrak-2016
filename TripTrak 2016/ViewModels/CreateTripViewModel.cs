using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripTrak_2016.Model;

namespace TripTrak_2016.ViewModels
{
    public class CreateTripViewModel : BindableBase
    {
        private LocationPin pinDisplayInformation = new LocationPin();
        public LocationPin PinDisplayInformation
        {
            get { return this.pinDisplayInformation; }
            set { this.SetProperty(ref this.pinDisplayInformation, value); }
        }

        private ObservableCollection<LocationPin> pinnedLocations= new ObservableCollection<LocationPin>();
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
    }
}
