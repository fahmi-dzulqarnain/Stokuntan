using Stokuntan.DatabaseModel;
using Stokuntan.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stokuntan.Classes
{
    class ComboBoxItem
    {
        public List<string> Kategori { get; set; }
        public List<string> KategoriCari { get; set; }
        public List<string> KategoriJual { get; set; }
        public List<string> Supplier { get; set; }
        public List<string> Satuan { get; set; }
        public List<string> Metode { get; set; }
        public List<string> MetodeJual { get; set; }
        public List<string> SebabReject { get; set; }
        public List<string> NamaStok { get; set; }
        public List<string> Customer { get; set; }
        public List<string> JenisBeban { get; set; }
        public List<string> FieldTotal { get; set; }
        public List<string> SubOrAdd { get; set; }
        public List<string> Bulan { get; set; }
        public double ScreenWidth { get; set; }

        public ComboBoxItem() 
        {
            Kategori = new List<string>();
            Supplier = new List<string>();
            Satuan = new List<string>();
            NamaStok = new List<string>();
            Customer = new List<string>();
            SebabReject = new List<string>() { "Busuk", "Hilang", "Rusak", "Lain Lain" };
            KategoriCari = new List<string>() { "Tanggal Masuk", "Kategori", "Supplier", "Merek", "Deskripsi" };
            KategoriJual = new List<string>() { "Tanggal Transaksi", "Customer" };
            Metode = new List<string>() { "Cash / Bank", "Utang" };
            MetodeJual = new List<string>() { "Cash / Bank", "Piutang" };
            JenisBeban = new List<string>() { "Beban Gaji", "Beban Listrik / Air", "Beban Telepon / Internet", "Beban Perlengkapan" };
            FieldTotal = new List<string>() { "Pengurangan / Pengambilan Kas", "Penambahan Kas", "Penambahan Utang", "Pelunasan Utang", "Pelunasan Piutang" };
            Bulan = new List<string>() { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "November", "Desember" };
            ScreenWidth = Halaman_Utama.ScreenWidth;
            BacaDatabase();
        }

        private void BacaDatabase() 
        {
            var db = new IDatabase().Conn();
            var getKategori = db.Query<TabelKategori>("SELECT NAMA_KATEGORI FROM TabelKategori");
            var getSatuan = db.Query<TabelSatuan>("SELECT NAMA_SATUAN FROM TabelSatuan");
            var getSupplier = db.Query<TabelSupplier>("SELECT NAMA_SUPPLIER FROM TabelSupplier");
            var getNamaStok = db.Query<TabelRealStok>("SELECT KATEGORI_MEREK FROM TabelRealStok");
            var getCustomer = db.Query<TabelCustomer>("SELECT NAMA_CUSTOMER FROM TabelCustomer");

            if (getKategori.Count != 0) Kategori = getKategori.ConvertAll(x => Convert.ToString(x));
            if (getSatuan.Count != 0) Satuan = getSatuan.ConvertAll(x => x.ToString());
            if (getSupplier.Count != 0) Supplier = getSupplier.ConvertAll(x => x.ToString());
            if (getNamaStok.Count != 0) NamaStok = getNamaStok.ConvertAll(x => x.ToString());
            if (getCustomer.Count != 0) Customer = getCustomer.ConvertAll(x => x.ToString());
        }
    }
}
