using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Control = System.Windows.Forms.Control;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace _BUILD_LogWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _exclusive;
        private bool _autoScroll;
        
        private volatile int _count, _size;
        private Log.Level _filterLevel = Log.Level.Trace;
        private readonly ICollection<LogListViewItem> _currentLogListViewItems = new List<LogListViewItem>();

        public MainWindow()
        {            
            InitializeComponent();  
          
            Log.NewMessage += AddMessage;
            Log.ChangedLoggingLevel += RefreshLogLevel;            

            SetLogLevelComboBox();           
            RefreshListView();
        }

        void RefreshLogLevel(object obj)
        {
            SetLogLevelComboBox();
        }

        /// <summary>
        /// Refreshes the text of the log level combobox to match the actual loggin level.
        /// </summary>
        private void SetLogLevelComboBox()
        {
            Dispatcher.BeginInvoke((Action) (() =>
            {
                switch (Log.LoggingLevel)
                {
                    case Log.Level.Trace:
                    {
                        CbLoggingLevel.SelectedIndex = 0;
                        break;
                    }
                    case Log.Level.Debug:
                    {
                        CbLoggingLevel.SelectedIndex = 1;
                        break;
                    }
                    case Log.Level.Info:
                    {
                        CbLoggingLevel.SelectedIndex = 2;
                        break;
                    }
                    case Log.Level.Warn:
                    {
                        CbLoggingLevel.SelectedIndex = 3;
                        break;
                    }
                    case Log.Level.Error:
                    {
                        CbLoggingLevel.SelectedIndex = 4;
                        break;
                    }
                    case Log.Level.Fatal:
                    {
                        CbLoggingLevel.SelectedIndex = 5;
                        break;
                    }
                }
            }));
        }

        #region Business Logic
        private void AddMessage(object item)
        {
            var logItem = item as Log.Entry;
            if (logItem == null) throw new ArgumentNullException("item");

            if (_exclusive)
            {
                if (_filterLevel != logItem.Level) return;
            }
            else
            {
                if (_filterLevel < logItem.Level) return;
            }

            lock(logListView)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    logListView.Items.Add(new LogListViewItem(logItem));
                    _currentLogListViewItems.Add(new LogListViewItem(logItem));

                    ScrollToEnd();

                    _count++;
                    _size += (logItem).ToString().Length;

                    msgCount.Text = _count.ToString(CultureInfo.CurrentCulture);
                    sumMsgCount.Text = Log.Messages.Count.ToString();                    
                    
                    var fs = Utils.GetFileSize(_size).Split(' ');
                    msgSize.Text = fs[0];
                    msgUnit.Text = fs[1];                    
                    
                }));            
            }            
        }

        private void ScrollToEnd()
        {
            if (_autoScroll) logListView.ScrollIntoView(logListView.Items[logListView.Items.Count - 1]);
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            Log.NewMessage -= AddMessage;
        }

        private bool IsContainsLevel(Log.Level l)
        {            
            //return ((_filterLevel & l) == l); old way
            return _filterLevel.HasFlag(l); //new way from .NET 4
        }

        #endregion

        #region Events

        /// <summary>
        /// Refreshes the <see cref="logListView"/> with <see cref="Log.Entry"/> entries referring to the selected filter level.
        /// </summary>
        private void RefreshListView()
        {
            logListView.Items.Clear();
            _count = 0;

            if (_exclusive)
            {
                foreach (
                    var entry in
                        Log.Messages.ToArray().Reverse().OrderBy(x => x.Time).Where(x => IsContainsLevel(x.Level)))
                {
                    logListView.Items.Add(new LogListViewItem(entry));
                    _count++;
                    _size += entry.ToString().Length;
                }
            }
            else
            {
                foreach (
                    var entry in
                        Log.Messages.ToArray().Reverse().OrderBy(x => x.Time).Where(x => (x.Level <= _filterLevel)))
                {
                    logListView.Items.Add(new LogListViewItem(entry));
                    _count++;
                    _size += entry.ToString().Length;                    
                }
            }


            var fs = Utils.GetFileSize(_size).Split(' ');
            msgSize.Text = fs[0];
            msgUnit.Text = fs[1];                

            msgCount.Text = _count.ToString(CultureInfo.InvariantCulture);
            sumMsgCount.Text = Log.Messages.Count.ToString(CultureInfo.InvariantCulture);

            _currentLogListViewItems.Clear();
            foreach (var item in logListView.Items)
            {
                _currentLogListViewItems.Add((LogListViewItem)item);
            }

            ScrollToEnd();
        }

        #region Levels        

        #region Trace

        private void SPTrace_MouseEnter(object sender, MouseEventArgs e)
        {
            lblTrace.FontWeight = FontWeights.Bold;
        }

        private void SPTrace_MouseLeave(object sender, MouseEventArgs e)
        {
            lblTrace.FontWeight = FontWeights.Normal;
        }

        private void SPTrace_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && _exclusive)
            {
                if (!IsContainsLevel(Log.Level.Trace))
                {
                    _filterLevel |= Log.Level.Trace;
                    SPTrace.Background = Brushes.White;
                }
                else
                {
                    _filterLevel &= ~Log.Level.Trace;
                    SPTrace.Background = Brushes.Transparent;
                }                
            }
            else
            {
                ColorizeTrace();
                _filterLevel = Log.Level.Trace;
            }
            
            RefreshListView();
        }

        private void ColorizeTrace()
        {
            if (!_exclusive)
            {
                SPTrace.Background = Brushes.White;
                SPDebug.Background = Brushes.WhiteSmoke;
                SPInfo.Background = Brushes.WhiteSmoke;
                SPWarn.Background = Brushes.WhiteSmoke;
                SPError.Background = Brushes.WhiteSmoke;
                SPFatal.Background = Brushes.WhiteSmoke;
            }
            else
            {
                SPTrace.Background = Brushes.White;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.Transparent;
                SPError.Background = Brushes.Transparent;
                SPFatal.Background = Brushes.Transparent;
            }
        }

        #endregion

        #region Debug

        private void SPDebug_MouseEnter(object sender, MouseEventArgs e)
        {
            lblDebug.FontWeight = FontWeights.Bold;
        }

        private void SPDebug_MouseLeave(object sender, MouseEventArgs e)
        {
            lblDebug.FontWeight = FontWeights.Normal;
        }

        private void SPDebug_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Log.Debug("Setting log level to Debug...");
            
            if (Control.ModifierKeys == Keys.Control && _exclusive)
            {
                if (!IsContainsLevel(Log.Level.Debug))
                {
                    _filterLevel |= Log.Level.Debug;
                    SPDebug.Background = Brushes.White;
                }
                else
                {
                    _filterLevel &= ~Log.Level.Debug;
                    SPDebug.Background = Brushes.Transparent;
                }
            }
            else
            {        
                ColorizeDebug();
                _filterLevel = Log.Level.Debug;
            }
            
            RefreshListView();
        }

        private void ColorizeDebug()
        {
            if (!_exclusive)
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.White;
                SPInfo.Background = Brushes.WhiteSmoke;
                SPWarn.Background = Brushes.WhiteSmoke;
                SPError.Background = Brushes.WhiteSmoke;
                SPFatal.Background = Brushes.WhiteSmoke;
            }
            else
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.White;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.Transparent;
                SPError.Background = Brushes.Transparent;
                SPFatal.Background = Brushes.Transparent;
            }
        }

        #endregion

        #region Info

        private void SPInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            lblInfo.FontWeight = FontWeights.Bold;
        }

        private void SPInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            lblInfo.FontWeight = FontWeights.Normal;
        }

        private void SPInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {           
            if (Control.ModifierKeys == Keys.Control && _exclusive)
            {
                if (!IsContainsLevel(Log.Level.Info))
                {
                    _filterLevel |= Log.Level.Info;
                    SPInfo.Background = Brushes.White;
                }
                else
                {
                    _filterLevel &= ~Log.Level.Info;
                    SPInfo.Background = Brushes.Transparent;
                }
            }
            else
            {
                ColorizeInfo();
                _filterLevel = Log.Level.Info;
            }

            RefreshListView();
        }

        private void ColorizeInfo()
        {
            if (!_exclusive)
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.White;
                SPWarn.Background = Brushes.WhiteSmoke;
                SPError.Background = Brushes.WhiteSmoke;
                SPFatal.Background = Brushes.WhiteSmoke;
            }
            else
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.White;
                SPWarn.Background = Brushes.Transparent;
                SPError.Background = Brushes.Transparent;
                SPFatal.Background = Brushes.Transparent;
            }
        }

        #endregion

        #region Warn

        private void SPWarn_MouseEnter(object sender, MouseEventArgs e)
        {
            lblWarn.FontWeight = FontWeights.Bold;
        }

        private void SPWarn_MouseLeave(object sender, MouseEventArgs e)
        {
            lblWarn.FontWeight = FontWeights.Normal;
        }

        private void SPWarn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && _exclusive)
            {
                if (!IsContainsLevel(Log.Level.Warn))
                {
                    _filterLevel |= Log.Level.Warn;
                    SPWarn.Background = Brushes.White;
                }
                else
                {
                    _filterLevel &= ~Log.Level.Warn;
                    SPWarn.Background = Brushes.Transparent;
                }
            }
            else
            {
                ColorizeWarn();
                _filterLevel = Log.Level.Warn;
            }

            RefreshListView();
        }

        private void ColorizeWarn()
        {
            if (!_exclusive)
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.White;
                SPError.Background = Brushes.WhiteSmoke;
                SPFatal.Background = Brushes.WhiteSmoke;
            }
            else
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.White;
                SPError.Background = Brushes.Transparent;
                SPFatal.Background = Brushes.Transparent;
            }
        }

        #endregion

        #region Error

        private void SPError_MouseEnter(object sender, MouseEventArgs e)
        {
            lblError.FontWeight = FontWeights.Bold;
        }

        private void SPError_MouseLeave(object sender, MouseEventArgs e)
        {
            lblError.FontWeight = FontWeights.Normal;
        }

        private void SPError_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && _exclusive)
            {
                if (!IsContainsLevel(Log.Level.Error))
                {
                    _filterLevel |= Log.Level.Error;
                    SPError.Background = Brushes.White;
                }
                else
                {
                    _filterLevel &= ~Log.Level.Error;
                    SPError.Background = Brushes.Transparent;
                }
            }
            else
            {
                ColorizeError();
                _filterLevel = Log.Level.Error;
            }

            RefreshListView();
        }

        private void ColorizeError()
        {
            if (!_exclusive)
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.Transparent;
                SPError.Background = Brushes.White;
                SPFatal.Background = Brushes.WhiteSmoke;
            }
            else
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.Transparent;
                SPError.Background = Brushes.White;
                SPFatal.Background = Brushes.Transparent;
            }
        }

        #endregion

        #region Fatal

        private void SPFatal_MouseEnter(object sender, MouseEventArgs e)
        {
            lblFatal.FontWeight = FontWeights.Bold;
        }

        private void SPFatal_MouseLeave(object sender, MouseEventArgs e)
        {
            lblFatal.FontWeight = FontWeights.Normal;
        }

        private void SPFatal_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && _exclusive)
            {
                if (!IsContainsLevel(Log.Level.Fatal))
                {
                    _filterLevel |= Log.Level.Fatal;
                    SPFatal.Background = Brushes.White;
                }
                else
                {
                    _filterLevel &= ~Log.Level.Fatal;
                    SPFatal.Background = Brushes.Transparent;
                }
            }
            else
            {
                ColorizeFatal();
                _filterLevel = Log.Level.Fatal;
            }

            RefreshListView();
        }

        private void ColorizeFatal()
        {
            if (!_exclusive)
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.Transparent;
                SPError.Background = Brushes.Transparent;
                SPFatal.Background = Brushes.White;
            }
            else
            {
                SPTrace.Background = Brushes.Transparent;
                SPDebug.Background = Brushes.Transparent;
                SPInfo.Background = Brushes.Transparent;
                SPWarn.Background = Brushes.Transparent;
                SPError.Background = Brushes.Transparent;
                SPFatal.Background = Brushes.White;
            }
        }

        #endregion

        #endregion

        private void SPExclusive_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_exclusive)
            {
                imgExclusive.Source = new BitmapImage(new Uri("Images/unchecked.png", UriKind.Relative));
                _exclusive = false;
            }
            else
            {
                imgExclusive.Source = new BitmapImage(new Uri("Images/checked.png", UriKind.Relative));
                _exclusive = true;
            }
            RefreshListView();

            switch (_filterLevel)
            {
                case Log.Level.Trace:
                {
                    ColorizeTrace();
                    break;
                }
                case Log.Level.Debug:
                {
                    ColorizeDebug();
                    break;
                }
                case Log.Level.Info:
                {
                    ColorizeInfo();
                    break;
                }
                case Log.Level.Warn:
                {
                    ColorizeWarn();
                    break;
                }
                case Log.Level.Error:
                {
                    ColorizeError();
                    break;
                }
                case Log.Level.Fatal:
                {
                    ColorizeFatal();
                    break;
                }
            }
        }

        #endregion

        private void btnClear_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            while (!Log.Messages.IsEmpty)
            {
                Log.Entry entry;
                Log.Messages.TryTake(out entry);
            }
            logListView.Items.Clear();
            msgCount.Text = "0";
            sumMsgCount.Text = "0";
        }

        private void btnClear_MouseEnter(object sender, MouseEventArgs e)
        {
            bClear.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBFBFBF"));
            bClear.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
        }

        private void btnClear_MouseLeave(object sender, MouseEventArgs e)
        {
            bClear.Background = Brushes.Transparent;
            bClear.BorderBrush = Brushes.Transparent;
        }

        private void btnScroll_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_autoScroll)
            {
                bAutoScroll.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBFBFBF"));
                bAutoScroll.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFAFAFA"));   
            }            
        }

        private void btnScroll_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_autoScroll)
            {
                bAutoScroll.Background = Brushes.Transparent;
                bAutoScroll.BorderBrush = Brushes.Transparent;    
            }            
        }

        private void bAutoScroll_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {            
            _autoScroll = !_autoScroll;
            ScrollToEnd();
        }

        private void TxtSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            logListView.Items.Clear();

            if (String.IsNullOrEmpty(txtSearch.Text))
            {               
                foreach (var item in _currentLogListViewItems)
                {
                    logListView.Items.Add(item);
                }
                ScrollToEnd();
                msgCount.Text = _currentLogListViewItems.Count.ToString();
            }
            else
            {
                var ic = new ItemsControl();

                foreach (var i in _currentLogListViewItems.Where(i => i.Message.ToLower().Contains(txtSearch.Text.ToLower()) || i.Location.ToLower().Contains(txtSearch.Text.ToLower())))
                {
                    ic.Items.Add(i);
                }

                _count = 0;
                foreach (var item in ic.Items)
                {
                    logListView.Items.Add(item);
                    _count++;
                }

                ScrollToEnd();
                msgCount.Text = _count.ToString();
                
                /*
                 foreach (var entry in (itemCollection as LogListViewItem)
                    .Reverse()
                    .Where(x => (x.Message.Contains(txtSearch.Text)) || (x.File.Contains(txtSearch.Text)) || (x.Method.Contains(txtSearch.Text)))
                    .OrderBy(x => x.Time))
                {
                    logListView.Items.Add(new LogListViewItem(entry));
                    _count++;
                }
                 */
            }            
        }

        private void BDummy_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            for (var i = 0; i < 1; i++)
            {
                Log.Trace("Ez egy trace");
                Log.Debug("Ez egy debug");
                Log.Info("Ez egy info");
                Log.Warn("Ez egy warn");
                Log.Error("Ez egy error");
                Log.Fatal("Ez egy fatal");
            }            
        }

        private void CbLoggingLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (System.Windows.Controls.ComboBox) sender;
            switch (comboBox.SelectedIndex)
            {
                case 0: Log.SetLevel(Log.Level.Trace);
                    break;
                case 1: Log.SetLevel(Log.Level.Debug);
                    break;
                case 2: Log.SetLevel(Log.Level.Info);
                    break;
                case 3: Log.SetLevel(Log.Level.Warn);
                    break;
                case 4: Log.SetLevel(Log.Level.Error);
                    break;
                case 5: Log.SetLevel(Log.Level.Fatal);
                    break;
            }
        }

        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(((LogListViewItem)logListView.SelectedItem).Item.ToString());
        }
    }
}
