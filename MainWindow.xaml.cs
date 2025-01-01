using System.Media;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Space_Remover
{
    public partial class MainWindow : Window
    {
        [GeneratedRegex(@"([.!?,;:\-&])\s{2,}")]
        private static partial Regex SpacesRegex();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // For gathering the version number and clipboard data.
        }

        public static string PackageVersion
        {
            get
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                if (assembly != null)
                {
                    var version = assembly.GetName().Version;
                    if (version != null)
                    {
                        return version.ToString();
                    }
                }
                return "Error getting version number.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        // Changing the window border size when maximized so that it doesn't go past the screen borders.
        public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness(5);
            }
            else
            {
                BorderThickness = new Thickness(0);
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private async void CheckforUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Version currentVersion = new(PackageVersion);

            // GitHub API URL for the latest release.
            string latestReleaseUrl = "https://api.github.com/repos/Jestzer/Space_Remover/releases/latest";

            // Use HttpClient to fetch the latest release data.
            using HttpClient client = new();

            // GitHub API requires a user-agent. I'm adding the extra headers to reduce HTTP error 403s.
            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Space_Remover", PackageVersion));
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                try
                {
                    // Make the latest release a JSON string.
                    string jsonString = await client.GetStringAsync(latestReleaseUrl);

                    // Parse the JSON to get the tag_name (version number).
                    using JsonDocument doc = JsonDocument.Parse(jsonString);
                    JsonElement root = doc.RootElement;
                    string latestVersionString = root.GetProperty("tag_name").GetString()!;

                    // Remove 'v' prefix if present in the tag name.
                    latestVersionString = latestVersionString.TrimStart('v');

                    // Parse the version string.
                    Version latestVersion = new(latestVersionString);

                    // Compare the current version with the latest version.
                    if (currentVersion.CompareTo(latestVersion) < 0)
                    {
                        // A newer version is available!
                        ErrorWindow errorWindow = new();
                        errorWindow.Owner = this;
                        errorWindow.ErrorTextBlock.Text = "";
                        errorWindow.URLTextBlock.IsEnabled = true;
                        errorWindow.URLTextBlock.Visibility = Visibility.Visible;
                        errorWindow.DownloadButton.Visibility = Visibility.Visible;
                        errorWindow.Title = "Check for updates";
                        errorWindow.ShowDialog();
                        errorWindow.URLTextBlock.IsEnabled = false;
                    }
                    else
                    {
                        ShowUpdateWindow("You are using the latest release available.");
                    }
                }
                catch (JsonException ex)
                {
                    ShowUpdateWindow("The Json code in this program didn't work. Here's the automatic error message it made: \"" + ex.Message + "\"");
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    ShowUpdateWindow("HTTP error 403: GitHub is saying you're sending them too many requests, so... slow down, I guess? " +
                        "Here's the automatic error message: \"" + ex.Message + "\"");
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ShowUpdateWindow("HTTP error 404: when checking for updates, we were told the page to check for updates doesn't exist. Check GitHub manually: https://github.com/Jestzer/Space_Remover");
                }
                catch (HttpRequestException ex)
                {
                    ShowUpdateWindow("HTTP error. Here's the automatic error message: \"" + ex.Message + "\"");
                }
            }
            catch (Exception ex)
            {
                ShowUpdateWindow("Oh dear, it looks this program had a hard time making the needed connection to GitHub. Make sure you're connected to the internet " +
                    "and your lousy firewall/VPN isn't blocking the connection. Here's the automated error message: \"" + ex.Message + "\"");
            }
        }

        private void ShowErrorWindow(string errorMessage)
        {
            ErrorWindow errorWindow = new();
            errorWindow.ErrorTextBlock.Text = errorMessage;
            errorWindow.Owner = this;
            errorWindow.Title = "Error";
            errorWindow.ShowDialog();
        }

        private void ShowUpdateWindow(string errorMessage)
        {
            ErrorWindow errorWindow = new();
            errorWindow.ErrorTextBlock.Text = errorMessage;
            errorWindow.Owner = this;
            errorWindow.Title = "Check for Updates";
            SystemSounds.Exclamation.Play();
            errorWindow.ShowDialog();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (FixedTextBox != null)
            {;
                int attempts = 0;
                bool success = false;
                string errorMessage = string.Empty;

                while (!success && attempts < 5)
                {
                    try
                    {
                        Clipboard.SetText(FixedTextBox.Text);
                        success = true;
                    }
                    catch (System.Runtime.InteropServices.ExternalException ex)
                    {
                        attempts++;
                        errorMessage = ex.Message;
                        Thread.Sleep(100); // In milliseconds.
                    }
                }

                if (!success)
                {
                    if (errorMessage == "OpenClipboard Failed (0x800401D0 (CLIPBRD_E_CANT_OPEN))")
                    {
                        errorMessage = "Copying to the clipboard failed. Try copying the text like you normally would.";
                    }
                    ShowErrorWindow(errorMessage);
                }
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                OriginalTextBox.Text = Clipboard.GetText();
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            FixedTextBox.Text = SpacesRegex().Replace(OriginalTextBox.Text, "$1 ");
        }
    }
}