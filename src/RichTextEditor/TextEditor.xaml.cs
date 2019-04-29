using RichTextEditor.TextFormatters;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace RichTextEditor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        #region Dependency Properties

        public static DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextEditor), 
            new FrameworkPropertyMetadata { DefaultValue = string.Empty, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = UpdateSourceTrigger.LostFocus, IsAnimationProhibited = true });

        public static DependencyProperty TextFormatterProperty = DependencyProperty.Register(nameof(TextFormatter), typeof(ITextFormatter), typeof(TextEditor), new PropertyMetadata(new HtmlFormatter()));

        public static DependencyProperty TextEditorFontFamilyProperty = DependencyProperty.Register(nameof(TextEditorFontFamily), typeof(FontFamily), typeof(TextEditor), new PropertyMetadata(new FontFamily("Calibri")));

        public static DependencyProperty TextEditorFontSizeProperty = DependencyProperty.Register(nameof(TextEditorFontSize), typeof(double), typeof(TextEditor), new PropertyMetadata(11.0));

        #endregion Dependency Properties

        #region Properties

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ITextFormatter TextFormatter
        {
            get => (ITextFormatter) GetValue(TextFormatterProperty);
            set => SetValue(TextFormatterProperty, value);
        }

        public FontFamily TextEditorFontFamily
        {
            get => (FontFamily) GetValue(TextEditorFontFamilyProperty);
            set => SetValue(TextEditorFontFamilyProperty, value);
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double TextEditorFontSize
        {
            get => (double) GetValue(TextEditorFontSizeProperty);
            set => SetValue(TextEditorFontSizeProperty, value);
        }

        private static double[] FontSizes => new double[] {8.0, 9.0, 10.0, 11.0, 12.0, 14.0, 16.0, 18.0, 20.0, 22.0, 24.0, 26.0, 28.0, 36.0, 48.0, 72.0};

        #endregion Properties

        public TextEditor()
        {
            InitializeComponent();

            // Toolbar Default Control Values
            FontFamilyComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(fontFamily => fontFamily.Source);
            FontSizeComboBox.ItemsSource = FontSizes;

            FontFamilyComboBox.SelectedItem = TextEditorFontFamily;
            FontSizeComboBox.SelectedValue = TextEditorFontSize;

            FontBackgroundColorPicker.SelectedColor = Colors.Transparent;
            FontColorPicker.SelectedColor = Colors.Black;

            #region Events

            FontFamilyComboBox.SelectionChanged += FontFamilyComboBox_SelectionChanged;
            FontSizeComboBox.SelectionChanged += FontSizeComboBox_SelectionChanged;
            FontBackgroundColorPicker.SelectedColorChanged += FontBackgroundColorPicker_SelectedColorChanged;
            FontColorPicker.SelectedColorChanged += FontColorPicker_SelectedColorChanged;

            #endregion Events

            EditorRichTextBox.SelectionChanged += EditorRichTextBox_SelectionChanged;
        }

        #region Event Handlers

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ApplyPropertyValueToSelectedText(TextElement.FontFamilyProperty, (FontFamily) e.AddedItems[0]);
        }
        
        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            ApplyPropertyValueToSelectedText(TextElement.FontSizeProperty, (double) e.AddedItems[0] * (4.0/3.0)); // TODO DIU to Pt.
        }

        private void FontBackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var color = e.NewValue;
            ApplyPropertyValueToSelectedText(TextElement.BackgroundProperty, color.HasValue ? new SolidColorBrush(color.Value) : null);
        }

        private void FontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var color = e.NewValue;
            ApplyPropertyValueToSelectedText(TextElement.ForegroundProperty, color.HasValue ? new SolidColorBrush(color.Value) : null);
        }

        #endregion Event Handlers

        #region Methods

        private void UpdateSelectedFontFamily()
        {
            var value = DependencyProperty.UnsetValue;
            if (EditorRichTextBox != null && EditorRichTextBox.Selection != null)
                value = EditorRichTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);

            if (value == DependencyProperty.UnsetValue)
                return;

            var currentFontFamily = (FontFamily) value;
            if (currentFontFamily != null && FontFamilyComboBox != null)
                FontFamilyComboBox.SelectedItem = currentFontFamily;
        }

        private void UpdateSelectedFontSize()
        {
            var value = DependencyProperty.UnsetValue;
            if (EditorRichTextBox != null && EditorRichTextBox.Selection != null)
                value = EditorRichTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);

            if (value == DependencyProperty.UnsetValue)
                return;

            if (FontSizeComboBox != null)
                FontSizeComboBox.SelectedValue = (double) value / (4.0/3.0);
        }

        private void UpdateFontBackgroundColor()
        {
            // TODO Set selected color and save it so the user can select it and write in this color later on (like MS Word).
            var value = DependencyProperty.UnsetValue;
            if (EditorRichTextBox != null && EditorRichTextBox.Selection != null)
                value = EditorRichTextBox.Selection.GetPropertyValue(TextElement.BackgroundProperty);

            if (value == DependencyProperty.UnsetValue)
                return;

            var currentColor = value == null ? Colors.Transparent : (Color?) ((SolidColorBrush) value).Color;
            if (FontBackgroundColorPicker != null)
                FontBackgroundColorPicker.SelectedColor = currentColor;
        }

        private void UpdateFontColor()
        {
            // TODO Set selected color and save it so the user can select it and write in this color later on (like MS Word).
            var value = DependencyProperty.UnsetValue;
            if (EditorRichTextBox != null && EditorRichTextBox.Selection != null)
                value = EditorRichTextBox.Selection.GetPropertyValue(TextElement.ForegroundProperty);

            if (value == DependencyProperty.UnsetValue)
                return;

            Color? currentColor = value == null ? null : (Color?) ((SolidColorBrush) value).Color;
            if (FontColorPicker != null)
                FontColorPicker.SelectedColor = currentColor;
        }

        private void UpdateToggleButtonCheckedState()
        {
            UpdateItemCheckedState(BoldToggleButton, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(ItalicToggleButton, TextElement.FontStyleProperty, FontStyles.Italic);
            UpdateItemCheckedState(UnderlineToggleButton, Inline.TextDecorationsProperty, TextDecorations.Underline);
            UpdateItemCheckedState(StrikethroughToggleButton, Inline.TextDecorationsProperty, TextDecorations.Strikethrough);

            UpdateItemCheckedState(AlignLeftToggleButton, Block.TextAlignmentProperty, TextAlignment.Left);
            UpdateItemCheckedState(AlignCenterToggleButton, Block.TextAlignmentProperty, TextAlignment.Center);
            UpdateItemCheckedState(AlignRightToggleButton, Block.TextAlignmentProperty, TextAlignment.Right);
            UpdateItemCheckedState(AlignJustifyToggleButton, Block.TextAlignmentProperty, TextAlignment.Justify);

            void UpdateItemCheckedState(ToggleButton toggleButton, DependencyProperty formattingProperty, object expectedValue)
            {
                var currentValue = DependencyProperty.UnsetValue;
                if (EditorRichTextBox != null && EditorRichTextBox.Selection != null)
                    currentValue = EditorRichTextBox.Selection.GetPropertyValue(formattingProperty);

                if (currentValue == DependencyProperty.UnsetValue)
                    return;

                if (toggleButton != null)
                    toggleButton.IsChecked = currentValue == null ? false : currentValue != null && currentValue.Equals(expectedValue);
            }
        }

        private void UpdateSelectionListType()
        {
            var startParagraph = EditorRichTextBox.Selection.Start;
            var endParagraph = EditorRichTextBox.Selection.End;
            if (startParagraph == null || endParagraph == null)
                return;

            if (!(startParagraph.Parent is ListItem) || !(endParagraph.Parent is ListItem) ||
                !ReferenceEquals(((ListItem) startParagraph.Parent).List, ((ListItem) endParagraph.Parent).List))
                return;

            var markerStyle = ((ListItem) startParagraph.Parent).List.MarkerStyle;
            switch (markerStyle) // TODO Add additional list types.
            {
                case TextMarkerStyle.None:
                    break;
                case TextMarkerStyle.Disc:
                    BulletListToggleButton.IsChecked = true;
                    break;
                case TextMarkerStyle.Circle:
                    break;
                case TextMarkerStyle.Square:
                    break;
                case TextMarkerStyle.Box:
                    break;
                case TextMarkerStyle.LowerRoman:
                    break;
                case TextMarkerStyle.UpperRoman:
                    break;
                case TextMarkerStyle.LowerLatin:
                    break;
                case TextMarkerStyle.UpperLatin:
                    break;
                case TextMarkerStyle.Decimal:
                    NumberListToggleButton.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void ApplyPropertyValueToSelectedText(DependencyProperty formattingProperty, object value)
        {
            if (EditorRichTextBox == null && EditorRichTextBox.Selection == null)
                return;

            if (value is SolidColorBrush colorBrush && colorBrush.Color.Equals(Colors.Transparent))
                EditorRichTextBox.Selection.ApplyPropertyValue(formattingProperty, null);
            else
                EditorRichTextBox.Selection.ApplyPropertyValue(formattingProperty, value);
        }

        #endregion Methods

        private void EditorRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // TODO: Update FormatToolBar Control
            UpdateSelectedFontFamily();
            UpdateSelectedFontSize();
            UpdateFontBackgroundColor();
            UpdateFontColor();
            UpdateToggleButtonCheckedState();
            UpdateSelectionListType();
        }
    }
}
