using System;
using System.IO;
using System.Linq;
using Rewriter.MovieDb;

namespace Rewriter.Rules
{
    public class MovieRenameRule
    {
        private readonly string _directoryTemplate;
        private readonly string _fileTemplate;

        public MovieRenameRule(string directoryTemplate, string fileTemplate)
        {
            if (string.IsNullOrWhiteSpace(directoryTemplate))
                throw new ArgumentException(nameof(directoryTemplate));
            if (string.IsNullOrWhiteSpace(fileTemplate))
                throw new ArgumentException(nameof(fileTemplate));

            _directoryTemplate = directoryTemplate;
            _fileTemplate = fileTemplate;
        }

        public string GenerateDirectoryName(MovieInfo movieInfo, FileInfo sourceFile)
            => GenerateFileSystemName(_directoryTemplate, movieInfo, sourceFile);

        public string GenerateFileName(MovieInfo movieInfo, FileInfo sourceFile)
            => GenerateFileSystemName(_fileTemplate, movieInfo, sourceFile);

        private static string GenerateFileSystemName(string template, MovieInfo movieInfo, FileSystemInfo sourceFile)
        {
            object GetTokenValue(Token token)
            {
                return token.ValueFactory(movieInfo, sourceFile) ?? "Unknown";
            }

            template = template ?? string.Empty;

            for (var i = 0; i < Tokens.All.Length; i++)
            {
                var token = Tokens.All[i];
                template = template.Replace($"{{{token.Selector}", $"{{{i}");
            }

            return string.Format(template, Tokens.All.Select(GetTokenValue).ToArray());
        }
    }
}