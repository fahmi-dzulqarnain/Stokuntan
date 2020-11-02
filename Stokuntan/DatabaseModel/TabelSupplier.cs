using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelSupplier
    {
        [PrimaryKey]
        public int ID_SUPPLIER { get; set; }
        public string NAMA_SUPPLIER { get; set; }
        public string KONTAK_SUPPLIER { get; set; }
        public string ALAMAT_SUPPLIER { get; set; }

        public override string ToString()
        {
            return $"{NAMA_SUPPLIER}{KONTAK_SUPPLIER}{ALAMAT_SUPPLIER}";
        }
    }
}
