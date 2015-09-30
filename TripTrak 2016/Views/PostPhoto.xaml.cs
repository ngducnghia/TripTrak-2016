using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TripTrak_2016.Helpers;
using TripTrak_2016.Model;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TripTrak_2016.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PostPhoto : Page
    {
        LocalDataStorage localData = new LocalDataStorage();
        bool isRandomPhotoLocation = false;
        bool isOpenContactPicker = false;
        public IList<Contact> contacts;
        public string imageName = string.Empty;
        private string locationName = string.Empty;
        public PostPhoto()
        {
            this.InitializeComponent();
            contactPickerTb.GotFocus += ContactPickerTb_GotFocus;
            PostButton.Click += PostButton_Click;
        }

        private async void PostButton_Click(object sender, RoutedEventArgs e)
        {
            var position = App.currentLocation.Position;
            if (isRandomPhotoLocation)
                position = LocationHelper.getRandomLocation(App.currentLocation.Position);

            var item = new LocationPin
            {
                Position = position,
                Photo = new SharedPhoto
                {
                    ShareWith = contactPickerTb.Text,
                    ImageName = imageName,
                    Description = DescriptionTb.Text,
                },
                IsCheckPoint = true
            };
            await localData.InsertLocationDataAsync(item);
            App.isSimpleMap = false;
            this.Frame.GoBack();
        }

        private void ContactPickerTb_GotFocus(object sender, RoutedEventArgs e)
        {
            isOpenContactPicker = !isOpenContactPicker;
            if (isOpenContactPicker)
                contactsPicker();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Dictionary<string, string>)
            {
                var dic = e.Parameter as Dictionary<string, string>;
                imageName = dic["ImagePath"];
            }
            StorageFile sourcePhoto = await KnownFolders.CameraRoll.GetFileAsync(imageName);
            IRandomAccessStream stream = await sourcePhoto.OpenAsync(FileAccessMode.Read);

            Geopoint geopoint = await GeotagHelper.GetGeotagAsync(sourcePhoto);
            if (geopoint != null && geopoint.Position.Latitude != 0)
                App.currentLocation.Position = geopoint.Position;
            await LocationHelper.TryUpdateMissingLocationInfoAsync(App.currentLocation, null);
            locationName = App.currentLocation.Name;

            if (sourcePhoto != null)
            {
                var imgSource = new BitmapImage();
                imgSource.SetSource(stream);
                imageToPost.Source = imgSource;
            }
            if (locationName != null)
                LocationNameTbl.Text = locationName;
            else
                LocationNameTbl.Text = "Unknown";
            base.OnNavigatedTo(e);
        }

        private async void contactsPicker()
        {
            var contactPicker = new ContactPicker();
            contactPicker.CommitButtonText = "Select";
            contacts = await contactPicker.PickContactsAsync();
            contactPickerTb.Text = string.Empty;
            if (contacts != null && contacts.Count > 0)
            {
                foreach (Contact contact in contacts)
                {
                    var item = (new ContactItemAdapter(contact));

                    contactPickerTb.Text = contactPickerTb.Text + item.Name + "; ";
                }
            }
            else
            {

            }
        }
    }
}
