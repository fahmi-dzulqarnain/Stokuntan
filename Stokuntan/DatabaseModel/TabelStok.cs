using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Stokuntan.DatabaseModel
{
    class TabelStok
    {
        [PrimaryKey]
        public int ID_STOK { get; set; }
        public string TGL_MASUK { get; set; }
        public string KATEGORI { get; set; }
        public string DESKRIPSI { get; set; }
        public string MEREK { get; set; }
        public int JUMLAH_STOK { get; set; }
        public string SATUAN { get; set; }
        public string NO_NOTA_REF { get; set; }
        public int HARGA_BELI { get; set; }
        public int HARGA_JUAL { get; set; }
        public string SATUAN_JUAL { get; set; }
        public string METODE_BAYAR { get; set; } // Utang / Cash
        public int ID_SUPPLIER { get; set; }
    }
}
