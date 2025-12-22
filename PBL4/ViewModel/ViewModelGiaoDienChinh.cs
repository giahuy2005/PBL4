using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.Model;
using PBL4.Model.BO;
using PBL4.Model.Entities;
using PBL4.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace PBL4.ViewModel
{
    partial class ViewModelGiaoDienChinh : ObservableObject
    {
        // text của thanh tìm kiếm 
        [ObservableProperty]
        private string searchText;
        // tắt hay bật chiều dài explore
        [ObservableProperty]
        private bool isExploreOpen = true;
        [ObservableProperty]
        public string exploreWidth = "0.23*";

        public ObservableCollection<CameraModel> Cameras { get; } = new();
        public ObservableCollection<CameraModel> ActiveCameras { get; } = new();
        public ObservableCollection<CameraModel> UserCameraList { get; } = new();      
        [ObservableProperty]
        private CameraModel selectedCamera;  // báo để biết camera nào được chọn
        private CameraClient _client;
        private User? user;
        private ICollectionView _cameraView;
        [ObservableProperty]
        private String name;
        public ViewModelGiaoDienChinh(CameraClient client,User user)
        {
            _client = client;
            _client.FrameReceived += OnFrameReceived;
            this.user = user;
            Name = user.UserName ?? "User";
            var cameraView=CollectionViewSource.GetDefaultView(UserCameraList);
            cameraView.Filter = FilterCameras;
            LoadDataFromServer();

        }
        // sự kiện khi ấn dô descrip của từng camera
        [RelayCommand]
        public void OpenCameraDetail(CameraModel? cam)
        {
            SelectedCamera = cam;
            var window = new PBL4.View.CameraDescription();
            window.DataContext = new ViewModelCameraDescription(SelectedCamera,_client);
            window.ShowDialog();
            LoadDataFromServer();
        }
        // sự kiện xóa camera 
        [RelayCommand]
        public async Task DeleteCameraAsync(CameraModel? cam)
        {
            if (cam == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa camera {cam.NameCamera}?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                if (cam.IsStreaming)
                {
                    await ToggleCameraStream(cam);
                }
                CamerasBO _camerasBO = new CamerasBO();

                await _camerasBO.DeleteCamera(cam.CamId);

                UserCameraList.Remove(cam);

                MessageBox.Show("Xóa camera thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa camera: " + ex.Message);
            }
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
                // 1. Lấy dữ liệu mới nhất từ Database
                var listEntity = await _camerasBO.LoadCamerasByUserIdAsync(user.IdUser);

                if (listEntity == null) return;

                foreach (var entity in listEntity)
                {
                    // Tìm xem camera này đã có trong UserCameraList chưa (so sánh ID)
                    var existingCam = UserCameraList.FirstOrDefault(c => c.CamId == entity.IdCamera);

                    if (existingCam != null)
                    {
                        // [QUAN TRỌNG]: Nếu đã có, chỉ update thông tin, KHÔNG update trạng thái stream
                        // Chỉ cập nhật nếu có thay đổi để đỡ tốn tài nguyên binding
                        if (existingCam.NameCamera != entity.NameCamera) existingCam.NameCamera = entity.NameCamera;
                        if (existingCam.Url != entity.URL) existingCam.Url = entity.URL;
                        if (existingCam.NameUser != entity.NameUser) existingCam.NameUser = entity.NameUser;
                        if (existingCam.Password != entity.Password) existingCam.Password = entity.Password;

                        // Lưu ý: KHÔNG đụng đến existingCam.IsStreaming hay existingCam.IsLoading
                        // Vì camera này có thể đang chạy

                        // [Nâng cao]: Nếu camera ĐANG STREAM mà URL bị đổi, bạn có thể cân nhắc 
                        // tự động stop camera đó hoặc giữ nguyên luồng cũ tùy logic.
                        // Ở đây tôi giữ nguyên luồng cũ để tránh gián đoạn.
                    }
                    else
                    {
                        // Nếu chưa có thì tạo mới
                        var newModel = new CameraModel
                        {
                            CamId = entity.IdCamera,
                            NameCamera = entity.NameCamera,
                            Url = entity.URL,
                            NameUser = entity.NameUser,
                            Password = entity.Password,
                            IsLoading = false,
                            IsStreaming = false,
                            Image = null
                        };
                        UserCameraList.Add(newModel);
                    }
                }

                var dbIds = listEntity.Select(e => e.IdCamera).ToList();

                var camerasToDelete = UserCameraList.Where(c => !dbIds.Contains(c.CamId)).ToList();

                foreach (var camToDelete in camerasToDelete)
                {
                    if (camToDelete.IsStreaming)
                    {
                        await ToggleCameraStream(camToDelete); 
                    }

                    UserCameraList.Remove(camToDelete);

                    var active = ActiveCameras.FirstOrDefault(c => c.CamId == camToDelete.CamId);
                    if (active != null) ActiveCameras.Remove(active);
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
                var activeCam = ActiveCameras.FirstOrDefault(c => c.CamId == camId);

                if (activeCam != null)
                {
                  
                    activeCam.Image = image;
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
        // xử lý giao diện khi người dùng ấn close 
        [RelayCommand]
        public void CloseExplore()
        {
            double from = IsExploreOpen ? 0.23 : 0.03;
            double to = IsExploreOpen ? 0.03 : 0.23;
            double duration = 300; 
            double interval = 15;  // ms
            int steps = (int)(duration / interval);
            int currentStep = 0;

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(interval);
            timer.Tick += (s, e) =>
            {
                currentStep++;
                double progress = (double)currentStep / steps;
                double value = from + (to - from) * progress;
                ExploreWidth = value.ToString("0.00") + "*";

                if (currentStep >= steps)
                {
                    timer.Stop();
                    ExploreWidth = to.ToString("0.00") + "*"; // chắc chắn giá trị cuối
                }
            };
            timer.Start();

            IsExploreOpen = !IsExploreOpen;
        }
        [RelayCommand]
        public void PerformSearch()
        {
            // Lệnh này chỉ chạy khi bạn ấn nút
            CollectionViewSource.GetDefaultView(UserCameraList).Refresh();
        }
        private bool FilterCameras(object item)
        {
            if (item is CameraModel cam)
            {
                // Nếu ô tìm kiếm rỗng thì hiện tất cả (return true)
                if (string.IsNullOrEmpty(SearchText)) return true;

                // Kiểm tra tên camera có chứa từ khóa không (không phân biệt hoa thường)
                return cam.NameCamera != null &&
                       cam.NameCamera.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }


    }
}
