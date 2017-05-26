using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Rewriter.Commands;
using Rewriter.Core;
using Rewriter.MovieDb;
using Rewriter.Properties;
using Rewriter.Rules;

namespace Rewriter.ViewModels
{
    public class MainViewModel : Observable
    {
        private string _destinationDirectory = Settings.Default.LastDestinationDirectory;
        private string _sourceDirectory = Settings.Default.LastSourceDirectory;
        private string _directoryNamingRule = Settings.Default.LastDirectoryNamingTemplate;
        private string _fileNamingRule = Settings.Default.LastFileNamingTemplate;
        private bool _isRefreshingSourceFiles;
        private bool _isRefreshingMovieCache;

        private CancellationTokenSource _refreshSourceFilesCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _refreshMovieInfoFilesCancellationTokenSource = new CancellationTokenSource();


        public MainViewModel()
        {
            BrowseSourceFolderCommand = new RelayCommand(BrowseSourceFolder);
            BrowseDestinationFolderCommand = new RelayCommand(BrowseDestinationFolder);
        }

        public bool IsRefreshingSourceFiles
        {
            get => _isRefreshingSourceFiles;
            set
            {
                if (value == _isRefreshingSourceFiles) return;
                _isRefreshingSourceFiles = value;
                OnPropertyChanged();
            }
        }

        public bool IsRefreshingMovieCache
        {
            get => _isRefreshingMovieCache;
            set
            {
                if (value == _isRefreshingMovieCache) return;
                _isRefreshingMovieCache = value;
                OnPropertyChanged();
            }
        }

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

        public ICommand BrowseSourceFolderCommand { get; }
        public ICommand BrowseDestinationFolderCommand { get; }

        public async Task Refresh()
        {
            await RefreshSourceFiles();
            await RefreshCache();
        }

        public async Task RefreshCache()
        {
            if (IsRefreshingSourceFiles)
            {
                CancelRefreshSourceFiles();
            }

            using (new BusyContext(() => IsRefreshingMovieCache))
            {
                var cancellationToken = _refreshMovieInfoFilesCancellationTokenSource.Token;

                try
                {
                    foreach (var sourceFile in SourceFiles)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        
                        if (!MovieInfoCache.TryGetValue(sourceFile, out MovieInfo movieInfo))
                        {
                            var regexMatch = Regex.Match(Path.GetFileNameWithoutExtension(sourceFile),
                                @"^(?<title>.+?)(?<year>\d{4}|$)", RegexOptions.Compiled);

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

                            movieInfo = await MovieClient.Search(titleFromFileName, year);
                            MovieInfoCache[sourceFile] = movieInfo;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
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
                catch (OperationCanceledException)
                {
                }
            }
        }

        public void CancelRefreshSourceFiles() => CancelTokenSource(ref _refreshSourceFilesCancellationTokenSource);

        public void CancelRefreshMovieInfo() => CancelTokenSource(ref _refreshMovieInfoFilesCancellationTokenSource);

        private static void CancelTokenSource(ref CancellationTokenSource cancellationTokenSource)
        {
            var current = cancellationTokenSource;
            cancellationTokenSource = new CancellationTokenSource();
            current.Cancel();
            current.Dispose();
        }

        public async Task RefreshSourceFiles()
        {
            if (IsRefreshingMovieCache)
            {
                CancelRefreshMovieInfo();
            }

            using (new BusyContext(() => IsRefreshingSourceFiles))
            {
                var cancellationToken = _refreshSourceFilesCancellationTokenSource.Token;

                SourceFiles.Clear();
                Changes.Clear();

                if (string.IsNullOrWhiteSpace(SourceDirectory) || !Directory.Exists(SourceDirectory))
                    return;

                try
                {
                    await Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var sourceFiles = Directory.EnumerateFiles(SourceDirectory, "*.*", SearchOption.AllDirectories)
                            .Where(s => Settings.Default.VideoFileExtensions.OfType<string>()
                                .Any(ext => s.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)));

                        foreach (var sourceFile in sourceFiles)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var fileInfo = new FileInfo(sourceFile);

                            // Filter out 'sample' files
                            if (fileInfo.Name.StartsWith("sample", StringComparison.InvariantCultureIgnoreCase) ||
                                fileInfo.Length < 50 * 1024 * 1024)
                                continue;

                            SourceFiles.Add(sourceFile);
                            Application.Current.Dispatcher.Invoke(() => Changes.Add(new FileUpdateOperation(sourceFile)));
                        }
                    }, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private void BrowseSourceFolder(object o)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                SourceDirectory = dialog.FileName;
            }
        }

        private void BrowseDestinationFolder(object obj)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                DestinationDirectory = dialog.FileName;
            }
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