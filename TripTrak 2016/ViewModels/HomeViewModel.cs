using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripTrak_2016.Model;

namespace TripTrak_2016.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        private bool isSimpleMap = false;
        public bool IsSimpleMap
        {
            get { return this.isSimpleMap; }
            set { this.SetProperty(ref this.isSimpleMap, value); }
        }

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
            set { this.SetProperty(ref this.pinnedLocations, value); }
        }
    }
}
