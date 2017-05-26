using System.Windows;
using Rewriter.ViewModels;

namespace Rewriter.Views
{
    public abstract class View<TViewModel> : Window
        where TViewModel : ViewModel, new()
    {
        protected View()
        {
            ViewModel = new TViewModel();
            DataContext = ViewModel;
        }

        protected TViewModel ViewModel { get; }
    }
}