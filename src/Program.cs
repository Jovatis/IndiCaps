namespace IndiCaps;

internal static class Program
{
    private const string SingleInstanceMutexName = "IndiCaps_SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(true, SingleInstanceMutexName, out bool createdNew);
        if (!createdNew)
            return; // an instance is already running

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        FontService.Init();
        var settings = Settings.Load();
        bool debug = Environment.GetCommandLineArgs().Contains("--debug");

        using (var app = new CapsApp(settings, debug))
        {
            Application.Run();
        }
    }
}