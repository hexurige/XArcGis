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
using Esri.ArcGISRuntime.Xamarin.Forms;

namespace XArcGis
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private GeoPackage MyGeoPackage { get; set; }

        public MainPage()
        {
            InitializeComponent();

            InitializeMap();
        }

        internal static string GetOfflinePackagePath(string name)
        {
            return Path.Combine(App.GetOfflinePackageFolder(), name);
        }

        private async void ViewpointChangedHandler(object sender, EventArgs e)
        {


        }

        private async void GeoViewTappedHandler(object sender, GeoViewInputEventArgs e)
        {
            // get the tap location in screen units

            var pixelTolerance = 50;
            var returnPopupsOnly = false;
            var maxLayerResults = 5;

            // identify all layers in the MapView, passing the tap point, tolerance, types to return, and max results
            IReadOnlyList<IdentifyLayerResult> idLayerResults = await MyMapView.IdentifyLayersAsync(e.Position, pixelTolerance, returnPopupsOnly, maxLayerResults);

            // iterate the results for each layer
            foreach (IdentifyLayerResult idResults in idLayerResults)
            {
                // get the layer identified and cast it to FeatureLayer
                FeatureLayer idLayer = idResults.LayerContent as FeatureLayer;

                // iterate each identified GeoElement in the results for this layer
                foreach (GeoElement idElement in idResults.GeoElements)
                {
                    // cast the result GeoElement to Feature
                    Feature idFeature = idElement as Feature;
                   
                    // select this feature in the feature layer
                    idLayer.SelectFeature(idFeature);
                }

                
            }
        }

        private async void LoadByFeatureCollectionLayer()
        {
            FeatureCollection featCollection = new FeatureCollection();
            FeatureCollectionLayer layer = new FeatureCollectionLayer(featCollection);
            if (layer.Layers != null)
            {
                layer.Layers.Select(x => x.RenderingMode = FeatureRenderingMode.Dynamic);
            }
            MyMapView.Map.OperationalLayers.Add(layer);


            foreach (var tab in MyGeoPackage.GeoPackageFeatureTables)// Where(x => x.TableName.Contains("polygon")).ToList();
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
            var yellowMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.FromArgb(128, 0, 0, 255), 3);
            var purpleMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Purple, 15);
            renderer.FieldNames.Add("symbol");
            renderer.UniqueValues.Add(new UniqueValue("", "", yellowMarker, "(#0000FF,3,0.5)"));
            renderer.UniqueValues.Add(new UniqueValue("", "", purpleMarker, "(#0000FF,5,0.5)"));

            
            renderer.DefaultSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Black, 10);
            renderer.DefaultLabel = "(#00FFFF,15,0.5)";
            return renderer;
        }

        private async void LoadByFeatureLayer()
        {
            // Read the feature tables
            foreach (var tab in MyGeoPackage.GeoPackageFeatureTables)
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

            string geoPackagePath = GetOfflinePackagePath(@"Full_20200513043139.gpkg");

            try
            {
                //MyMapView.Map = new Map(BasemapType.OpenStreetMap, 0, 0, 10);
                var tileInfo = FGTiltInfoFactory.GetOpenStreetMapTileInfo();

                var center = new MapPoint(12886578.9273218, -3753475.38812722, new SpatialReference(3857));
                var extend = new Envelope(center, 100000000.0, 100000000.0);

                var aerial = new FGBaseMapTiledLayer(
                    tileInfo,
                    extend
                    );

                var basemap = new Basemap(aerial);
                MyMapView.Map = new Map(basemap);
                await MyMapView.SetViewpointCenterAsync(center, ScaleByZoomLevel(16));

                MyMapView.ViewpointChanged += ViewpointChangedHandler;
                MyMapView.GeoViewTapped += GeoViewTappedHandler;

                // Open the GeoPackage
                this.MyGeoPackage = await GeoPackage.OpenAsync(geoPackagePath);
                //LoadByFeatureCollectionLayer();
                LoadByFeatureLayer();
                //SetViewpointBasedOnOperationalLayers();

            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
    }
}
