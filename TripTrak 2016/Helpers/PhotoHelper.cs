using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace TripTrak_2016.Helpers
{
    public static class PhotoHelper
    {
        static PhotoHelper()
        {

        }

        public static string GenerateFileName(string context)
        {
            return context + "_" + DateTime.Now.ToString("yyyyMMddHH") + "_" + Guid.NewGuid().ToString() + ".jpg";
        }

        public static async Task<BitmapImage> getImageSource(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }
            try
            {
                StorageFile sourcePhoto = await KnownFolders.CameraRoll.GetFileAsync(imageName);
                if (sourcePhoto != null)
                {
                    var fileStream = await sourcePhoto.GetThumbnailAsync(ThumbnailMode.PicturesView);
                    sourcePhoto = null;
                    var img = new BitmapImage();
                    img.SetSource(fileStream);
                    return img;
                }
            }
            catch (Exception e)
            {
            }
            return null;
        }

        public static async Task<string> GetPhotoFromCameraLaunch(bool isCamera)
        {
            StorageFile photo = null;
            if (isCamera)
            {
                CameraCaptureUI captureUI = new CameraCaptureUI();
                captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;

                photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            }
            else
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                openPicker.FileTypeFilter.Add(".png");
                photo = await openPicker.PickSingleFileAsync();

            }
            if (photo == null)
            {
                // User cancelled photo capture
                return null;
            }
            else
            {
                if(!isCamera)
                    return photo.Name;

                IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
                string imageName = GenerateFileName("TripTrak");
                StorageFile destinationFile = await KnownFolders.CameraRoll.CreateFileAsync(imageName);
                using (var destinationStream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var destinationOutputStream = destinationStream.GetOutputStreamAt(0))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(stream, destinationStream);
                    }
                }

                return imageName;

            }
        }
    }
}
