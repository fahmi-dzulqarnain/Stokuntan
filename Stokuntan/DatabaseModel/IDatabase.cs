using Microsoft.Win32;
using SQLite;
using System.Configuration;
using System.IO;
using System.Windows;

namespace Stokuntan.DatabaseModel
{
    public class IDatabase
    {
        readonly string path = ConfigurationManager.AppSettings["lokasiDatabase"].ToString();
        public static SQLiteConnection conns;
        //public static SQLiteConnection conns = new SQLiteConnection(App.lokasiDatabase);
        public IDatabase() 
        {
            if (path == "")
            {
                var result = MessageBox.Show("Database Belum Ditemukan. Cari Lokasi Database", "Database Belum Ada", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    var theDialog = new OpenFileDialog
                    {
                        Title = "Open Text File",
                        Filter = "DB files|*.db",
                        InitialDirectory = @"C:\"
                    };
                    theDialog.ShowDialog();
                    if (theDialog.FileName != "")
                    {
                        MessageBox.Show(theDialog.FileName.ToString());
                        AddUpdateSetting("lokasiDatabase", theDialog.FileName);
                        path = theDialog.FileName;
                        conns = new SQLiteConnection(path);
                        CreateTables();
                        DumpToDatabaseIfEmpty();
                    }
                }
            }
            else conns = new SQLiteConnection(path);
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
            conns.CreateTable<TabelPembelian>();
            conns.CreateTable<TabelPenjualan>();
            conns.CreateTable<TabelBeban>();
            conns.CreateTable<TabelTransaksi>();
            conns.CreateTable<TabelLokasi>();
            conns.CreateTable<TabelTotal>();
            conns.CreateTable<RiwayatBayarUtang>();
            conns.CreateTable<RiwayatBayarPiutang>();
        }

        public void DumpToDatabaseIfEmpty() 
        {
            var akun = Conn().Query<TabelAkun>("SELECT * FROM TabelAkun").Count;
            var satuan = Conn().Query<TabelSatuan>("SELECT * FROM TabelSatuan").Count;
            var trans = Conn().Query<TabelTotal>("SELECT * FROM TabelTotal").Count;
            var customer = Conn().Query<TabelCustomer>("SELECT * FROM TabelCustomer").Count;
            var lokasi = Conn().Query<TabelLokasi>("SELECT * FROM TabelLokasi").Count;

            if (akun == 0)
            {
                Conn().Insert(new TabelAkun
                {
                    ID_AKUN = 1,
                    USERNAME = "admin@minnaagency",
                    KATA_SANDI = "AdminMinna",
                    ALAMAT = "Komp. Genta 1 Blok U No. 5 Depan Masjid Darussalam Batu Aji Batam",
                    JENIS_AKUN = "ADMIN",
                    NAMA_TOKO = "CV. Minna Agency Batam",
                    KONTAK = "0821-7310-4567 / 0821-7111-2121"
                });
                Conn().Insert(new TabelAkun
                {
                    ID_AKUN = 2,
                    USERNAME = "employee@minnaagency",
                    KATA_SANDI = "employee20",
                    ALAMAT = "Komp. Genta 1 Blok U No. 5 Depan Masjid Darussalam Batu Aji Batam",
                    JENIS_AKUN = "CABANG",
                    NAMA_TOKO = "CV. Minna Agency Batam",
                    KONTAK = "0821-7310-4567 / 0821-7111-2121"
                });
            }
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
                Conn().Insert(new TabelTotal() { NAMA_FIELD = "Persediaan Akhir", TOTAL = 0 });
                Conn().Insert(new TabelTotal() { NAMA_FIELD = "Ekuitas", TOTAL = 0 });
            }
            if (customer == 0) Conn().Insert(new TabelCustomer() { ID_CUSTOMER = 1, NAMA_CUSTOMER = "Customer Umum", ALAMAT_CUSTOMER = "", DISKON_TETAP = 0, KONTAK_CUSTOMER = "" });
            if (lokasi == 0) 
            {
                conns.Insert(new TabelLokasi() { ID_LOKASI = 1, NAMA_LOKASI = "Gudang" });
                conns.Insert(new TabelLokasi() { ID_LOKASI = 2, NAMA_LOKASI = "Pusat" });
                conns.Insert(new TabelLokasi() { ID_LOKASI = 3, NAMA_LOKASI = "Cabang" });
            }
        }

        private void AddUpdateSetting(string key, string value)
        {
            var pathSetting = "C:\\Program Files (x86)\\efsoftwares company\\Setup Stokuntan\\Stokuntan.exe";
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(pathSetting);
            var settings = configuration.AppSettings.Settings;
            if (settings.Count == 0 || settings[key] == null) settings.Add(key, value);
            else settings[key].Value = value;

            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configuration.AppSettings.SectionInformation.Name);
        }
    }
}
