using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Rewriter.Core;
using Rewriter.MovieDb;
using Rewriter.Properties;
using Rewriter.Rules;

namespace Rewriter.ViewModels
{
    public class MainViewModel : ViewModel
    {
        private string _destinationDirectory = Settings.Default.LastDestinationDirectory;
        private string _sourceDirectory = Settings.Default.LastSourceDirectory;
        private string _directoryNamingRule = Settings.Default.LastDirectoryNamingTemplate;
        private string _fileNamingRule = Settings.Default.LastFileNamingTemplate;
        public HashSet<string> SourceFiles { get; } = new HashSet<string>();

        public string DestinationDirectory
        {
            get => _destinationDirectory;
            set
            {
                if (value == _destinationDirectory) return;
                _destinationDirectory = value;
                Settings.Default.LastDestinationDirectory = value;
                OnPropertyChanged();
            }
        }

        public string DirectoryNamingRule
        {
            get => _directoryNamingRule;
            set
            {
                if (value == _directoryNamingRule) return;
                _directoryNamingRule = value;
                Settings.Default.LastDirectoryNamingTemplate = value;
                OnPropertyChanged();
                OnNamingRuleChanged();
            }
        }

        public string FileNamingRule
        {
            get => _fileNamingRule;
            set
            {
                if (value == _fileNamingRule) return;
                _fileNamingRule = value;
                Settings.Default.LastFileNamingTemplate = value;
                OnPropertyChanged();
                OnNamingRuleChanged();
            }
        }

        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                if (value == _sourceDirectory) return;
                _sourceDirectory = value;
                Settings.Default.LastSourceDirectory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileUpdateOperation> Changes { get; } = new ObservableCollection<FileUpdateOperation>();
        public MovieRenameRule MovieRenameRule => new MovieRenameRule(DirectoryNamingRule, FileNamingRule);
        public FileProcessor FileProcessor => new FileProcessor(DestinationDirectory, MovieRenameRule.GenerateDirectoryName, MovieRenameRule.GenerateFileName);
        public MovieClient MovieClient { get; } = new MovieClient();
        public ConcurrentDictionary<string, MovieInfo> MovieInfoCache { get; } = new ConcurrentDictionary<string, MovieInfo>();

        public async Task Refresh()
        {
            await RefreshSourceFiles();
            await RefreshCache();
        }

        public async Task RefreshCache()
        {
            foreach (var sourceFile in SourceFiles)
            {
                var regexMatch = Regex.Match(Path.GetFileNameWithoutExtension(sourceFile), @"^(?<title>.+?)(?<year>\d{4}|$)", RegexOptions.Compiled);

                var titleFromFileName = regexMatch
                    .Groups["title"]
                    .Value
                    .Replace('_', ' ')
                    .Replace('.', ' ');

                var yearFromFileName = regexMatch
                    .Groups["year"]
                    .Value
                    .Replace('_', ' ')
                    .Replace('.', ' ');

                int.TryParse(yearFromFileName, out int year);

                var movieInfo = await MovieClient.Search(titleFromFileName, year);
                MovieInfoCache[sourceFile] = movieInfo;

                var change = Changes.First(c => c.SourceFilePath == sourceFile);
                change.MovieInfo = movieInfo;

                try
                {
                    change.DestinationFilePath = FileProcessor.GetRelocatedFilePath(sourceFile, movieInfo);
                }
                catch
                {
                }
            }
        }

        public async Task RefreshSourceFiles()
        {
            SourceFiles.Clear();
            Changes.Clear();

            if(string.IsNullOrWhiteSpace(SourceDirectory) || !Directory.Exists(SourceDirectory))
                return;

            await Task.Run(() =>
            {
                var sourceFiles = Directory.EnumerateFiles(SourceDirectory, "*.*", SearchOption.AllDirectories)
                    .Where(s => Settings.Default.VideoFileExtensions.OfType<string>()
                        .Any(ext => s.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)));

                foreach (var sourceFile in sourceFiles)
                {
                    var fileInfo = new FileInfo(sourceFile);

                    // Filter out 'sample' files
                    if (fileInfo.Name.StartsWith("sample", StringComparison.InvariantCultureIgnoreCase) || fileInfo.Length < 50 * 1024 * 1024)
                        continue;

                    SourceFiles.Add(sourceFile);
                    Application.Current.Dispatcher.Invoke(() => Changes.Add(new FileUpdateOperation(sourceFile)));
                }
            });
        }

        private void OnNamingRuleChanged()
        {
            foreach (var sourceFile in SourceFiles)
            {
                var change = Changes.First(c => c.SourceFilePath == sourceFile);

                try
                {
                    change.DestinationFilePath = FileProcessor.GetRelocatedFilePath(sourceFile, MovieInfoCache[sourceFile]);
                }
                catch
                {
                    return;
                }
            }
        }
    }
}