using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PBL4.Services;

namespace PBL4.ViewModel
{
    public partial class ViewModelLoadingSignInUc : ObservableObject
    {
        // tạo web socket client kết nối đến server
        private CameraClient _client = new CameraClient();
        [ObservableProperty]
        private string loadingMessage = "Loading.....";
        public string Message = ""; 
        public event Action? LoadedSuccess;
        public event Action? LoadFailed;

        public ViewModelLoadingSignInUc()
        {
            /// chạy server 
            LoadServer();
            // tạo web socket client kết nối đến server


        }
        // chạy server
        public bool runPythonScript(string python_path, string script_path)
        {
            try
            {
                RunPython.Instance.RunPythonScript(python_path, script_path);
                return true;
            }
            catch (Exception)
            {
                LoadingMessage = "Error";
                return false;
            }

        }
        // tắt server nếu có lỗi 
        public void stopServer()
        {
            RunPython.Instance.StopPython();
        }
        /// load server 
        public async void LoadServer()
        {
            LoadingMessage = "Khởi động server...";
            string python_path = @"C:\Users\Admin\AppData\Local\Programs\Python\Python313\python.exe";
            string path = System.IO.Path.GetFullPath(@"..\..\..\Python");
            string script_path = System.IO.Path.Combine(path, "Run_server.py");
            if (runPythonScript(python_path, script_path)) {
                LoadingMessage = "Loading....";
                await Task.Delay(2000);
                LoadingMessage = "Kết nối WebSocket...";
                await ConnectToServer();
            }
            else
            {
                Message = "Khởi động server thất bại!!.";
                LoadFailed?.Invoke();
                return;
            }
        }
        // connect websocket to server
        private async Task ConnectToServer()
        {
            try
            {
                string uri = "ws://localhost:36000";
                await _client.ConnectAsync(uri);
                LoadingMessage = "Đã kết nối WebSocket thành công!";
                LoadedSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                LoadingMessage = "Kết nối WebSocket thất bại!";
                stopServer();
                await Task.Delay(1000);
                Message = ex.Message;
                LoadFailed?.Invoke();
            }
        }
        // trả về client
        public CameraClient GetClient()
        {
            return _client;
        }

    }
}

