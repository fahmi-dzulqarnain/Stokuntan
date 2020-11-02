using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace Stokuntan
{
    public partial class App : Application
    {
        static readonly string lokasiFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static readonly string namaDatabase = "stokuntan.db";
        public static string lokasiDatabase = Path.Combine(lokasiFolder, namaDatabase);
    }
}
