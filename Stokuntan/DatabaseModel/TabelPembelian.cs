using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelPembelian
    {
        [PrimaryKey]
        public string REF_NOTA { get; set; }
        public string TGL_BELI { get; set; }
        public int ID_SUPPLIER { get; set; }
        public string DAFTAR_STOK { get; set; } // Berisi ID dari TabelStok dan Dipisahkan dengan ";"
        public string METODE_BAYAR { get; set; }
        public int TOTAL_TAGIHAN { get; set; }
        public int TOTAL_BAYAR { get; set; }
        public int SISA_BAYAR { get; set; }
        public string LOKASI { get; set; }
    }
}
