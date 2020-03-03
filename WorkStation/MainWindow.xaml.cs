using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace WorkStation
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ILogger m_Log = LogManager.GetCurrentClassLogger();
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //Global.ShowMessageDialog();
            ShowMessageDialog();
        }

        private async void ShowMessageDialog()
        {
            // This demo runs on .Net 4.0, but we're using the Microsoft.Bcl.Async package so we have async/await support
            // The package is only used by the demo and not a dependency of the library!
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Hi",
                NegativeButtonText = "Go away!",
                FirstAuxiliaryButtonText = "Cancel",
                ColorScheme = MetroDialogOptions.ColorScheme,
            };

            MessageDialogResult result = await this.ShowMessageAsync("Hello!", "Welcome to the world of metro!",
                MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, mySettings);
        }
    }
}
