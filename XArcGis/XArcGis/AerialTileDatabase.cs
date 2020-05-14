using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace XArcGis
{
    [Table("tiles")]
    public class AerialTile
    {
        [Column("zoom_level")]
        public int ZoomLevel { get; set; }

        [Column("tile_row")]
        public int Row { get; set; }

        [Column("tile_column")]
        public int Column { get; set; }

        [Column("tile_data")]
        public Byte[] TileData { get; set; }
    }

    public class AerialTileDatabase
    {
        readonly SQLiteAsyncConnection _database;

        public AerialTileDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public Task<AerialTile> GetTileAsync(int zoom, int row, int column)
        {
            return _database.Table<AerialTile>()
                            .Where(i => i.Row == row && i.ZoomLevel == zoom && i.Column == column)
                            .FirstOrDefaultAsync();
        }
    }
}
