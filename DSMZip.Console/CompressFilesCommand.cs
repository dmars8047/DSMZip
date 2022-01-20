using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DSMZip.Console
{
    public class CompressFilesCommand : Command<CompressFilesSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] CompressFilesSettings settings)
        {
            var files = new List<FileInfo>();

            foreach (var file in settings.TargetFiles)
            {
                if (File.Exists(file))
                {
                    files.Add(new FileInfo(file));
                }
                else
                {
                    throw new Exception($"Error: The file '{file}' does not exist.");
                }
            }

            string targetName = "archive.zip";

            if (!string.IsNullOrEmpty(settings.ArchiveName))
            {
                targetName = settings.ArchiveName;

                if (!targetName.EndsWith(".zip"))
                {
                    targetName += ".zip";
                }
            }

            long totalBytes = files.Sum(x => x.Length);
            long totalBytesComplete = 0;
            int archiveProgressInteger = 0;

            // Synchronous
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var archiveTask = ctx.AddTask($"[Blue]Overall Progress ({targetName.TrimStart('.').TrimStart('/')})[/]");

                    while (!ctx.IsFinished)
                    {
                        using (var zipFileStream = new FileStream(targetName, FileMode.Create, FileAccess.Write))
                        {
                            using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);

                            foreach (var file in files)
                            {
                                long fileSize = file.Length;
                                long fileBytesComplete = 0;
                                int fileProgressInteger = 0;
                                var fileTask = ctx.AddTask($"[Yellow]Compressing {file.Name}[/]");

                                ZipArchiveEntry entry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);

                                using var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);

                                using Stream entryStream = entry.Open();

                                var buffer = new byte[4096];
                                long num = fileStream.Read(buffer);

                                while (num > 0)
                                {
                                    entryStream.Write(buffer);
                                    totalBytesComplete += num;
                                    fileBytesComplete += num;

                                    if (totalBytesComplete / (double)totalBytes * 100 > archiveProgressInteger + 1)
                                    {
                                        archiveTask.Increment(1);
                                        archiveProgressInteger++;
                                    }

                                    if (fileBytesComplete / (double)fileSize * 100 > fileProgressInteger + 1)
                                    {
                                        fileTask.Increment(1);
                                        fileProgressInteger++;
                                    }

                                    num = fileStream.Read(buffer);
                                }

                                fileTask.Value(100);
                                fileTask.StopTask();
                            }
                        }

                        archiveTask.Value(100);
                        archiveTask.StopTask();
                    }
                });

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), targetName);
            var resultingFile = new FileInfo(filePath);
            var resultingDirectoryName = resultingFile.Directory.FullName;

            var table = new Table();
            table.AddColumn(new TableColumn("[yellow]File Name[/]"));
            table.AddColumn(new TableColumn("[blue]Directory[/]"));
            table.AddColumn(new TableColumn("[green]Compressed Size[/]"));
            table.AddColumn(new TableColumn("[red]Original Size[/]"));
            table.AddRow(new Markup(resultingFile.Name), new Markup(resultingDirectoryName), new Markup(Math.Round(resultingFile.Length / (double)1024) + " KB"), new Markup(Math.Round(totalBytes / (double)1024) + " KB"));
            AnsiConsole.Write(table);

            return 0;
        }
    }
}