using System;
using System.Security.Cryptography.Pkcs;
using System.Windows;

namespace AsyncThrottleDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HardwareController _controller;

        public MainWindow()
        {
            InitializeComponent();
            _controller = new HardwareController(
                l => Dispatcher.BeginInvoke(
                    () => Log(l)));
        }

        private void Log(string l)
        {
            logTextBox.Text += l + Environment.NewLine;
        }

        private async void moveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var param = new MoveCommandParam { Steps = 10, Id = Count++ };
                await _controller.RunCommand("move", param);
                Log($"Command ({param.Id}) is now done");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            finally
            {
            }
        }

        public int Count { get; set; }

        private async void stopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var param = new CommandParam { Id = Count++ };
                await _controller.RunCommand("stop", param);
                Log($"Command ({param.Id}) is now done");
            }
            finally
            {
            }
        }
    }
}
