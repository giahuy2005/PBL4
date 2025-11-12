using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace PBL4.Helper
{
    // Đảm bảo class là static, vì chúng ta đang làm việc với Attached Properties
    public static class PasswordBoxHelper
    {
        // Định nghĩa Attached Property
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty, OnPasswordChanged));

        // Getter và Setter cho Attached Property
        public static string GetBoundPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject obj, string value)
        {
            obj.SetValue(BoundPasswordProperty, value);
        }

        // Thực hiện khi giá trị Password thay đổi
        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // Ngừng sự kiện PasswordChanged để tránh vòng lặp vô hạn
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

                // Nếu mật khẩu từ Attached Property khác mật khẩu của PasswordBox, cập nhật
                if ((string)e.NewValue != passwordBox.Password)
                    passwordBox.Password = (string)e.NewValue;

                // Đăng ký lại sự kiện PasswordChanged
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        // Cập nhật giá trị của Attached Property khi PasswordBox thay đổi
        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetBoundPassword(passwordBox, passwordBox.Password); // Cập nhật giá trị
            }
        }
    }
}
