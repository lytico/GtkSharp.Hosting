using System;
using Gtk;
using GtkSharp.Hosting;

namespace MyApp
{
    public partial class MainWindow : Window, IMainWindow
    {
        public MainWindow() : this(new Builder(@"MainWindow.glade"))
        {
        }

        private void on_btnTest_clicked(object? sender, EventArgs e)
        {
            Console.WriteLine("123465");
        }
    }
}