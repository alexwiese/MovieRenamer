using System;
using System.Collections.Generic;
using System.Linq;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace Rewriter.MovieDb
{
    public class MovieInfo
    {
        public MovieInfo(SearchMovie searchMovie, IEnumerable<Genre> genres)
        {
            Adult = searchMovie.Adult;
            ReleaseDate = searchMovie.ReleaseDate;
            Title = searchMovie.Title;
            OriginalTitle = searchMovie.OriginalTitle;
            Video = searchMovie.Video;
            PrimaryGenre = searchMovie.GenreIds.Any() ? genres.FirstOrDefault(g => g.Id == searchMovie.GenreIds[0])?.Name : null;
        }

        protected MovieInfo(string title, DateTime? releaseDate)
        {
            Title = title;
            ReleaseDate = releaseDate;
        }

        public bool Adult { get; }
        public string OriginalTitle { get; }
        public DateTime? ReleaseDate { get; }
        public string Title { get; }
        public bool Video { get; }
        public string PrimaryGenre { get; }
    }
}