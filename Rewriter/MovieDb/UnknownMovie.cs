using System;

namespace Rewriter.MovieDb
{
    public class UnknownMovie : MovieInfo
    {
        public bool IsUnknownMovie { get; } = true;

        public UnknownMovie(string title, int year)
            : base(title, year == 0 ? (DateTime?)null: new DateTime(year, 1, 1))
        {
        }
    }
}