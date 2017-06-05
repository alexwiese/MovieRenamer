using System;
using System.Windows.Controls;

namespace Rewriter.Logging
{
    /// <summary>
    /// Interaction logic for ViewLogControl.xaml
    /// </summary>
    public partial class ViewLogControl : UserControl
    {
        public ViewLogControl()
        {
            InitializeComponent();

            ViewLogTextBox.Text = Logger.CurrentLog;
            Logger.CurrentLogChanged += LoggerOnCurrentLogChanged;
        }

        private void LoggerOnCurrentLogChanged(string message) => Dispatcher.Invoke(() => ViewLogTextBox.AppendText(message));
    }
}
