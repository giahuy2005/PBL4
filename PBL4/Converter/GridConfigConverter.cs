using System;
using System.Globalization;
using System.Windows.Data;

namespace PBL4.Converters
{
    public class GridConfigConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                string type = parameter as string; // "Rows" hoặc "Columns"
                int rows = 1;
                int cols = 1;

                if (count <= 1)
                {
                    rows = 1; cols = 1;
                }
                // nếu lớn hơn 1 thì bắt đầu phân chia lưới 
                else if (count <=2) // chia 2
                {
                    rows = 1;
                    cols = count;
                }
                //  nếu lớn hơn 2 thì chia 4
                else if (count <= 4 && count >2 )
                {
                    rows = 2; cols = 2;
                }
                else if (count <= 6 && count>4)
                {
                    rows = 2; cols = 3;
                }
                else if (count <= 8)
                {
                    rows = 2; cols = 4;
                }
                else
                {
                    rows = 3; cols = 3;
                }

                // Trả về giá trị dựa trên tham số truyền vào từ XAML
                return type == "Rows" ? rows : cols;
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}