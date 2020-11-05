using Stokuntan.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stokuntan.Windows
{
    public partial class Alihkan_Stok : Window
    {
        public Alihkan_Stok()
        {
            InitializeComponent();
            DataContext = new Classes.ComboBoxItem();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            var db = new IDatabase().Conn();
            var stok = cmbBoxNamaStok.SelectedItem.ToString();
            var jumlah = txtBoxJumlahStok.Text;
            var satuan = cmbBoxSatuan.SelectedItem.ToString();
            var lokasiAsal = cmbBoxLokasiAsal.SelectedItem.ToString();
            var lokasi = cmbBoxLokasi.SelectedItem.ToString();

            try
            {
                if (stok != "" && int.TryParse(jumlah, out int jml) && satuan != "" && lokasi != "") 
                {
                    var cekStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND LOKASI = ?", stok, lokasiAsal);
                    var totalStok = db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", satuan).FirstOrDefault().DALAM_GRAM * jml;

                    if (cekStok.Count != 0) 
                    {
                        var finalStok = cekStok.FirstOrDefault();
                        if (finalStok.STOK_DLM_GRAM < totalStok) MessageBox.Show("Jumlah stok yang akan dipindahkan melebihi jumlah stok yang ada", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
                        else 
                        {
                            var lokasiAlih = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok WHERE KATEGORI_MEREK = ? AND LOKASI = ?", stok, lokasi);
                            var totalStokNew = (double)totalStok / db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", finalStok.SATUAN_STOK).FirstOrDefault().DALAM_GRAM;

                            if (lokasiAlih.Count == 0)
                            {
                                var result = MessageBox.Show($"Di lokasi yang Anda pilih belum ada stok dengan nama barang {stok}. Apakah Anda ingin membuatnya?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (result == MessageBoxResult.Yes)
                                {
                                    var id = 1;
                                    var getStok = db.Query<TabelRealStok>("SELECT * FROM TabelRealStok");
                                    if (getStok.Count != 0) id = getStok.Max(x => x.ID_STOK_END) + 1;
                                    db.Insert(new TabelRealStok() { ID_STOK_END = id, KATEGORI_MEREK = stok, HARGA_JUAL_PER = finalStok.HARGA_JUAL_PER, LOKASI = lokasi, SATUAN_STOK = satuan, STOK_DLM_GRAM = totalStok, TOTAL_STOK = jml });
                                }
                            }
                            else 
                            {
                                var updated = lokasiAlih.FirstOrDefault();
                                totalStokNew = (double)totalStok / db.Query<TabelSatuan>("SELECT * FROM TabelSatuan WHERE NAMA_SATUAN = ?", updated.SATUAN_STOK).FirstOrDefault().DALAM_GRAM;
                                updated.STOK_DLM_GRAM += totalStok;
                                updated.TOTAL_STOK += totalStokNew;
                                db.RunInTransaction(() => db.Update(updated));
                            }

                            finalStok.TOTAL_STOK -= totalStokNew;
                            finalStok.STOK_DLM_GRAM -= totalStok;
                            db.RunInTransaction(() => db.Update(finalStok));
                            DialogResult = true;
                        }
                    }
                    else MessageBox.Show("Tidak Ada Stok yang Ditemukan pada lokasi asal terpilih", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else MessageBox.Show("Ada kolom yang belum terisi, Atau pastikan jumlah hanya menggunakan nomor", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show("Ada sesuatu yang salah", "Mohon Diperhatikan!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBatal_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
