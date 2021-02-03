using System;
using System.Linq;
using Gtk;
using GtkSharp.Hosting;
using MyApp.Data;

namespace MyApp
{
    public partial class MainWindow : Window, IMainWindow
    {
        private readonly MyDbContext _db;

        public MainWindow(MyDbContext db) : this(new Builder(@"MainWindow.glade"))
        {
            Console.WriteLine("123465");
            _db = db;
        }

        private void on_btnTest_clicked(object? sender, EventArgs e)
        {
            Console.WriteLine("123465");
            Console.WriteLine(_db.Blogs.Any());
        }
    }
}