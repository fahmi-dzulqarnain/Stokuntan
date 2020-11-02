using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelRealStok
    {
        [PrimaryKey]
        public int ID_STOK_END { get; set; }
        public string KATEGORI_MEREK { get; set; }
        public double TOTAL_STOK { get; set; }
        public string SATUAN_STOK { get; set; }
        public int STOK_DLM_GRAM { get; set; }
        public string HARGA_JUAL_PER { get; set; } // Dalam Satuan Ons atau Kg | Cth. 100.000/Ons
        public override string ToString()
        {
            return $"{KATEGORI_MEREK}{SATUAN_STOK}{HARGA_JUAL_PER}";
        }
    }
}
