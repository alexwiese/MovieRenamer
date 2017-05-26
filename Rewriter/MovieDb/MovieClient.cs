using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rewriter.Properties;
using TMDbLib.Client;
using TMDbLib.Objects.General;

namespace Rewriter.MovieDb
{
    public class MovieClient
    {
        private readonly TMDbClient _client = new TMDbClient(Settings.Default.ApiKey);
        private List<Genre> _genres;
        
        public async Task<MovieInfo> Search(string title, int year)
        {
            if (_genres == null)
            {
                _genres = await _client.GetMovieGenresAsync();
            }

            var search = await _client.SearchMovieAsync(title, year: year);
            var result = search.Results.FirstOrDefault();
            
            return  result != null ? new MovieInfo(result, _genres) : new UnknownMovie(title, year);
        }
    }
}