using System;
using System.IO;
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
                    var path = Path.Combine(GetOfflinePackageFolder(), "aerial.sqlite");
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

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
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
