using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using PBL4.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.Services;
using PBL4.Model.Entities;
using PBL4.Model.BO;
namespace PBL4.ViewModel
{
    public partial class ViewModelMain : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject? currentView;
        private string username;
        public ViewModelMain()
        {
            CurrentView = new ViewModelSignInUC();
            if (CurrentView is ViewModelSignInUC signInVM)
            {
                signInVM.LoginSucceeded += OnLoginSucceeded;
            }
        }
        private void OnLoginSucceeded()
        {
            // hiện tại tắt màn hình đăng nhập và chuyển sang giao diện chính
            //var GiaoDienChinh = new GiaoDienChinh();
            //GiaoDienChinh.Show();
            //foreach (Window window in Application.Current.Windows)
            //{
            //    if (window is MainWindow)
            //    {
            //        window.Close();
            //        break;
            //    }

            //}
            // gán username cho thằng viewmodelmain 
            if ( CurrentView is ViewModelSignInUC signInVM)
            {
                username = signInVM.Username;
            }
            CurrentView = new ViewModelLoadingSignInUc();
            if(CurrentView is ViewModelLoadingSignInUc loadingVM)
            {
                loadingVM.LoadFailed += OnLoadFailed;
                loadingVM.LoadedSuccess += OnLoadSuccess;
            }
        }
        private async void OnLoadSuccess()
        {
            var GiaoDienChinh = new GiaoDienChinh();
            GiaoDienChinh.Show();
            CameraClient client; 
            if (CurrentView is ViewModelLoadingSignInUc loadingVM)
            {
                client = loadingVM.GetClient();
            }
            else
            {
                throw new InvalidOperationException("CurrentView is not ViewModelLoadingSignInUc");
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    window.Close();
                    break;
                }
            }
            UserBO userbo= new UserBO();
            User user = await userbo.GetUserByUserNameAsync(username);
            GiaoDienChinh.DataContext = new ViewModelGiaoDienChinh(client,user);
        }
        private void OnLoadFailed()
        {
            string message = "";
            if(CurrentView is ViewModelLoadingSignInUc loadingVM)
            {
                message = loadingVM.Message;
            }
            MessageBox.Show("Failed to load server.");
            CurrentView = new ViewModelSignInUC();
            if (CurrentView is ViewModelSignInUC signInVM)
            {
                signInVM.LoginSucceeded += OnLoginSucceeded;
            }
        }
    }
}
