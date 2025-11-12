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
        private CameraClient _client = new CameraClient();
        [ObservableProperty]
        private string stateServer= "off";
        public ViewModelGiaoDienChinh()
        {
            
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

        public void runPythonScript(string python_path, string script_path)
        {

            RunPython.Instance.RunPythonScript(python_path, script_path);
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
        public async Task StartServer()
        {
            // massage box
            System.Windows.MessageBox.Show("Starting camera...");
            //C:\Users\Admin\AppData\Local\Programs\Python\Python313\python.exe
            string python_path = @"C:\Users\Admin\AppData\Local\Programs\Python\Python313\python.exe";
            //D:\kỳ 5\PBL4\PBL4\PBL4\Python\get_camera.py      
            string script_path = @"D:\ky5\PBL4\PBL4_Truy_xuat_va_quan_ly_du_lieu_hinh_anh_Camera_thong_qua_mang\PBL4\PBL4\Python\Run_server.py";
            runPythonScript(python_path, script_path);
            int retryCount = 0;
            while (true) {
                if (IsPortOpen("127.0.0.1", 36000))
                {
                    MessageBox.Show("đã thấy server");
                    break;
                }
                else
                {
                    retryCount++; 
                    if (retryCount > 10)
                    {
                        MessageBox.Show("Không thể kết nối đến server sau nhiều lần thử.", "Lỗi Kết Nối", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show("chưa thấy server, thử lại sau 1s");
                    await Task.Delay(1000); 
                }
               
            }
            await ConnectToServer();
        }
        [RelayCommand]
        public async Task StartVideo()
        {
            string cam_id = "cam1"; 
            string url = "http://admin:admin@192.168.100.217:39000/video";
            if (_client.check_ws()) 
            {
                MessageBox.Show("Bắt đầu video");
                 await _client.StartCamera(cam_id, url);
            }
        }
        private async Task ConnectToServer()
        {
            string uri = "ws://localhost:36000";
            await _client.ConnectAsync(uri);

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
