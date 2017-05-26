using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Rewriter.Core;
using Rewriter.Properties;
using Rewriter.ViewModels;

namespace Rewriter.Views
{
    public partial class MainWindow : View<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Settings.Default.Save();
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel.Changes.CollectionChanged += (o, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var newItem in args.NewItems)
                    {
                        if (newItem is FileUpdateOperation change)
                        {
                            change.PropertyChanged += (_, __) => ResizeGridViewColumns();
                        }
                    }
                }

                ResizeGridViewColumns();
            };

            await ViewModel.RefreshSourceFiles();
        }

        private void ResizeGridViewColumns()
        {
            foreach (var column in MainGridView.Columns)
            {
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                }

                column.Width = double.NaN;
            }
        }

        private async void GetMovieInfoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.RefreshCache();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenameFilesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var sourceFile in ViewModel.SourceFiles)
            {
                ViewModel.FileProcessor.Process(sourceFile, ViewModel.MovieInfoCache[sourceFile]);
            }
        }

        private async void RefreshSourceFilesButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RefreshSourceFiles();
        }
    }
}
