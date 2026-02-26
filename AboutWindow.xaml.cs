using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NinjaForge
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        string _version="v420.0.69";//FIXME <-- this version should come from the build system when a new release build is created.
        public AboutWindow()
        {
            InitializeComponent();
            WindowStyle = WindowStyle.ToolWindow;
            Title = "About";

            Version? version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string versionText = version?.ToString(3) ?? "0";

            VersionText.Text = $"NinjaForge v{versionText}";

            ImageTarget.Source = new BitmapImage(new Uri("/Launcher2.ico", UriKind.Relative));
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot open link:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
        }
    }
}
