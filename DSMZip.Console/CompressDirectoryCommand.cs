using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace DSMZip.Console
{
    public class CompressDirectoryCommand : Command<CompressDirectorySettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] CompressDirectorySettings settings)
        {
            if (!Directory.Exists(settings.TargetDirectory))
            {
                throw new Exception($"Error: The directory '{settings.TargetDirectory}' does not exist.");
            }

            var targetDirectory = new DirectoryInfo(settings.TargetDirectory);
            string archiveName = settings.TargetDirectory.TrimEnd('\\');

            if (!string.IsNullOrEmpty(settings.ArchiveName))
            {
                archiveName = targetDirectory.Name;
            }

            if (!archiveName.EndsWith(".zip"))
            {
                archiveName = archiveName + ".zip";
            }

            var archiveFile = new FileInfo(settings.ToParentDirectory ? Path.Combine(targetDirectory.Parent.FullName, Path.GetFileName(archiveName)) : Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(archiveName)));

            var dirObjects = new List<FileSystemInfo>();
            dirObjects = targetDirectory.GetFileSystemInfos("*", SearchOption.AllDirectories).OrderBy(x => x.FullName).ToList();

            long totalBytes = targetDirectory.GetFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            long totalBytesComplete = 0;
            int archiveProgressInteger = 0;

            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var archiveTask = ctx.AddTask($"[Blue]Overall Progress ({archiveFile.Name})[/]");

                    while (!ctx.IsFinished)
                    {
                        using (var zipFileStream = new FileStream(archiveFile.FullName, FileMode.Create, FileAccess.Write))
                        {
                            using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);

                            foreach (var dirObject in dirObjects)
                            {
                                if (dirObject.Attributes == FileAttributes.Directory)
                                {
                                    var dir = new DirectoryInfo(dirObject.FullName);
                                    var entryName = dir.FullName[targetDirectory.FullName.Length..];
                                    entryName = entryName.TrimStart('\\');
                                    ZipArchiveEntry entry = archive.CreateEntry(entryName + @"\", CompressionLevel.Optimal);
                                }
                                else
                                {
                                    var file = new FileInfo(dirObject.FullName);
                                    long fileSize = file.Length;
                                    long fileBytesComplete = 0;
                                    int fileProgressInteger = 0;

                                    var entryName = file.FullName[targetDirectory.FullName.Length..];
                                    entryName = entryName.TrimStart('\\');

                                    var fileTask = ctx.AddTask($"[Yellow]Compressing {file.Name}[/]");

                                    ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

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
                        }

                        archiveTask.Value(100);
                        archiveTask.StopTask();
                    }
                });

            var table = new Table();
            table.AddColumn(new TableColumn("[yellow]File Name[/]"));
            table.AddColumn(new TableColumn("[blue]Directory[/]"));
            table.AddColumn(new TableColumn("[green]Compressed Size[/]"));
            table.AddColumn(new TableColumn("[red]Original Size[/]"));
            table.AddRow(new Markup(archiveFile.Name), new Markup(archiveFile.Directory.FullName), new Markup(Math.Round(archiveFile.Length / (double)1024) + " KB"), new Markup(Math.Round(totalBytes / (double)1024) + " KB"));
            AnsiConsole.Write(table);

            return 0;
        }
    }
}
