using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DSMZip.Console
{
    public class ExtractCommand : Command<ExtractSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] ExtractSettings settings)
        {
            if (!File.Exists(settings.TargetArchive))
            {
                throw new Exception($"Error: The zip archive '{settings.TargetArchive}' does not exist.");
            }

            var file = new FileInfo(settings.TargetArchive);

            if (file.Extension != ".zip")
            {
                throw new Exception($"Error: The provided archive '{settings.TargetArchive}' does not appear to be a zip file.");
            }

            var extractFolderName = string.IsNullOrEmpty(settings.ExtractFolderName) ? file.Name.TrimEnd('p').TrimEnd('i').TrimEnd('z').TrimEnd('.') : settings.ExtractFolderName;

            var extractPath = settings.ToParentDirectory ? Path.Combine(file.Directory.FullName, extractFolderName) : Path.Combine(Directory.GetCurrentDirectory(), extractFolderName);

            if (Directory.Exists(extractPath))
            {
                throw new Exception($"Error: The extract folder '{extractPath}' already exist. Use the --overwrite flag to overwrite.");
            }

            //ZipFile.ExtractToDirectory(file.FullName, extractPath, settings.Overwrite);
            using var archiveStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            using var zipArchive = new ZipArchive(archiveStream, ZipArchiveMode.Read);

            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var archiveTask = ctx.AddTask($"[Blue]Overall Extraction Progress ({extractFolderName.TrimStart('.').TrimStart('/')})[/]");

                    while (!ctx.IsFinished)
                    {
                        foreach (var entry in zipArchive.Entries)
                        {
                            using var entryStream = entry.Open();

                            //pickup here
                        }
                    }
                });

            var resultingDirectory = new DirectoryInfo(extractPath);
            var totalBytes = resultingDirectory.GetFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            var table = new Table();
            table.AddColumn(new TableColumn("[yellow]Directory Name[/]"));
            table.AddColumn(new TableColumn("[blue]Path[/]"));
            table.AddColumn(new TableColumn("[green]Compressed Size[/]"));
            table.AddColumn(new TableColumn("[red]Extracted Size[/]"));
            table.AddRow(new Markup(resultingDirectory.Name), new Markup(resultingDirectory.FullName), new Markup(Math.Round(file.Length / (double)1024) + " KB"), new Markup(Math.Round(totalBytes / (double)1024) + " KB"));

            AnsiConsole.Write(table);

            return 0;
        }
    }
}