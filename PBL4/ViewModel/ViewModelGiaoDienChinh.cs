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
using System.Printing;
using PBL4.Model.BO;
using PBL4.Model.Entities;
namespace PBL4.ViewModel
{
    partial class ViewModelGiaoDienChinh : ObservableObject
    {
        public ObservableCollection<CameraModel> Cameras { get; } = new();
        public ObservableCollection<Cameras> UserCameraList { get; } = new();
        // báo để biết camera nào được chọn
        [ObservableProperty]
        private Cameras selectedCamera;
        private CameraClient _client;
        private User? user;

        public ViewModelGiaoDienChinh(CameraClient client,User user)
        {
            _client = client;
            _client.FrameReceived += OnFrameReceived;
            this.user = user;
            LoadDataFromServer();

        }
        // sự kiện khi ấn dô descrip của từng camera
        [RelayCommand]
        public void OpenCameraDetail(Cameras cam)
        {
            SelectedCamera = cam;
            var window = new PBL4.View.CameraDescription();
            window.DataContext = new ViewModelCameraDescription(SelectedCamera);
            window.ShowDialog();
        }
        // sự kiện mở add camera 
        [RelayCommand]
        public void OpenAddCameraWindow()
        {
            var window = new PBL4.View.AddCamera();
            window.DataContext = new ViewModelAddCamera(_client,user);
            window.ShowDialog();
            LoadDataFromServer();

        }
        private async void LoadDataFromServer()
        {
            if (user == null || string.IsNullOrEmpty(user.IdUser)) return;

            try
            {
                // --- ĐÂY LÀ CHỖ BẠN CẦN ---

                // 1. Gọi BO để lấy danh sách từ DB
                CamerasBO _camerasBO = new CamerasBO();
                var listCamFromDB = await _camerasBO.LoadCamerasByUserIdAsync(user.IdUser);

                // 2. Gán thẳng vào cái ListCamera của User (đúng ý bạn muốn)
                user.ListCamera = listCamFromDB;

                // 3. Đổ dữ liệu từ user.ListCamera sang ObservableCollection để hiện lên giao diện
                UserCameraList.Clear();
                if (user.ListCamera != null)
                {
                    foreach (var cam in user.ListCamera)
                    {
                        UserCameraList.Add(cam);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách camera: " + ex.Message);
            }
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
