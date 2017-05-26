using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Rewriter.MovieDb;
using Rewriter.Properties;

namespace Rewriter.Core
{
    public class FileUpdateOperation : INotifyPropertyChanged
    {
        private string _destinationFilePath;
        private MovieInfo _movieInfo;
        private string _truncatedSourceFilePath;

        public FileUpdateOperation(string sourceFilePath)
        {
            SourceFilePath = sourceFilePath;
            TruncatedSourceFilePath = sourceFilePath;
        }

        public string SourceFilePath { get; }

        public string TruncatedSourceFilePath
        {
            get => _truncatedSourceFilePath;
            private set
            {
                if (value == _truncatedSourceFilePath) return;
                _truncatedSourceFilePath = value;
                OnPropertyChanged();
            }
        }

        public string DestinationFilePath
        {
            get => _destinationFilePath;
            set
            {
                if (Equals(value, _destinationFilePath)) return;
                _destinationFilePath = value;
                OnPropertyChanged();

                var lcs = GetLongestCommonSubstring(SourceFilePath, value);

                if (lcs.Length > 0)
                {
                    TruncatedSourceFilePath = SourceFilePath.Replace(lcs, string.Empty);
                    DestinationFilePath = DestinationFilePath.Replace(lcs, string.Empty);
                }
            }
        }

        public MovieInfo MovieInfo
        {
            get => _movieInfo;
            set
            {
                if (Equals(value, _movieInfo)) return;
                _movieInfo = value;
                OnPropertyChanged();
            }
        }

        private static string GetLongestCommonSubstring(string left, string right)
        {
            var commonSubstrings = new HashSet<string>(GetSubstrings(left));

            commonSubstrings.IntersectWith(GetSubstrings(right));

            if (commonSubstrings.Count == 0)
                return string.Empty;

            return commonSubstrings
                .OrderByDescending(s => s.Length)
                .First();
        }

        private static IEnumerable<string> GetSubstrings(string str)
        {
            for (var i = 1; i <= str.Length; i++)
            {
                yield return str.Substring(0, i);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}