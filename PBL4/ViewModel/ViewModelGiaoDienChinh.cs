using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.Model;
using PBL4.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;
namespace PBL4.ViewModel
{
    partial class ViewModelGiaoDienChinh : ObservableObject
    {
        public ObservableCollection<CameraModel> Cameras { get; } = new();
        private CameraClient _client;
        [ObservableProperty]
        private string stateServer= "off";
        public ViewModelGiaoDienChinh(CameraClient client)
        {
            _client = client;
            _client.FrameReceived += OnFrameReceived;
            _client.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_client.ClientState))
                    StateServer = _client.ClientState;
            };
        }
        public void UpdateFrame(BitmapImage new_image)
        {
            if (new_image == null) return;
        }
        public static bool IsPortOpen(string host, int port, int timeout = 2000)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var task = client.ConnectAsync(host, port);
                    return task.Wait(timeout) && client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task StopServer()
        {
            try
            {

                await _client.StopServer();

                await Task.Delay(1000);

                RunPython.Instance.StopPython();

                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi dừng server: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


       
        [RelayCommand]
        public async Task StartVideo()
        {
            string cam_id = "cam1"; 
            string url = "http://admin:admin@192.168.110.224:39000/video";
            if (_client.check_ws()) 
            {
                MessageBox.Show("Bắt đầu video");
                 await _client.StartCamera(cam_id, url);
            }
        }

        private void OnFrameReceived(string camId, BitmapImage? image)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existing = Cameras.FirstOrDefault(c => c.CamId == camId);
                if (existing != null)
                {
                    existing.Image = image;
                }
                else
                {
                    Cameras.Add(new CameraModel { CamId = camId, Image = image });
                }
            });
        }


    }
}
