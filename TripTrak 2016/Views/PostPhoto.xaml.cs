using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TripTrak_2016.Helpers;
using TripTrak_2016.Model;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
        bool isOpenContactPicker = false;
        public IList<Contact> contacts;
        private string imagePath = string.Empty;
        private string locationName = string.Empty;
        public PostPhoto()
        {
            this.InitializeComponent();
            contactPickerTb.GotFocus += ContactPickerTb_GotFocus;
            CameraButton.Click += CameraButton_Click;
        }

        private async void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            var item = new LocationPin
            {
                Position = App.currentLocation.Position,
                Photo = new SharedPhoto
                {
                    ShareWith = contactPickerTb.Text,
                    ImageName = imagePath,
                    description = DescriptionTb.Text,
                },
                IsCheckPoint = true
            };
            await LocalDataStorage.InsertLocationDataAsync(item);
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
                imagePath = dic["ImagePath"];
                locationName = dic["LocationName"];
            }
            StorageFile sourcePhoto = await KnownFolders.CameraRoll.GetFileAsync(imagePath);
            IRandomAccessStream stream = await sourcePhoto.OpenAsync(FileAccessMode.Read);

            if (sourcePhoto != null)
            {
                var imgSource = new BitmapImage();
                imgSource.SetSource(stream);
                imageToPost.Source = imgSource;
            }
            LocationNameTbl.Text = locationName;

            base.OnNavigatedTo(e);
        }

        private async void contactsPicker()
        {
            var contactPicker = new ContactPicker();
            contactPicker.CommitButtonText = "Select";
            contacts = await contactPicker.PickContactsAsync();
            contactPickerTb.Text = string.Empty;
            if (contacts!=null && contacts.Count > 0)
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
