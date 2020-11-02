using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelTotal
    {
        [PrimaryKey]
        public string NAMA_FIELD { get; set; }
        public double TOTAL { get; set; }
    }
}
