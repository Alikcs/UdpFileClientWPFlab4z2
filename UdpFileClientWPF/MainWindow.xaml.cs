using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UdpFileClientWPF
{
    public partial class MainWindow : Window
    {
        private const string ServerIp = "127.0.0.1";
        private const int ServerPort = 5000;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            var filePath = FilePathTextBox.Text;

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл не найден");
                return;
            }

            await SendFileAsync(filePath);
        }

        private async Task SendFileAsync(string filePath)
        {
            try
            {
                var udpClient = new UdpClient();
                var serverEndpoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);

                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    byte[] buffer = new byte[1024];

                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await udpClient.SendAsync(buffer, bytesRead, serverEndpoint);
                        StatusTextBlock.Text = $"Передано {fileStream.Position} байт";
                    }
                }

                // Отправка сигнала завершения
                byte[] endSignal = Encoding.UTF8.GetBytes("END");
                await udpClient.SendAsync(endSignal, endSignal.Length, serverEndpoint);
                StatusTextBlock.Text = "Передача завершена";

                // После завершения передачи показываем диалог "Открыть файл?"
                var result = MessageBox.Show("Открыть файл?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Открываем файл
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }



    }
}
