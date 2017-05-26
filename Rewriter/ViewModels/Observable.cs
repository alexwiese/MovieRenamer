using System.ComponentModel;
using System.Runtime.CompilerServices;
using Rewriter.Properties;

namespace Rewriter.ViewModels
{
    public abstract class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}