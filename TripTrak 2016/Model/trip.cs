using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripTrak_2016.Model
{
    public class Trip : BindableBase
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

        private string description;
        /// <summary>
        /// Gets or sets the description of the trip.
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.SetProperty(ref this.description, value); }
        }

        private string type;
        /// <summary>
        /// Gets or sets the type of the trip.
        /// </summary>
        public string Type
        {
            get { return this.type; }
            set { this.SetProperty(ref this.type, value); }
        }

        private string shareWith;
        public string ShareWith
        {
            get { return this.shareWith; }
            set { this.SetProperty(ref this.shareWith, value); }
        }

        private LocationPin startPin;
        /// <summary>
        /// Gets or sets the startPin of the trip.
        /// </summary>
        public LocationPin StartPin
        {
            get { return this.startPin; }
            set { this.SetProperty(ref this.startPin, value); }
        }

        private LocationPin endPin;
        /// <summary>
        /// Gets or sets the endPin of the trip.
        /// </summary>
        public LocationPin EndPin
        {
            get { return this.endPin; }
            set { this.SetProperty(ref this.endPin, value); }
        }

    }
}
