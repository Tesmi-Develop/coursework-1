using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Coursework1.Data;

namespace Coursework1.UI
{
    public partial class FormattedDatePicker : UserControl
    {
        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register("SelectedDate", typeof(DateTime?), typeof(FormattedDatePicker), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public static readonly DependencyProperty InputTextProperty =
            DependencyProperty.Register("InputText", typeof(string), typeof(FormattedDatePicker), new PropertyMetadata(string.Empty));

        public string InputText
        {
            get => (string)GetValue(InputTextProperty);
            set => SetValue(InputTextProperty, value);
        }

        private System.Windows.Controls.Primitives.ButtonBase? _calendarButton;
        private bool _isSyncing = false;

        public FormattedDatePicker()
        {
            InitializeComponent();
            this.Loaded += FormattedDatePicker_Loaded;
            
            InternalPicker.CalendarOpened += InternalPicker_CalendarOpened;
        }

        private void FormattedDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            _calendarButton = InternalPicker.Template.FindName("PART_Button", InternalPicker) as System.Windows.Controls.Primitives.ButtonBase;

            if (SelectedDate.HasValue)
            {
                SyncTextFromDate(SelectedDate.Value);
            }
        }
        
        private void InternalPicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            ApplyInputToDateSilent();
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FormattedDatePicker)d;
            if (!control._isSyncing)
            {
                if (e.NewValue is DateTime date)
                {
                    control.SyncTextFromDate(date);
                }
                else
                {
                    control.InputText = string.Empty;
                }
            }
        }

        private void SyncTextFromDate(DateTime date)
        {
            _isSyncing = true;
            try
            {
                var formatted = new FormattedDate(date);
                InputText = formatted.ToString();
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void CustomInputBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_calendarButton != null)
            {
                _calendarButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                CustomInputBox.Focus();
            }
        }

        private void CustomInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyInputToDate();
        }

        private void CustomInputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                ApplyInputToDate();
                Keyboard.ClearFocus(); 
                e.Handled = true;
            }
        }
        
        private void ApplyInputToDate()
        {
            ApplyInputToDateCore(showVisualFeedback: true);
        }
        
        private void ApplyInputToDateSilent()
        {
            ApplyInputToDateCore(showVisualFeedback: false);
        }

        private void ApplyInputToDateCore(bool showVisualFeedback)
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                if (SelectedDate.HasValue)
                {
                    _isSyncing = true;
                    try
                    {
                        SelectedDate = null;
                        InternalPicker.SelectedDate = null;
                    }
                    finally { _isSyncing = false; }
                }
                return;
            }

            if (FormattedDate.TryParse(InputText, out var parsedDate))
            {
                if (SelectedDate != parsedDate.Value)
                {
                    _isSyncing = true;
                    try
                    {
                        SelectedDate = parsedDate.Value;
                        InternalPicker.SelectedDate = parsedDate.Value;
                        
                        if (showVisualFeedback)
                        {
                            SyncTextFromDate(parsedDate.Value);
                        }
                    }
                    finally { _isSyncing = false; }
                }
            }
            else
            {
                if (!InternalPicker.SelectedDate.HasValue) 
                    return;
                
                _isSyncing = true;
                try
                {
                    SelectedDate = InternalPicker.SelectedDate.Value;
                    if (showVisualFeedback)
                    {
                        SyncTextFromDate(InternalPicker.SelectedDate.Value);
                    }
                }
                finally { _isSyncing = false; }
            }
        }
    }
}