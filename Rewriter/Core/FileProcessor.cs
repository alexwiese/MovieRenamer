using System;
using System.IO;
using System.Linq;
using Rewriter.Logging;
using Rewriter.MovieDb;
using Rewriter.Rules;

namespace Rewriter.Core
{
    public class FileProcessor
    {
        private readonly string _baseDirectory;
        private readonly FileSystemRenameRule _directoryRule;
        private readonly FileSystemRenameRule _fileRule;

        public FileProcessor(string baseDirectory, FileSystemRenameRule directoryRule, FileSystemRenameRule fileRule)
        {
            _baseDirectory = baseDirectory;
            _directoryRule = directoryRule;
            _fileRule = fileRule;
        }

        public void MoveFiles(string sourceFile, MovieInfo movieInfo)
        {
            var destinationFile = GetRelocatedFilePath(sourceFile, movieInfo);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);

            if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
            {
                Logger.Info($"Creating directory '{destinationDirectory}'");
                Directory.CreateDirectory(destinationDirectory);
            }

            if (sourceFile.Equals(destinationFile, StringComparison.InvariantCultureIgnoreCase))
                return;

            Logger.Info($"Moving file from '{sourceFile}' to '{destinationDirectory}'");
            File.Move(sourceFile, destinationFile);
        }

        public string GetRelocatedFilePath(string sourceFile, MovieInfo movieInfo)
        {
            var sourceFileInfo = new FileInfo(sourceFile);

            var directoryRuleResult = Sanitize(_directoryRule(movieInfo, sourceFileInfo));
            var destinationDirectory = Path.Combine(_baseDirectory, directoryRuleResult);

            var fileRuleResult = Sanitize(_fileRule(movieInfo, sourceFileInfo));
            var destinationFile = Path.Combine(destinationDirectory, fileRuleResult);

            return Sanitize(destinationFile);
        }

        private static string Sanitize(string input)
        {
            bool IsUnc(string path) => path?.StartsWith(@"\\") == true;

            var pathRoot = IsUnc(input) ? string.Empty : Path.GetPathRoot(input.Substring(0, 3));

            input = input.Substring(pathRoot.Length);

            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars().Except(new[] { '\\', '/' }))
            {
                input = input.Replace(invalidFileNameChar.ToString(), "");
            }

            return pathRoot + input;
        }
    }
}