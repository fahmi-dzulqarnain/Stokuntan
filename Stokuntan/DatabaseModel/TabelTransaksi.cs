using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelTransaksi
    {
        [PrimaryKey]
        public int ID_TRANSAKSI { get; set; }
        public string NAMA_FIELD_TOTAL { get; set; }
        public string TGL_TRANSAKSI { get; set; }
        public string KETERANGAN { get; set; }
        public string METODE_BAYAR { get; set; }
        //public string SUB_OR_ADD { get; set; } // Minus atau Tambah
        public double NILAI { get; set; }
    }
}
