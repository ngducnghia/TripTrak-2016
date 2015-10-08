﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripTrak_2016.Helpers;
using TripTrak_2016.Model;

namespace TripTrak_2016.ViewModels
{
    public class MyTripsViewModel : BindableBase
    {
        LocalDataStorage localData = new LocalDataStorage();
        public MyTripsViewModel()
        {
        }

        private ObservableCollection<Trip> allTrips = new ObservableCollection<Trip>();
        /// <summary>
        /// Gets or sets the locations represented on the map; this is a superset of Locations, and 
        /// includes the current location and any locations being added but not yet saved. 
        /// </summary>
        public ObservableCollection<Trip> AllTrips
        {
            get { return this.allTrips; }
            set
            {
                this.SetProperty(ref this.allTrips, value);
            }
        }



    }
}
