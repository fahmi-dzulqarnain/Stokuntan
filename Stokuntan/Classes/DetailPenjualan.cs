using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stokuntan.Classes
{
    class DetailPenjualan
    {
        public string NAMA_BARANG { get; set; }
        public string JUMLAH_BELI { get; set; }
        public string HARGA_SATUAN { get; set; }
        public string DISKON { get; set; }
        public string HARGA_TOTAL { get; set; }
        public override string ToString()
        {
            return $"{NAMA_BARANG};{JUMLAH_BELI};{HARGA_SATUAN};{DISKON};{HARGA_TOTAL}";
        }
    }
}
