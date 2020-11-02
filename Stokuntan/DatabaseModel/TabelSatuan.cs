using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelSatuan
    {
        [PrimaryKey]
        public string NAMA_SATUAN { get; set; }
        public int DALAM_GRAM { get; set; }

        public override string ToString()
        {
            return $"{NAMA_SATUAN}";
        }
    }
}
