using SQLite;

namespace Stokuntan.DatabaseModel
{
    class TabelAkun
    {
        [PrimaryKey]
        public int ID_AKUN { get; set; }
        public string USERNAME { get; set; }
        public string KATA_SANDI { get; set; }
        public string JENIS_AKUN { get; set; }
        public string NAMA_TOKO { get; set; }
        public string KONTAK { get; set; }
        public string ALAMAT { get; set; }
        public bool BIARMASUK { get; set; }
    }
}
