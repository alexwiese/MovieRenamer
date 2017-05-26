using System.Windows;
using Rewriter.ViewModels;

namespace Rewriter.Views
{
    public abstract class View<TViewModel> : Window
        where TViewModel : Observable, new()
    {
        protected View()
        {
            ViewModel = new TViewModel();
            DataContext = ViewModel;
        }

        protected TViewModel ViewModel { get; }
    }
}