using System;
using System.IO;
using Rewriter.MovieDb;

namespace Rewriter.Rules
{
    public class Token
    {
        public Token(string selector, string description, Func<MovieInfo, FileSystemInfo, object> valueFactory)
        {
            Selector = selector;
            Description = description;
            ValueFactory = valueFactory;
        }

        public string Selector { get; }
        public string Description { get; }
        public Func<MovieInfo, FileSystemInfo, object> ValueFactory { get; }
    }
}