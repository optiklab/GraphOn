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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(@"Программа позволяет выполнить следующие действия:");
            text.AppendLine(@"- Построение ориентированного графа через рисование или матрицу смежности,");
            text.AppendLine(@"- Построение матрицы смежности графа по его рисунку или рисунка по матрице,");
            text.AppendLine(@"- Выполнение различных операций над графами.");
            text.AppendLine(@" ");
            text.AppendLine(@"Полное описание проекта на сайте:");
            Description.Text = text.ToString();

            DescriptionContinue.Text = @"Все вопросы, пожелания и комментарии Вы также можете слать мне на электронную почту:";

            TechSupportMail.Text = @" anton.yarkov@gmail.com";
        }
    }
}
