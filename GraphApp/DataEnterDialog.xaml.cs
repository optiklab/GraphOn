using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GraphApp
{
    /// <summary>
    /// Класс описывает поведение окна ввода веса ребра.
    /// </summary>
    public partial class WeightEnterDialog : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public WeightEnterDialog(string caption)
        {
            _caption = caption;
            InitializeComponent();
            Data = -1;
        }

        /// <summary>
        /// Метод сохранения веса.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Okbutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Data = Convert.ToDouble(TextBox1.Text.ToString());
                this.Close();
            }
            catch (Exception)
            {
                Data = -1;
                MessageBox.Show("Не удалось корректно интерпретировать значение веса ребра!");
            }
        }

        /// <summary>
        /// Вес - который пользователь должен ввести.
        /// </summary>
        public double Data
        {
            get;
            set;
        }

        /// <summary>
        /// Метод, выполняемый во время загрузки окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Выставляем автоматический фокус на текстовое поле
            TextBox1.Focus();

            Caption.Content = _caption;
        }

        /// <summary>
        /// 
        /// </summary>
        private string _caption;
    }
}
