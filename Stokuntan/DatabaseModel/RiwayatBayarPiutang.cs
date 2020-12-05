using SQLite;

namespace Stokuntan.DatabaseModel
{
    class RiwayatBayarPiutang
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int ID_BELI { get; set; }
        public string TGL_BAYAR { get; set; }
        public string TOTAL_BAYAR { get; set; }
    }
}
