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
        public ObservableCollection<CameraModel> ActiveCameras { get; } = new();
        public ObservableCollection<CameraModel> UserCameraList { get; } = new();      
        [ObservableProperty]
        private CameraModel selectedCamera;  // báo để biết camera nào được chọn
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
        public void OpenCameraDetail(CameraModel? cam)
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
                CamerasBO _camerasBO = new CamerasBO();
                // 1. Lấy dữ liệu thô (Entity)
                var listEntity = await _camerasBO.LoadCamerasByUserIdAsync(user.IdUser);

                UserCameraList.Clear();
                if (listEntity != null)
                {
                    foreach (var entity in listEntity)
                    {
                        var model = new CameraModel
                        {
                            CamId = entity.IdCamera,
                            NameCamera = entity.NameCamera,
                            Url = entity.URL,
                            NameUser = entity.NameUser,
                            Password = entity.Password,
                            IsLoading = false,
                            IsStreaming = false
                        };
                        UserCameraList.Add(model);
                    }
                }
            }
            catch (Exception ex)
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
        [RelayCommand]
        public async Task ToggleCameraStream(CameraModel model)
        {
            if (model == null) return;

            if (model.IsStreaming)
            {
                // --- TRƯỜNG HỢP TẮT CAMERA ---
                model.IsStreaming = false;
                model.Image = null;

                // Gửi lệnh stop cho server (nếu cần)
                await _client.StopCamera(model.CamId);

                // Xóa khỏi danh sách hiển thị bên phải
                if (ActiveCameras.Contains(model))
                {
                    ActiveCameras.Remove(model);
                }
            }
            else
            {
                // bật camera 
                model.IsLoading = true; 

                // Giả lập check kết nối (hoặc gọi hàm check thật)
                bool isOk = await Task.Run(async () =>
                {
                    return await _client.CheckCamera(model.CamId, model.Url,model.NameUser,model.Password); 
                });

                model.IsLoading = false;

                if (isOk)
                {
                    model.IsStreaming = true; 

                    // Gửi lệnh start stream thật
                     await _client.StartCamera(model.CamId, model.Url,model.NameUser,model.Password);

                    if (!ActiveCameras.Contains(model))
                    {
                        ActiveCameras.Add(model);
                    }
                }
                else
                {
                    MessageBox.Show($"Không thể kết nối camera {model.NameCamera}");
                }
            }
        }


    }
}
