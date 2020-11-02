using Parago.Windows;
using SQLite;
using Stokuntan.DatabaseModel;
using Stokuntan.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Stokuntan
{
    public partial class MainWindow : Window
    {
        readonly SQLiteConnection db = new IDatabase().Conn();

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                new IDatabase().DumpToDatabaseIfEmpty();
                CekBiarMasuk();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }           
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProgressDialog { Label = "Memproses Login..." };
            dialog.Show();
            var username = txtBoxEmail.Text;
            var password = txtBoxKataSandi.Password;

            new Thread(() =>
            {
                if (username != "" && password != "")
                {
                    var akun = db.Query<TabelAkun>("SELECT * FROM TabelAkun").Where(x => x.USERNAME == username && x.KATA_SANDI == password);

                    if (akun.Count() != 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Halaman_Utama window = new Halaman_Utama();
                            dialog.Close();
                            window.Show();
                            Close();
                        });
                    }
                    else Dispatcher.Invoke(() => { MessageBox.Show("Username atau Kata Sandi yang Anda Masukkan Salah", "Terjadi Kesalahan"); dialog.Close(); });
                }
                else Dispatcher.Invoke(() => { MessageBox.Show("Isi Kolom Username atau Kata Sandi", "Lengkapi Kolom"); dialog.Close(); });
            }).Start();
        }

        private void CekBiarMasuk()
        {
            var cekBiarMasuk = db.Query<TabelAkun>("select * from TabelAkun").FirstOrDefault();

            if (cekBiarMasuk != null)
            {
                if (cekBiarMasuk.BIARMASUK == true)
                {
                    checkBiarMasuk.IsChecked = true;
                    txtBoxEmail.Text = cekBiarMasuk.USERNAME;
                    txtBoxKataSandi.Password = cekBiarMasuk.KATA_SANDI;
                }
                else checkBiarMasuk.IsChecked = false;
            }
        }

        private void CheckBiarMasuk_Checked(object sender, RoutedEventArgs e)
        {
            var cekBiarMasuk = db.Query<TabelAkun>("select * from TabelAkun").FirstOrDefault();
            cekBiarMasuk.BIARMASUK = true;

            db.RunInTransaction(() => db.Update(cekBiarMasuk));
        }

        private void CheckBiarMasuk_Unchecked(object sender, RoutedEventArgs e)
        {
            var cekBiarMasuk = db.Query<TabelAkun>("select * from TabelAkun").FirstOrDefault();
            cekBiarMasuk.BIARMASUK = false;

            db.RunInTransaction(() => db.Update(cekBiarMasuk));
        }
    }
}
