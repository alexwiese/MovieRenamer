using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Rewriter.ViewModels;

namespace Rewriter.Controls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private CompletionWindow _completionWindow;

        public SettingsControl()
        {
            InitializeComponent();

            using (var reader = new XmlTextReader("Controls/Syntax.xml"))
            {
                // Load syntax highlighting
                var highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                DirectoryRuleTextEditor.SyntaxHighlighting = highlightingDefinition;
                FileRuleTextEditor.SyntaxHighlighting = highlightingDefinition;
            }

            DirectoryRuleTextEditor.TextArea.TextEntered += TextAreaOnTextEntered;
            DirectoryRuleTextEditor.TextArea.TextEntering+= TextAreaOnTextEntering;
            FileRuleTextEditor.TextArea.TextEntered += TextAreaOnTextEntered;
            FileRuleTextEditor.TextArea.TextEntering+= TextAreaOnTextEntering;
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object s, RoutedEventArgs e)
        {
            // TextEditor doesn't support binding - boo
            DirectoryRuleTextEditor.Text = ((MainViewModel) DataContext).DirectoryNamingRule;
            FileRuleTextEditor.Text = ((MainViewModel) DataContext).FileNamingRule;
        }

        private void TextAreaOnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains("\n"))
            {
                // Disable enter/new lines
                e.Handled = true;
            }

            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (e.Text[0] == '}')
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                    e.Handled = true;
                }
            }
        }

        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            if (textCompositionEventArgs.Text == "{")
            {
                // Show completion list
                _completionWindow = new CompletionWindow((TextArea) sender);
                var data = _completionWindow.CompletionList.CompletionData;

                foreach (var tokenCompletionData in TokenCompletionData.All)
                {
                    data.Add(tokenCompletionData);
                }

                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }

        private void DirectoryRuleTextEditor_OnTextChanged(object sender, EventArgs e)
        {
            ((MainViewModel) DataContext).DirectoryNamingRule = DirectoryRuleTextEditor.Text;
        }

        private void FileRuleTextEditor_OnTextChanged(object sender, EventArgs e)
        {
            ((MainViewModel)DataContext).FileNamingRule = FileRuleTextEditor.Text;
        }
    }
}
