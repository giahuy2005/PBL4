using CommunityToolkit.Mvvm.ComponentModel;
using PBL4.Model;
using PBL4.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PBL4.ViewModel
{
    public partial class ViewModelCameraDescription : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject? currentView;

        public ViewModelCameraDescription(CameraModel Cam)
        {
            // khởi tạo các viewmodel con
            ViewModelCameraDescriptionUC viewmodel = new ViewModelCameraDescriptionUC(Cam);
            viewmodel.IsClose += OnIsClose; 
            CurrentView = viewmodel;

        }
        private void OnIsClose()
        {
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
