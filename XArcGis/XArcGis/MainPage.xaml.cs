using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Esri.ArcGISRuntime.Portal;
using System.IO;

namespace XArcGis
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            InitializeMap();
        }

        internal static string GetOfflinePackagePath(string name)
        {
            return Path.Combine(GetOfflinePackageFolder(), name);
        }

        internal static string GetOfflinePackageFolder()
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

        private async void ViewpointChangedHandler(object sender, EventArgs args)
        {

                // Get the full path
            string geoPackagePath = GetOfflinePackagePath(@"OfflinePackage_20200501021332.sqlite");

                // Open the GeoPackage
            GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);

            // Read the feature tables and get the first one
            FeatureTable geoPackageTable = myGeoPackage.GeoPackageFeatureTables.FirstOrDefault();

            // Make sure a feature table was found in the package
            if (geoPackageTable == null) { return; }

            // Create a layer to show the feature table
            FeatureLayer newLayer = new FeatureLayer(geoPackageTable);


            var queryPts = new QueryParameters();
            queryPts.Geometry = MyMapView.VisibleArea;
            queryPts.WhereClause = @" 1 = 1 ";
               

            var featureResult = await geoPackageTable.QueryFeaturesAsync(queryPts);

            // Create a new feature collection table from the result features
            FeatureCollectionTable collectTable = new FeatureCollectionTable(featureResult);

            // Create a feature collection and add the table
            FeatureCollection featCollection = new FeatureCollection();
            featCollection.Tables.Add(collectTable);

            // Create a layer to display the feature collection, add it to the map's operational layers
            FeatureCollectionLayer featCollectionTable = new FeatureCollectionLayer(featCollection);

            // Add the feature table as a layer to the map (with default symbology)
            MyMapView.Map.OperationalLayers.Add(featCollectionTable);

            var newVp = new Viewpoint(newLayer.FullExtent);
            MyMapView.SetViewpoint(newVp);


        }

        private async void InitializeMap()
        {
            try
            {
                // Create a new map centered on Aurora Colorado
                MyMapView.Map = new Map(BasemapType.OpenStreetMap, 39.7294, -104.8319, 9);
                MyMapView.ViewpointChanged += ViewpointChangedHandler;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
    }
}
