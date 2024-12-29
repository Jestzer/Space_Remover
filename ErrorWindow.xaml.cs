using System.Windows;
using System.Windows.Input;


namespace Space_Remover
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        // Implement window dragging with the mouse.
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseWithEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Close();
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var url = "https://github.com/Jestzer/options.file.checker/releases/latest";

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Necessary to use the system's default web browser.
                };

                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
               URLTextBlock.Text = $"Error opening URL: {ex.Message}";
            }
        }
    }
}
