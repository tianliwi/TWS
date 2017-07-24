namespace IBHistoricalConsole
{
    class Program 
	{
        static void Main(string[] args)
        {
            Downloader downloader = new Downloader();
            downloader.Run();
        }
    }
}
