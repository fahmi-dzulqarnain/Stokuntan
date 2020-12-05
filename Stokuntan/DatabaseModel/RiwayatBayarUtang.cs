using SQLite;

namespace Stokuntan.DatabaseModel
{
    class RiwayatBayarUtang
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string REF_NOTA_BELI { get; set; }
        public string TGL_BAYAR { get; set; }
        public string TOTAL_BAYAR { get; set; }
    }
}
