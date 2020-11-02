using SQLite;

namespace Stokuntan.DatabaseModel
{
    public class IDatabase
    {
        public static SQLiteConnection conns = new SQLiteConnection(App.lokasiDatabase);
        public IDatabase() 
        {
            CreateTables();
        }

        public SQLiteConnection Conn() { return conns; }

        public void CreateTables()
        {
            conns.CreateTable<TabelAkun>();
            conns.CreateTable<TabelKategori>();
            conns.CreateTable<TabelSupplier>();
            conns.CreateTable<TabelSatuan>();
            conns.CreateTable<TabelStok>();
            conns.CreateTable<TabelRealStok>();
            conns.CreateTable<TabelReject>();
            conns.CreateTable<TabelCustomer>();
            conns.CreateTable<TabelPenjualan>();
            conns.CreateTable<TabelBeban>();
            conns.CreateTable<TabelTransaksi>();
            conns.CreateTable<TabelTotal>();
        }

        public void DumpToDatabaseIfEmpty() 
        {
            var akun = Conn().Query<TabelAkun>("SELECT * FROM TabelAkun").Count;
            var satuan = Conn().Query<TabelSatuan>("SELECT * FROM TabelSatuan").Count;
            var trans = Conn().Query<TabelTotal>("SELECT * FROM TabelTotal").Count;
            var customer = Conn().Query<TabelCustomer>("SELECT * FROM TabelCustomer").Count;

            if (akun == 0) Conn().Insert(new TabelAkun 
            { 
                ID_AKUN = 1, USERNAME = "admin@minnaagency", KATA_SANDI = "admin", 
                ALAMAT = "Komp. Genta 1 Blok U No. 5 Depan Masjid Darussalam Batu Aji Batam", 
                JENIS_AKUN = "ADMIN", NAMA_TOKO = "CV. Minna Agency Batam", KONTAK = "0821-7310-4567 / 0821-7111-2121" 
            });
            if (satuan == 0) { 
                Conn().Insert(new TabelSatuan { NAMA_SATUAN = "Kilogram", DALAM_GRAM = 1000 });
                Conn().Insert(new TabelSatuan { NAMA_SATUAN = "Ons", DALAM_GRAM = 100 });
                Conn().Insert(new TabelSatuan { NAMA_SATUAN = "500 gr", DALAM_GRAM = 500 });
                Conn().Insert(new TabelSatuan { NAMA_SATUAN = "Botol 250 gr", DALAM_GRAM = 250 });
            }
            if (trans == 0) 
            {
                Conn().Insert(new TabelTotal() { NAMA_FIELD = "Kas", TOTAL = 0 });
                Conn().Insert(new TabelTotal() { NAMA_FIELD = "Piutang", TOTAL = 0 });
                Conn().Insert(new TabelTotal() { NAMA_FIELD = "Utang", TOTAL = 0 });
                Conn().Insert(new TabelTotal() { NAMA_FIELD = "Ekuitas", TOTAL = 0 });
            }
            if (customer == 0) Conn().Insert(new TabelCustomer() { ID_CUSTOMER = 1, NAMA_CUSTOMER = "Customer Umum", ALAMAT_CUSTOMER = "", DISKON_TETAP = 0, KONTAK_CUSTOMER = "" });
        }
    }
}
