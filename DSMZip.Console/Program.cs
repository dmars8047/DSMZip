using DSMZip.Console;
using Spectre.Console.Cli;

public static class Program
{
    public static void Main(string[] args)
    {
        //args = new List<string>() { "compress", "directory", "test", "--name", "poop.zip" }.ToArray();
        args = new List<string>() { "extract", "poop.zip" }.ToArray();

        var app = new CommandApp();

        app.Configure(config =>
        {
            config.AddBranch<CompressSettings>("compress", compress =>
            {
                compress.AddCommand<CompressFilesCommand>("files");
                compress.AddCommand<CompressDirectoryCommand>("directory");
            });
            config.AddCommand<ExtractCommand>("extract");
        });

        var resultCode = app.Run(args);
    }
}
