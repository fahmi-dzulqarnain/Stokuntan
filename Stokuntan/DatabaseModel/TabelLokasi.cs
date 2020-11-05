using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelLokasi
    {
        [PrimaryKey]
        public int ID_LOKASI { get; set; }
        public string NAMA_LOKASI { get; set; }
        public override string ToString()
        {
            return $"{NAMA_LOKASI}";
        }
    }
}
