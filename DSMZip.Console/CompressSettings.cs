using Spectre.Console.Cli;

namespace DSMZip.Console
{
    public class CompressSettings : CommandSettings
    {

    }

    public class CompressFilesSettings : CompressSettings
    {
        ///<summary>
        /// Files to compress.
        /// </summary>
        [CommandArgument(0, "<FILES>")]
        public string[] TargetFiles { get; set; }

        ///<summary>
        /// Name of the resulting zip archive.
        /// </summary>
        [CommandOption("--name")]
        public string ArchiveName { get; set; }
    }

    public class CompressDirectorySettings : CompressSettings
    {
        ///<summary>
        /// Directory to compress.
        /// </summary>
        [CommandArgument(0, "<DIRECTORY>")]
        public string TargetDirectory { get; set; }

        ///<summary>
        /// Name of the resulting zip archive.
        /// </summary>
        [CommandOption("--name")]
        public string ArchiveName { get; set; }
    }
}
