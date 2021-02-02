namespace GtkSharp.Hosting
{
    public interface IHostingStartup
    {
        void Configure(IGtkHostBuilder builder);
    }
}