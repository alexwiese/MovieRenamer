using System.IO;

namespace Rewriter.Rules
{
    public static class Tokens
    {
        public static Token[] All =
        {
            new Token("title", "The name of the movie", (metadata, sourceFileInfo) => metadata.Title),
            new Token("original-title", "The original name of the movie", (metadata, sourceFileInfo) => metadata.OriginalTitle),
            new Token("release-date", "The release date of the movie", (metadata, sourceFileInfo) => metadata.ReleaseDate),
            new Token("year", "The year of the movie", (metadata, sourceFileInfo) => metadata.ReleaseDate?.Year),
            new Token("genre", "The primary genre of the movie", (metadata, sourceFileInfo) => metadata.PrimaryGenre),
            new Token("source-name", "The name of the source file", (metadata, sourceFileInfo) => sourceFileInfo.Name),
            new Token("source-extension", "The extension of the source file", (metadata, sourceFileInfo) => sourceFileInfo.Extension),
            new Token("source-directory", "The directory of the source file", (metadata, sourceFileInfo) => Path.GetDirectoryName(sourceFileInfo.FullName)),
        };
    }
}