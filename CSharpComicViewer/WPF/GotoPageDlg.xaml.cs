using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace CSharpComicViewer.WPF
{
    /// <summary>
    /// Interaction logic for GotoPageDlg.xaml
    /// </summary>
    public partial class GotoPageDlg
    {
        // TODO error checking

        private readonly int _maxPage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxPage"></param>
        public GotoPageDlg(int maxPage)
        {
            _maxPage = maxPage;
            InitializeComponent();
            Page = 0;
            DataContext = this;

            lblMaxPage.Content = " of " + _maxPage;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PageText
        {
            get
            {
                return Page.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
Console.WriteLine("SetPage '{0}'", value);
                int newpage;
                if (!int.TryParse(value, out newpage))
                    return;
                if (newpage < 1 || newpage > _maxPage)
                    Page = 1;
                else
                    Page = newpage;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; 
            Close();
        }

        // Prevent space, non-numeric keys. Allow right/left arrow, tab, return, backspace.
        private void TxtPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
Console.Write(e.Key);
            // Note: allow Escape to close dialog
            if (e.Key != Key.Back && e.Key != Key.Right && e.Key != Key.Left && e.Key != Key.Tab && e.Key != Key.Return && e.Key != Key.Delete && e.Key != Key.Escape)
            {
                int val = (int)e.Key;
Console.Write(" val:" + val);
                e.Handled = val > 43 || val < 34;  // 43 == '9', 34 == '0'
            }
Console.WriteLine();
        }
    }
}
