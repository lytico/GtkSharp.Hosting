namespace MyApp
{
    using Gtk;
    public partial class MainWindow : global::Gtk.Window
    {
        private MainWindow(Builder builder) : base(builder.GetRawObject("root"))
        {
            builder.Autoconnect(this);
        }

        [Builder.ObjectAttribute] private Button btnTest;
    }
}