using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelCustomer
    {
        [PrimaryKey]
        public int ID_CUSTOMER { get; set; }
        public string NAMA_CUSTOMER { get; set; }
        public string KONTAK_CUSTOMER { get; set; }
        public string ALAMAT_CUSTOMER { get; set; }
        public int DISKON_TETAP { get; set; }
        public override string ToString()
        {
            return $"{NAMA_CUSTOMER}{KONTAK_CUSTOMER}{ALAMAT_CUSTOMER}";
        }
    }
}
