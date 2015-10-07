using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TripTrak_2016.Model;
using Windows.Storage;

namespace TripTrak_2016.Helpers
{
    class LocalDataStorage
    {
        private const string dataFileName = "TripTrakPins";
        private const string tripsFileName = "TripTrakTips";
        public static string GenerateFileName(string context, DateTime date)
        {
            return context + "_" + date.ToString("yyyyMMdd") + ".txt";
        }


        /// <summary>
        /// Load all saved LocationPin list from roaming storage. 
        /// </summary>
        public async Task<ObservableCollection<LocationPin>> GetLocationPinsByDate(DateTime date)
        {
            ObservableCollection<LocationPin> data = new ObservableCollection<LocationPin>();
            try
            {
                string fileName = GenerateFileName(dataFileName, date);
                StorageFile dataFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileName);
                string text = await FileIO.ReadTextAsync(dataFile);
                if (text.Length > 0)
                {
                    text = text.Insert(0, "[");
                    text = text.Remove(text.Length - 1);
                    text = text.Insert(text.Length, "]");
                    byte[] bytes = Encoding.Unicode.GetBytes(text);
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<LocationPin>));
                    using (var stream = new MemoryStream(bytes))
                    {
                        data = serializer.ReadObject(stream) as ObservableCollection<LocationPin>;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            return data;
        }

        /// <summary>
        /// Save the BasicGeoposition list to roaming storage. 
        /// </summary>
        /// <param name="locations">The BasicGeoposition list  to save.</param>
        public async Task InsertLocationDataAsync(LocationPin location)
        {
            string fileName = GenerateFileName(dataFileName, location.DateCreated.Date);
            StorageFile sampleFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(
                fileName, CreationCollisionOption.OpenIfExists);
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(LocationPin));
                serializer.WriteObject(stream, location);
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    string locationString = reader.ReadToEnd();
                    locationString = locationString + ",";
                    await FileIO.AppendTextAsync(sampleFile, locationString);
                }
            }
        }


        /// <summary>
        /// Insert new trip to roaming storage. 
        /// </summary>
        /// <param name="trip">trip  to save.</param>
        public async Task CreateNewTrip(Trip trip)
        {
            string fileName = tripsFileName;
            StorageFile sampleFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(
                fileName, CreationCollisionOption.OpenIfExists);
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(Trip));
                serializer.WriteObject(stream, trip);
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    string locationString = reader.ReadToEnd();
                    locationString = locationString + ",";
                    await FileIO.AppendTextAsync(sampleFile, locationString);
                }
            }
        }

        /// <summary>
        /// Load all saved Trip list from roaming storage. 
        /// </summary>
        public async Task<ObservableCollection<Trip>> GetAllTrip()
        {
            ObservableCollection<Trip> data = new ObservableCollection<Trip>();
            try
            {
                string fileName = tripsFileName;
                StorageFile dataFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileName);
                string text = await FileIO.ReadTextAsync(dataFile);
                if (text.Length > 0)
                {
                    text = text.Insert(0, "[");
                    text = text.Remove(text.Length - 1);
                    text = text.Insert(text.Length, "]");
                    byte[] bytes = Encoding.Unicode.GetBytes(text);
                    var serializer = new DataContractJsonSerializer(typeof(ObservableCollection<Trip>));
                    using (var stream = new MemoryStream(bytes))
                    {
                        data = serializer.ReadObject(stream) as ObservableCollection<Trip>;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            return data;
        }

        /// <summary>
        /// Save all trip data to roaming storage. 
        /// </summary>
        /// <param name="locations">The locations to save.</param>
        public static async Task SaveAllTrips(IEnumerable<Trip> trips)
        {
            StorageFile sampleFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(
                tripsFileName, CreationCollisionOption.ReplaceExisting);
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(List<Trip>));
                serializer.WriteObject(stream, trips.ToList());
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    string locationString = reader.ReadToEnd();
                    locationString = locationString.Replace("[", "");
                    locationString = locationString.Replace("]", "");
                    locationString = locationString + ",";
                    await FileIO.WriteTextAsync(sampleFile, locationString);
                }
            }
        }


        /// <summary>
        /// Edit trip and save to roaming storage. 
        /// </summary>
        /// <param name="editedTrip">trip  to save.</param>
        public async Task EditTrip(Trip editedTrip)
        {
            ObservableCollection<Trip> data = new ObservableCollection<Trip>();
            data = await GetAllTrip();
            var tripItem = data.Select(t => t.DateCreated == editedTrip.DateCreated) as Trip;
            data.Remove(tripItem);
            data.Add(editedTrip);
            await SaveAllTrips(data);
        }

    }
}
