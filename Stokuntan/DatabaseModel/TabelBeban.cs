using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelBeban
    {
        [PrimaryKey]
        public int ID_BEBAN { get; set; }
        public string TANGGAL_BEBAN { get; set; }
        public string KETERANGAN { get; set; }
        public string JENIS { get; set; }
        public string NILAI_BEBAN { get; set; }
        public string JUMLAH { get; set; }
        public string TOTAL_BEBAN { get; set; }
    }
}
