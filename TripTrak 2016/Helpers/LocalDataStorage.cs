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
    }
}
