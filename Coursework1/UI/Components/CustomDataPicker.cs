using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Coursework1.Data;

namespace Coursework1.UI.Components
{
    public class CustomDatePicker : DatePicker
    {
        public static readonly DependencyProperty DisplayFormatProperty =
            DependencyProperty.Register(
                nameof(DisplayFormat),
                typeof(string),
                typeof(CustomDatePicker),
                new PropertyMetadata("dd.MM.yyyy", OnFormatChanged));

        public string DisplayFormat
        {
            get => (string)GetValue(DisplayFormatProperty);
            set => SetValue(DisplayFormatProperty, value);
        }

        private DatePickerTextBox? _textBox;
        private bool _isUpdatingText;
        
        private DateTime? _lastValidDate;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild("PART_TextBox") as DatePickerTextBox;

            if (_textBox != null)
            {
                this.SelectedDateFormat = DatePickerFormat.Long;

                _textBox.PreviewKeyDown -= TextBox_PreviewKeyDown;
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;

                _textBox.LostFocus -= TextBox_LostFocus;
                _textBox.LostFocus += TextBox_LostFocus;
                
                _lastValidDate = SelectedDate;
                
                ForceUpdateText();
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (_textBox == null) return;

                var text = _textBox.Text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (FormattedDate.TryParse(text, out var parsedDate))
                    {
                        SelectedDate = parsedDate.Value;
                        _lastValidDate = parsedDate.Value;
                        e.Handled = true;
                        
                        Dispatcher.BeginInvoke(new Action(() => ForceUpdateText()), 
                            System.Windows.Threading.DispatcherPriority.Background);
                    }
                    else
                    {
                        e.Handled = true;
                        
                        RestorePreviousValidState();
                    }
                }
                else
                {
                    SelectedDate = null;
                    _lastValidDate = null;
                    e.Handled = true;
                    Dispatcher.BeginInvoke(new Action(() => ForceUpdateText()), 
                        System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        protected override void OnSelectedDateChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectedDateChanged(e);
            
            if (SelectedDate.HasValue)
            {
                _lastValidDate = SelectedDate.Value;
            }
            else
            {
                _lastValidDate = null;
            }

            Dispatcher.BeginInvoke(new Action(() => 
            {
                if (!_isUpdatingText && _textBox != null && !_textBox.IsKeyboardFocused)
                {
                    ForceUpdateText();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        protected override void OnCalendarClosed(RoutedEventArgs e)
        {
            base.OnCalendarClosed(e);
            
            if (SelectedDate.HasValue)
            {
                _lastValidDate = SelectedDate.Value;
            }

            Dispatcher.BeginInvoke(new Action(() => 
            {
                if (_textBox != null)
                {
                    ForceUpdateText();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_textBox == null) return;

            var text = _textBox.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                SelectedDate = null;
                _lastValidDate = null;
                Dispatcher.BeginInvoke(new Action(() => ForceUpdateText()), 
                    System.Windows.Threading.DispatcherPriority.Background);
                return;
            }

            if (FormattedDate.TryParse(text, out var parsedDate))
            {
                if (SelectedDate != parsedDate.Value)
                {
                    SelectedDate = parsedDate.Value;
                    _lastValidDate = parsedDate.Value;
                }
            }
            else
            {
                RestorePreviousValidState();
            }

            Dispatcher.BeginInvoke(new Action(() => 
            {
                ForceUpdateText();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        
        private void RestorePreviousValidState()
        {
            if (_lastValidDate.HasValue)
            {
                if (SelectedDate != _lastValidDate.Value)
                {
                    SelectedDate = _lastValidDate.Value;
                }
            }
            else
            {
                if (SelectedDate.HasValue)
                {
                    SelectedDate = null;
                }
            }
            
        }

        private void ForceUpdateText()
        {
            if (_textBox == null) return;

            _isUpdatingText = true;

            try
            {
                if (SelectedDate.HasValue)
                {
                    var formatted = new FormattedDate(SelectedDate.Value);
                    var correctText = formatted.ToString();

                    if (_textBox.Text != correctText)
                    {
                        _textBox.Text = correctText;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_textBox.Text))
                    {
                        _textBox.Text = string.Empty;
                    }
                }
            }
            finally
            {
                _isUpdatingText = false;
            }
        }

        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomDatePicker picker)
            {
                picker.ForceUpdateText();
            }
        }
    }
}