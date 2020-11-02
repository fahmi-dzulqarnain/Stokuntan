using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelReject
    {
        [PrimaryKey]
        public int ID_REJECT { get; set; }
        public string NAMA_STOK { get; set; }
        public string HARGA_JUAL { get; set; }
        public string TANGGAL_REJECT { get; set; }
        public string SEBAB_REJECT { get; set; }
        public string STOK_DLM_GRAM { get; set; }
        public string TOTAL_RUGI { get; set; }
        public override string ToString()
        {
            return $"{NAMA_STOK}{TANGGAL_REJECT}{SEBAB_REJECT}";
        }
    }
}
