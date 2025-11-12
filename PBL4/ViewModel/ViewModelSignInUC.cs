using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PBL4.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace PBL4.ViewModel
{
    public partial class ViewModelSignInUC : ObservableObject
    {
        [ObservableProperty]
        private string? username;
        [ObservableProperty]
        private string? password;
        [RelayCommand]
        void Login()
        {
            if (Username == "admin" && Password == "admin" || true)
            {
                var GiaoDienChinh = new GiaoDienChinh();
                GiaoDienChinh.Show();
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }

            }
            else
            {
                System.Windows.MessageBox.Show("Invalid username or password.");
            }
        }
        public ViewModelSignInUC()
        {

        }

    }
}
