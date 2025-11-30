using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;


namespace PBL4.Model
{
    public partial class CameraModel : ObservableObject
    {
        [ObservableProperty]
        private string camId = "";

        [ObservableProperty]
        private string nameCamera = "";

        [ObservableProperty]
        private string url = "";

        [ObservableProperty]
        public string nameUser;
        [ObservableProperty]
        public string? password; 
        [ObservableProperty]
        private bool isStreaming;
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private BitmapImage? image;
    }
}


