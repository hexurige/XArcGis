using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;

namespace XArcGis
{
    public class FGTiltInfoFactory
    {
        public const double LevelZeroResolution = 156543.03392799999;
        public const double LevelZeroScale      = 591657527.591555;
        public const int TileHeight = 256;
        public const int TileWidth = 256;
        public const int MaxLevel = 23;
        public const int DPI = 96;

        public static TileInfo GetOpenStreetMapTileInfo()
        {
            var spatialReference = new SpatialReference(3857);

            var levelOfDetials = new List<LevelOfDetail>();
            levelOfDetials.Add(new LevelOfDetail(0, LevelZeroResolution, LevelZeroScale));
            var resolution = LevelZeroResolution;
            var scale = LevelZeroScale;
            for (var i = 1; i <= MaxLevel; i++)
            {
                resolution /= 2.0;
                scale /= 2.0;
                levelOfDetials.Add(new LevelOfDetail(i, resolution, scale));
            }

            var origin = new MapPoint(-20037508.342787, 20037508.342787, spatialReference);

            var tileInfo = new TileInfo(
                DPI,
                TileImageFormat.Png8,
                levelOfDetials,
                origin,
                spatialReference,
                TileHeight,
                TileWidth);

            return tileInfo;
        }
    }

    public class FGBaseMapTiledLayer : ImageTiledLayer
    {
        public FGBaseMapTiledLayer(TileInfo tileInfo, Envelope fullExtent) 
            : base(tileInfo, fullExtent) {
        }

        //protected override Task<Uri> GetTileUriAsync(int level, int row, int column, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        protected override Task<ImageTileData> GetTileDataAsync(int level, int row, int column, CancellationToken cancellationToken)
        {
            int _row = (int)Math.Pow(2, level) - row - 1;
            var tileTask = App.AerialTileDb.GetTileAsync(level, _row, column);
            var t = tileTask.ContinueWith<ImageTileData>( 
                x =>
                    {
                        var tile = tileTask.GetAwaiter().GetResult();
                        if (tile != null)
                        {
                            var imageTileData = new ImageTileData(tile.ZoomLevel, tile.Row, tile.Column, tile.TileData, "");
                            return imageTileData;
                        } 
                        else
                        {
                            return null;
                        }
                        
                    }
                );


            return t;
        }
    }
}
