using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelKategori
    {
        [PrimaryKey]
        public string NAMA_KATEGORI { get; set; }

        public override string ToString()
        {
            return $"{NAMA_KATEGORI}";
        }
    }
}
