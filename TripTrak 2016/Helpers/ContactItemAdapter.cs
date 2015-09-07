using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace TripTrak_2016.Helpers
{
    public class ContactItemAdapter
    {
        public string Name { get; private set; }
        public string SecondaryText { get; private set; }
        public BitmapImage Thumbnail { get; private set; }

        public ContactItemAdapter(Contact contact)
        {
            Name = contact.DisplayName;
            if (contact.Emails.Count > 0)
            {
                SecondaryText = contact.Emails[0].Address;
            }
            else if (contact.Phones.Count > 0)
            {
                SecondaryText = contact.Phones[0].Number;
            }
            else if (contact.Addresses.Count > 0)
            {
                List<string> addressParts = (new List<string> { contact.Addresses[0].StreetAddress,
                  contact.Addresses[0].Locality, contact.Addresses[0].Region, contact.Addresses[0].PostalCode });
                string unstructuredAddress = string.Join(", ", addressParts.FindAll(s => !string.IsNullOrEmpty(s)));
                SecondaryText = unstructuredAddress;
            }
            GetThumbnail(contact);
        }

        private async void GetThumbnail(Contact contact)
        {
            if (contact.Thumbnail != null)
            {
                IRandomAccessStreamWithContentType stream = await contact.Thumbnail.OpenReadAsync();
                if (stream != null && stream.Size > 0)
                {
                    Thumbnail = new BitmapImage();
                    Thumbnail.SetSource(stream);
                }
            }
        }
    }


}
