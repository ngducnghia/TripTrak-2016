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
        private const string dataFileName = "TripTrakPins.txt";


        /// <summary>
        /// Load all saved LocationPin list from roaming storage. 
        /// </summary>
        public async Task<ObservableCollection<LocationPin>> GetAllLocationPins()
        {
            ObservableCollection<LocationPin> data = null;
            try
            {
                StorageFile dataFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(dataFileName);
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
                // Do nothing.
            }
            return data ?? new ObservableCollection<LocationPin>();
        }

        /// <summary>
        /// Save the BasicGeoposition list to roaming storage. 
        /// </summary>
        /// <param name="locations">The BasicGeoposition list  to save.</param>
        public async Task InsertLocationDataAsync(LocationPin location)
        {
            StorageFile sampleFile = await ApplicationData.Current.RoamingFolder.CreateFileAsync(
                dataFileName, CreationCollisionOption.OpenIfExists);
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
    }
}
