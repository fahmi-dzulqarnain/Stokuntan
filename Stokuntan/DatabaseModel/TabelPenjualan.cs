using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelPenjualan
    {
        [PrimaryKey]
        public int ID_JUAL { get; set; }
        public string TANGGAL_JUAL { get; set; }
        public int CUSTOMER { get; set; }
        public string DETAIL_JUAL { get; set; }
        public string METODE_BAYAR { get; set; }
        public string TOTAL_PENJUALAN { get; set; }
        public string TOTAL_BAYAR { get; set; }
        public string SISA_BAYAR { get; set; }
        public string LOKASI_STOK { get; set; }
    }
}
