using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TripTrak_2016.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            VersionTbl.Text = "App version: " + GetAppVersion();
        }

        public string GetAppVersion()
        {
            Package package = Windows.ApplicationModel.Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            string temp = String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            return temp;
        }
    }
}
