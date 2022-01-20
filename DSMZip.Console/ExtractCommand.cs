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

            var archiveFile = new FileInfo(settings.TargetArchive);

            if (archiveFile.Extension != ".zip")
            {
                throw new Exception($"Error: The provided archive '{settings.TargetArchive}' does not appear to be a zip file.");
            }

            var extractFolderName = string.IsNullOrEmpty(settings.ExtractFolderName) ? archiveFile.Name.TrimEnd('p').TrimEnd('i').TrimEnd('z').TrimEnd('.') : settings.ExtractFolderName;

            var extractPath = settings.ToParentDirectory ? Path.Combine(archiveFile.Directory.FullName, extractFolderName) : Path.Combine(Directory.GetCurrentDirectory(), extractFolderName);

            System.Console.WriteLine($"Path: {extractPath}");
            System.Console.WriteLine($"To Parent: {settings.ToParentDirectory}");

            if (!settings.Overwrite && Directory.Exists(extractPath))
            {
                throw new Exception($"Error: The extract folder '{extractPath}' already exist. Use the --overwrite flag to overwrite.");
            }
            else if (settings.Overwrite && Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            var extractDirectory = Directory.CreateDirectory(extractPath);

            long totalBytesComplete = 0;
            int archiveExtractionProgressInteger = 0;

            using var archiveStream = new FileStream(archiveFile.FullName, FileMode.Open, FileAccess.Read);
            using var zipArchive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
            long totalBytes = zipArchive.Entries.Sum(x => x.Length);

            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var archiveTask = ctx.AddTask($"[Blue]Overall Extraction Progress ({extractFolderName.TrimStart('.').TrimStart('/')})[/]");

                    while (!ctx.IsFinished)
                    {
                        foreach (var entry in zipArchive.Entries.OrderBy(x => x.FullName))
                        {
                            if (entry.FullName.EndsWith('\\'))
                            {
                                Directory.CreateDirectory(Path.Combine(extractDirectory.FullName, entry.FullName));
                                continue;
                            }

                            long entryBytesComplete = 0;
                            long entrySize = entry.Length;
                            int entryProgressInteger = 0;

                            var entryTask = ctx.AddTask($"[Yellow]Extracting {entry.Name}[/]");

                            using var entryStream = entry.Open();
                            using var fileStream = new FileStream(Path.Combine(extractDirectory.FullName, entry.FullName), FileMode.Create, FileAccess.Write);

                            var buffer = new byte[4096];
                            long num = entryStream.Read(buffer);

                            while (num > 0)
                            {
                                fileStream.Write(buffer);
                                totalBytesComplete += num;
                                entryBytesComplete += num;

                                if (totalBytesComplete / (double)totalBytes * 100 > archiveExtractionProgressInteger + 1)
                                {
                                    archiveTask.Increment(1);
                                    archiveExtractionProgressInteger++;
                                }

                                if (entryBytesComplete / (double)entrySize * 100 > entryProgressInteger + 1)
                                {
                                    entryTask.Increment(1);
                                    entryProgressInteger++;
                                }

                                num = entryStream.Read(buffer);
                            }

                            entryTask.Value(100);
                            entryTask.StopTask();
                        }

                        archiveTask.Value(100);
                        archiveTask.StopTask();
                    }
                });

            var resultingDirectory = new DirectoryInfo(extractPath);

            var table = new Table();
            table.AddColumn(new TableColumn("[yellow]Directory Name[/]"));
            table.AddColumn(new TableColumn("[blue]Path[/]"));
            table.AddColumn(new TableColumn("[green]Compressed Size[/]"));
            table.AddColumn(new TableColumn("[red]Extracted Size[/]"));
            table.AddRow(new Markup(resultingDirectory.Name), new Markup(resultingDirectory.FullName), new Markup(Math.Round(archiveFile.Length / (double)1024) + " KB"), new Markup(Math.Round(totalBytes / (double)1024) + " KB"));

            AnsiConsole.Write(table);

            return 0;
        }
    }
}