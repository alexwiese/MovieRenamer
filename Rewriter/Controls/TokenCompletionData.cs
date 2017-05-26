using System;
using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Rewriter.Rules;

namespace Rewriter.Controls
{
    public class TokenCompletionData : ICompletionData
    {
        public static TokenCompletionData[] All =
            Tokens.All.Select(t => new TokenCompletionData(t)).ToArray();

        public TokenCompletionData(Token token)
        {
            Text = token.Selector;
            Description = token.Description;
        }

        public System.Windows.Media.ImageSource Image => null;

        public string Text { get; }

        public object Content => Text;

        public object Description { get; }

        public double Priority { get; }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text + "}");
        }
    }
}