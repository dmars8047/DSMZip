using Spectre.Console.Cli;

namespace DSMZip.Console
{
    public class ExtractSettings : CommandSettings
    {
        /// <summary>
        /// Archave to extract.
        /// </summary>
        [CommandArgument(0, "<ZipArchive>")]
        public string TargetArchive { get; set; }

        /// <summary>
        /// Name of the directory to create.
        /// </summary>
        [CommandOption("--name")]
        public string ExtractFolderName { get; set; }

        /// <summary>
        /// Will change the extraction location to the parent directory of the target archive.
        /// </summary>
        [CommandOption("--to-parent-directory")]
        public bool ToParentDirectory { get; set; }

        /// <summary>
        /// Will overwrite the resulting folder if a folder of the same path already exists.
        /// </summary>
        [CommandOption("--overwrite")]
        public bool Overwrite { get; set; }
    }
}
