using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data;
using System.Windows.Data;
using System.Globalization;

namespace GraphApp
{
    /// <summary>
    /// Класс - конвертер, выполняющий необходимую конвертацию значений в ячейках грида.
    /// </summary>
    internal sealed class RowIndexConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataRowView)
            {
                DataRowView row = value as DataRowView;
                string index = parameter as string;
                return row[index];
            }
            else
                return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
}
