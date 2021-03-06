﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Rewriter.Core;
using Rewriter.Logging;
using Rewriter.MovieDb;
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
            if (ViewModel.IsRefreshingMovieCache)
            {
                ViewModel.CancelRefreshMovieInfo();
            }
            else
            {
                try
                {
                    await ViewModel.RefreshCache();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                    MessageBox.Show(this, exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RenameFilesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var sourceFile in ViewModel.SourceFiles)
                {
                    if (!ViewModel.MovieInfoCache.TryGetValue(sourceFile, out MovieInfo movieInfo))
                    {
                        await ViewModel.RefreshCache();
                        ViewModel.MovieInfoCache.TryGetValue(sourceFile, out movieInfo);
                    }

                    ViewModel.FileProcessor.MoveFiles(sourceFile, movieInfo);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                MessageBox.Show(this, exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshSourceFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsRefreshingSourceFiles)
            {
                ViewModel.CancelRefreshSourceFiles();
            }
            else
            {
                await ViewModel.RefreshSourceFiles();
            }
        }
    }
}
