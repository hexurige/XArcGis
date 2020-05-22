using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XArcGis
{
    public partial class App : Application
    {

        private static AerialTileDatabase aerialTileDb;
        public static AerialTileDatabase AerialTileDb
        {
            get
            {
                if (aerialTileDb == null)
                {
                    var path = Path.Combine(GetOfflinePackageFolder(), "aerial.offline");
                    aerialTileDb = new AerialTileDatabase(path);
                }
                return aerialTileDb;
            }
        }

        public static string GetOfflinePackageFolder()
        {
#if NETFX_CORE
            string appDataFolder  = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif XAMARIN
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
            string sampleDataFolder = Path.Combine(appDataFolder, "OfflinePackages");

            if (!Directory.Exists(sampleDataFolder)) { Directory.CreateDirectory(sampleDataFolder); }

            return sampleDataFolder;
        }

        private async void DownloadAerial()
        {
            var fullPkgUrl = "http://localhost:57674/api/v1/GisExport/download?gisExportId=35";

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFileCompleted += DownloadFullCompleted;
                wc.DownloadFileAsync(
                    // Param1 = Link of file
                    new System.Uri(fullPkgUrl),
                    // Param2 = Path to save
                    Path.Combine(App.GetOfflinePackageFolder(), "aerial.offline")

                );
            }
        }
        void DownloadFullCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MainPage = new MainPage();
        }

        public App()
        {
            InitializeComponent();
            DownloadAerial();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
