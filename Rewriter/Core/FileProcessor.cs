﻿using System.IO;
using System.Linq;
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

        public void Process(string sourceFile, MovieInfo movieInfo)
        {
            var destinationFile = GetRelocatedFilePath(sourceFile, movieInfo);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);

            if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            File.Copy(sourceFile, destinationFile, true);
        }

        public string GetRelocatedFilePath(string sourceFile, MovieInfo movieInfo)
        {
            var sourceFileInfo = new FileInfo(sourceFile);

            var destinationDirectory = Path.Combine(_baseDirectory, _directoryRule(movieInfo, sourceFileInfo));
            var destinationFile = Path.Combine(destinationDirectory, _fileRule(movieInfo, sourceFileInfo));

           return Sanitize(destinationFile);
        }

        private static string Sanitize(string input)
        {
            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars().Except(new [] {'\\', '/'}))
            {
                input = input.Replace(invalidFileNameChar.ToString(), "");
            }
            return input;
        }
    }
}