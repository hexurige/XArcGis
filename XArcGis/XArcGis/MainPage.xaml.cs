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


        }

        private async void LoadByFeatureCollectionLayer(GeoPackage myGeoPackage)
        {
            FeatureCollection featCollection = new FeatureCollection();
            FeatureCollectionLayer layer = new FeatureCollectionLayer(featCollection);
            if (layer.Layers != null)
            {
                layer.Layers.Select(x => x.RenderingMode = FeatureRenderingMode.Dynamic);
            }
            MyMapView.Map.OperationalLayers.Add(layer);


            foreach (var tab in myGeoPackage.GeoPackageFeatureTables)// Where(x => x.TableName.Contains("polygon")).ToList();
            {
                var queryPts = new QueryParameters();
                //queryPts.Geometry = MyMapView.VisibleArea;
                //queryPts.WhereClause = @" 1 = 1 ";
                var featureResult = await tab.QueryFeaturesAsync(queryPts);
                FeatureCollectionTable collectTable = new FeatureCollectionTable(featureResult);
                
                featCollection.Tables.Add(collectTable);
            }


        }

        private async void SetViewpointBasedOnOperationalLayers()
        {
            foreach (var l in MyMapView.Map.OperationalLayers)
            {
                if (l is FeatureLayer)
                {
                    var fl = (FeatureLayer)l;
                    if (fl.FullExtent != null)
                    {
                        await MyMapView.SetViewpointGeometryAsync(fl.FullExtent);
                        break;
                    }
                }
            }
        }

        private double ScaleByZoomLevel(int level)
        {
            return 591657550.500000 / Math.Pow(2, level - 1);
        }

        private UniqueValueRenderer GetRenderer()
        {
            var renderer = new UniqueValueRenderer();
            renderer.FieldNames.Add("symbol");
            renderer.UniqueValues.Add(new UniqueValue("(#0000FF,3,0.5)", "(#0000FF,3,0.5)", new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Yellow, 15), "(#0000FF,3,0.5)"));
            renderer.UniqueValues.Add(new UniqueValue("(#0000FF,5,0.5)", "(#0000FF,5,0.5)", new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Purple, 15), "(#0000FF,5,0.5)"));

            
            renderer.DefaultSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Black, 10);
            renderer.DefaultLabel = "(#00FFFF,15,0.5)";
            return renderer;
        }

        private async void LoadByFeatureLayer(GeoPackage myGeoPackage)
        {
            // Read the feature tables
            foreach (var tab in myGeoPackage.GeoPackageFeatureTables)
            {
                FeatureLayer l = new FeatureLayer(tab);
                l.RenderingMode = FeatureRenderingMode.Dynamic;
                l.MaxScale = ScaleByZoomLevel(23);
                l.MinScale = ScaleByZoomLevel(15);
                await l.LoadAsync();
                if (tab.TableName.Contains("point"))
                {
                    l.Renderer = GetRenderer();
                }
                
                
                //await l.LoadAsync();
                MyMapView.Map.OperationalLayers.Add(l);
            }
            
        }

        private async void InitializeMap()
        {

            string geoPackagePath = GetOfflinePackagePath(@"OfflinePackage_20200505023722.sqlite");


            try
            {
                MyMapView.Map = new Map(BasemapType.OpenStreetMap, 0, 0, 10);
                MyMapView.ViewpointChanged += ViewpointChangedHandler;

                // Open the GeoPackage
                GeoPackage myGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);
                //LoadByFeatureCollectionLayer(myGeoPackage);
                LoadByFeatureLayer(myGeoPackage);
                //SetViewpointBasedOnOperationalLayers();

            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
    }
}
