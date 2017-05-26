using System.IO;
using Rewriter.MovieDb;

namespace Rewriter.Rules
{
    public delegate string FileSystemRenameRule(MovieInfo movieInfo, FileInfo sourceFileInfo);
}