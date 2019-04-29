using RichTextEditor.Converters;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Xceed.Wpf.Toolkit;

namespace RichTextEditor.TextFormatters
{
    /// <summary>
    /// Saves the text in XAML but yields it back in HTML format.
    /// Saving is done in XAML because of limitations with HTML.
    /// </summary>
    public class HtmlFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            var textRange = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                textRange.Save(ms, DataFormats.Xaml);
                var xaml = Encoding.Default.GetString(ms.ToArray()); // TODO Get text in HTML format
                return XamlToHtmlConverter.ConvertXamlToHtml(xaml, true); // TODO Remove mail headers and add seperately.
            }
        }

        public void SetText(FlowDocument document, string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    document.Blocks.Clear();
                    return;
                }

                var textRange = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    textRange.Load(ms, DataFormats.Xaml);
                }
            }
            catch
            {
                throw new InvalidDataException("Data provided is not in the correct XAML format.");
            }
        }
    }
}
