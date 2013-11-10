using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;

namespace GraphApp
{
    /// <summary>
    /// Класс описывает логику взаимодействия с главным окном программы.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor

        /// <summary>
        /// Конструтор.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Определение горячих клавиш (для команд).
            gestureCollection = new InputGestureCollection();

            _graphicArtist = new Artist(drawingSurface);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void matrixGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (matrixGrid.Items.Count == 0)
                matrixGrid.Visibility = Visibility.Hidden;
            else
                matrixGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик команды "О программе".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            About dlg = new About();
            dlg.ShowDialog();
        }

        /// <summary>
        /// Обработчик команды рисования графа по матрице.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowGraphBasedOnMatrix_Click(object sender, RoutedEventArgs e)
        {
            _UncheckAllTools();

            try
            {
                if (matrixGrid.Items.Count > 0)
                {
                    _ClearGraphic();

                    Int32 count = _graph.Matrix.Rows.Count;
                    List<Point> archimedSpiral = _graphicArtist.GetArchimedSpiralPoints(count);

                    // Рисуем все вершины по Архимедовой спирали.
                    for (Int32 i = 0; i < count; i++)
                    {
                        _CreateVertex(archimedSpiral[i]);
                    }

                    // Генерируем связи, и сразу отрисовываем между вершинами
                    for (int i = 0; i < count; i++)
                        for (int j = 1; j < count + 1; j++)
                        {
                            double weight = Convert.ToDouble(_graph.Matrix.Rows[i][j]);

                            // Если связь есть и это не диагональ матрицы, то...
                            if (weight != -1 && i != (j - 1))
                            {
                                // Найти визуальные объекты для обоих вершин
                                Vertex v1 = _graph.Vertexes[i];
                                Vertex v2 = _graph.Vertexes[j - 1];
                                // Создать ребро.
                                _CreateRiber(v1, v2, weight);
                            }
                        }

                    // Скроллируем область рисования на наш граф.
                    if (_graph.Vertexes.Count > 0)
                    {
                        ScatchScrollViewer.ScrollToVerticalOffset(_graph.Vertexes[0].Point.Y);
                        ScatchScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                        ScatchScrollViewer.ScrollToHorizontalOffset(_graph.Vertexes[0].Point.X);
                    }

                    resultsView.Items.Add(GRAPH_GENERATED);
                }
                else
                    MessageBox.Show(VERTEXES_COUNT_ERROR);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Обработчик команды создания матрицы по графу.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMatrixBasedOnGraph_Click(object sender, RoutedEventArgs e)
        {
            _UncheckAllTools();

            try
            {
                GenerateMatrixBasedOnGraph(_graph);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Слишком большое значение в поле ввода числа вершин!");
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат в поле ввода числа вершин!");
            }
            catch (Exception)
            {
                MessageBox.Show("Неизвестная ошибка! Попробуйте перезапустить программу!");
            }
        }

        /// <summary>
        /// Обработчик команды "Найти диаметр"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindDiameter_Click(object sender, RoutedEventArgs e)
        {
            // Отключаем все инструменты.
            _UncheckAllTools();

            GenerateMatrixBasedOnGraph(_graph);

            if (_graph.Matrix.Rows.Count != 0)
                _SolveDiameter();
        }

        /// <summary>
        /// Обработчик команды "Найти вершины, недоступные из заданной"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindVertexes_Click(object sender, RoutedEventArgs e)
        {
            // Отключаем все инструменты.
            _UncheckAllTools();

            GenerateMatrixBasedOnGraph(_graph);

            try
            {
                if (_graph.Matrix.Rows.Count != 0)
                {
                    Int32 number = Convert.ToInt32(_GetNumericDataFromUser(VERTEX_INDEX_CAPTION));
                    _SolveTask(number);
                }
            }
            catch (OverflowException)
            {
                MessageBox.Show("Слишком большое значение в поле ввода числа вершин!");
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат в поле ввода числа вершин!");
            }
            catch (Exception)
            {
                MessageBox.Show("Неизвестная ошибка! Попробуйте перезапустить программу!");
            }
        }

        /// <summary>
        /// Обработчик команды "Найти медиану"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindMedian_Click(object sender, RoutedEventArgs e)
        {
            // Отключаем все инструменты.
            _UncheckAllTools();

            GenerateMatrixBasedOnGraph(_graph);

            // Если матрица имеет хоть одну вершину, вычисляем медиану
            if (_graph.Matrix.Rows.Count != 0)
                _SolveMedian();
        }

        /// <summary>
        /// Обработчик кнопки "Очистить граф".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearGraph_Click(object sender, RoutedEventArgs e)
        {
            _ClearGraphic();
        }

        /// <summary>
        /// Обработчик кнопки "Очистить матрицу".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearMatrix_Click(object sender, RoutedEventArgs e)
        {
            _ClearMatrix();
        }

        /// <summary>
        /// Обработчик кнопки "Новый проект".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewProjectCommand_Click(object sender, RoutedEventArgs e)
        {
            _ClearLayout();
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить проект".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog fd = new SaveFileDialog();
                fd.Filter = "Проекты |*.graphon";

                if (fd.ShowDialog() == true)
                {
                    FileInfo fi = new FileInfo(fd.FileName);

                    if (fi.Exists == true)
                        File.Delete(fd.FileName);

                    BinaryFormatter bformatter = new BinaryFormatter();

                    FileStream str = new FileStream(fd.FileName, FileMode.CreateNew);

                    if (_graph.Matrix.Rows.Count == 0)
                        ShowMatrixBasedOnGraph_Click(sender, e);

                    DataTable table = _graph.Matrix;

                    bformatter.Serialize(str, table);

                    str.Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно сохранить проект из-за неизвестной ошибки!");
            }
        }

        /// <summary>
        /// Обработчик кнопки "Открыть проект".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();
                fd.Filter = "Проекты |*.graphon";

                if (fd.ShowDialog() == true)
                {
                    FileInfo fi = new FileInfo(fd.FileName);

                    if (fi.Exists == true)
                    {
                        FileStream str = new FileStream(fd.FileName, FileMode.Open);

                        BinaryFormatter bformatter = new BinaryFormatter();

                        _ClearLayout();

                        _graph.Matrix = (DataTable)bformatter.Deserialize(str);

                        matrixGrid.ItemsSource = _graph.Matrix.DefaultView;
                        matrixGrid.DataContext = _graph.Matrix.DefaultView;

                        ShowGraphBasedOnMatrix_Click(sender, e);

                        str.Close();
                    }
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Невозможно открыть проект из-за неизвестной ошибки!");
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Генерировать матрицу".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            // Отключаем все инструменты.
            _UncheckAllTools();

            Int32 countOfVertexes = 0;

            try
            {
                countOfVertexes = Convert.ToInt32(_GetNumericDataFromUser(MATRIX_CAPTION));
            }
            catch (OverflowException)
            {
                MessageBox.Show("Слишком большое значение в поле ввода числа вершин!");
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат в поле ввода числа вершин!");
            }
            catch (Exception)
            {
                MessageBox.Show("Неизвестная ошибка! Попробуйте перезапустить программу!");
            }

            _matrixDimension = countOfVertexes;
            _GenerateNewTableGrid(countOfVertexes);
        }

        /// <summary>
        /// Обработчик нажатия левой кнопки мыши.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pointClicked = e.GetPosition(drawingSurface);

            // "Вершина"
            if (CreateButton.IsChecked == true)
                _CreateVertex(pointClicked);
            // "Ребро"
            else if (LineButton.IsChecked == true)
            {
                // Создать ребро.
                if (_isLineDrawing)
                {
                    _isLineDrawing = false;

                    // Определяем, что находится под местом, куда кликнул юзер.
                    RectangleGeometry geometry = new RectangleGeometry(new Rect(
                        new Point(pointClicked.X - POINT_RADIUS * 2, pointClicked.Y - POINT_RADIUS * 2),
                        new Point(pointClicked.X + POINT_RADIUS * 2, pointClicked.Y + POINT_RADIUS * 2)));
                    List<DrawingVisual> visualsInRegion = drawingSurface.GetVisuals(geometry);

                    drawingSurface.DeleteVisual(_tempLineVisual);

                    if (visualsInRegion.Count > 0)
                    {
                        DrawingVisual vis1 = visualsInRegion[0];

                        // Если место, куда юзер кликнул является вершиной этого графа.
                        if (_graph.IsVertexOfGraph(vis1))
                        {
                            // Находим обе вершины графа.
                            Vertex v1 = _graph.GetVertexOfGraph(_firstLinePoint);
                            Vertex v2 = _graph.GetVertexOfGraph(vis1);

                            // Данной связи не должно быть.
                            if (!_graph.IsRiberWithVertexPair(v1, v2))
                                _CreateRiber(v1, v2, _GetNumericDataFromUser(WEIGHT_CAPTION));
                            else
                                MessageBox.Show("Данные вершины уже связаны!");

                            _tempLineVisual = null;
                        }
                    }
                }
                // Нарисовать временную линию между курсором и первой точкой.
                else
                {
                    DrawingVisual vis = drawingSurface.GetVisual(pointClicked);

                    if (_graph.IsVertexOfGraph(vis))
                    {
                        _firstLinePoint = _graphicArtist.GetCenterPointOfVisual(vis);
                        _isLineDrawing = true;
                    }
                }
            }
            // "Удаление"
            else if (RemoveButton.IsChecked == true)
                _graphicArtist.RemoveElement(_graph, pointClicked, true);
        }

        /// <summary>
        /// Обработчик движения мышью во время рисования ребра.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isLineDrawing)
            {
                // Если рисуется ребро - удалить старое, перерисовать новое в точку курсора.
                drawingSurface.DeleteVisual(_tempLineVisual);

                Point currentPosition = e.GetPosition(drawingSurface);

                _tempLineVisual = _graphicArtist.DrawLine(_firstLinePoint, currentPosition, Brushes.Black);

                drawingSurface.AddVisual(_tempLineVisual);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitCommand_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("А Вы не забыли сохранить свой проект?", "Сохранить проект", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                SaveProject_Click(sender, e);

            this.Close();
        }

        #endregion

        #region CanExecute event handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewProjectCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing || (_graph.Vertexes.Count == 0 && _graph.Matrix.Rows.Count == 0))
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveProjectCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenProjectCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearGraphCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing || _graph.Vertexes.Count == 0)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowGraphBasedOnMatrix_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_graph.Matrix.Rows.Count == 0)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMatrixBasedOnGraph_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_graph.Vertexes.Count == 0 || _isLineDrawing)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindVertexes_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing || (_graph.Vertexes.Count == 0 && _graph.Matrix.Rows.Count == 0))
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateMatrixButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindDiameter_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing || (_graph.Vertexes.Count == 0 && _graph.Matrix.Rows.Count == 0))
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindMedian_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing || (_graph.Vertexes.Count == 0 && _graph.Matrix.Rows.Count == 0))
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_isLineDrawing)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Метод чистит панели рисования, грид вывода матрицы,
        /// отключает выделенные инструменты.
        /// </summary>
        private void _ClearLayout()
        {
            // Отключаем все инструменты.
            _UncheckAllTools();

            // Чистим панель.
            _ClearGraphic();
            _ClearMatrix();
        }

        /// <summary>
        /// Метод поиска медианы.
        /// </summary>
        private void _SolveMedian()
        {
            List<Int32> medians = Solver.FindMedian(_graph);

            if (medians.Any())
            {
                // Выводим сообщение о результате
                string result = "Вершины, являющиеся медианами графа: ";

                foreach (Int32 value in medians)
                    result += value + ",";
                result = result.Remove(result.Length - 1);

                resultsView.Items.Add(OPERATION_COMPLETED + result);

                // Выделяем медианы цветом
                foreach (Int32 value in medians)
                {
                    Vertex v = _graph.GetVertexOfGraph(value+1);
                    _graphicArtist.UpdateVertex(v, true);
                }
            }
            else
                MessageBox.Show("Не все вершины графа имеют связи!");
        }

        /// <summary>
        /// Метод поиска диаметра.
        /// </summary>
        private void _SolveDiameter()
        {
            List<Int32> path = new List<Int32>();
            double diam = Solver.FindDiameter(_graph, out path);

            // Проверяем результаты.
            if (path.Count > 0)
            {
                // Вывести сообщение с результатом.
                string text = "Диаметр графа: " + diam.ToString() + ". Путь диаметра лежит через вершины:";
                for (int j = path.Count - 1; j > -1; j--)
                    text += " -> " + path[j];

                resultsView.Items.Add(OPERATION_COMPLETED + text);

                // Нарисовать путь диаметра.
                _graphicArtist.DrawPath(_graph, path);
            }
            else
                MessageBox.Show("Не все вершины графа имеют связи, для продолжения вычислений создайте недостающие связи между вершинами!");
        }

        /// <summary>
        /// Метод поиска вершин , из которых не достижима заданная, и их удаления.
        /// </summary>
        /// <param name="number"></param>
        private void _SolveTask(Int32 number)
        {
            // Проверяем что такой номер вообще есть в коллекции
            if (_graph.GetVertexOfGraph(number + 1) != null)
            {
                // Находим список вершин, из которых не достижима заданная.
                List<Int32> list = Solver.FindVertexes(_graph, number);

                // Проверяем результаты - если хотя бы одна вершина найдена...
                if (list.Any())
                {
                    // Вывести сообщение с результатом.
                    string text = "Данная вершина недостижима из вершин: ";
                    foreach (Int32 value in list)
                        text += value + ",";
                    text = text.Remove(text.Length - 1);

                    resultsView.Items.Add(OPERATION_COMPLETED + text);

                    _graphicArtist.RemoveVertexes(_graph, list);

                    _graphicArtist.UpdateAllVertexesNumbers(_graph);

                    resultsView.Items.Add("Вершины были перенумерованы, чтобы состояние матрицы смежности оставалось валидным!");

                    GenerateMatrixBasedOnGraph(_graph);
                }
                else
                    MessageBox.Show("В графе нет вершин, из которых недоступна указанная вершина!");
            }
            else
                MessageBox.Show("Вершина с таким порядковым номером отсутствует!");
        }

        /// <summary>
        /// Отключает все инструменты.
        /// </summary>
        private void _UncheckAllTools()
        {
            CreateButton.IsChecked = false;
            LineButton.IsChecked = false;
            RemoveButton.IsChecked = false;
        }

        /// <summary>
        /// Метод очищает картинку графа.
        /// </summary>
        private void _ClearGraphic()
        {
            // Удаляем с картинки все вершины.
            foreach (Vertex v in _graph.Vertexes)
            {
                drawingSurface.DeleteVisual(v.Visual);
            }

            _graph.Vertexes.Clear();

            // Удаляем с картинки все ребра.
            foreach (Riber r in _graph.Weights)
            {
                drawingSurface.DeleteVisual(r.Visual);
            }

            _graph.Weights.Clear();

            drawingSurface.ClearGraphic();

            resultsView.Items.Add(GRAPH_CLEARED);
        }

        /// <summary>
        /// Метод очищает матрицу.
        /// </summary>
        private void _ClearMatrix()
        {
            _graph.Matrix.Clear();
            _graph.Vertexes.Clear();
            _graph.Weights.Clear();
            matrixGrid.ItemsSource = null;

            resultsView.Items.Add(MATRIX_CLEARED);
        }

        /// <summary>
        /// Метод генерирует новую матрицу смежности и заполняет соответствующий грид.
        /// </summary>
        /// <param name="countOfPoints"></param>
        private void _GenerateNewTableGrid(Int32 countOfPoints)
        {
            _ClearMatrix();
            _ClearGraphic();

            // Если количество вершин не превосходит пределы, то генерим.
            if (countOfPoints <= MAX_POINTS && countOfPoints >= MIN_POINTS)
            {
                // Генерим матрицу со значениями по умолчанию.
                _graph.BuildDefaultMatrix(countOfPoints);

                // Заполняем грид.
                matrixGrid.ItemsSource = _graph.Matrix.DefaultView;
                matrixGrid.DataContext = _graph.Matrix.DefaultView;

                resultsView.Items.Add(MATRIX_GENERATED);
            }
            else
                MessageBox.Show(VERTEXES_COUNT_ERROR);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        private void GenerateMatrixBasedOnGraph(Graph graph)
        {
            // Берем размерность будущей матрицы
            int countOfPoints = graph.Vertexes.Count;

            // Создаем пустую матрицу со значениями по умолчанию.
            if (countOfPoints <= MAX_POINTS && countOfPoints >= MIN_POINTS)
            {
                graph.BuildDefaultMatrix(countOfPoints);

                // Заполняем матрицу значениями.
                foreach (Riber r in _graph.Weights)
                {
                    Int32 v1 = r.Vertex1.Number;
                    Int32 v2 = r.Vertex2.Number;
                    graph.Matrix.Rows[v1 - 1][v2] = r.Weight;
                }

                // Аттачим матрицу к грид таблице на форме.
                matrixGrid.Columns.Clear();
                matrixGrid.ItemsSource = graph.Matrix.DefaultView;
                matrixGrid.DataContext = graph.Matrix.DefaultView;

                resultsView.Items.Add(MATRIX_UPDATED);
            }
            else
            {
                MessageBox.Show(VERTEXES_COUNT_ERROR);
                graph.Matrix.Clear();
            }
        }

        /// <summary>
        /// Метод создает вершину в месте, указанном инструментом "Вершина".
        /// </summary>
        /// <param name="point">Точка.</param>
        private void _CreateVertex(Point point)
        {
            // Ищем визуальный объект в этой точке.
            DrawingVisual vis = drawingSurface.GetVisual(point);

            // В точке ничего не должно быть.
            if (vis == null)
            {
                string number = _graph.Vertexes.Count.ToString();
                // Создаем визуальный объект.
                DrawingVisual newvis = _graphicArtist.DrawVertex(number, point, Brushes.LightBlue, Brushes.Black, true);
                // Добавляем его в область рисования.
                drawingSurface.AddVisual(newvis);
                // Добавляем его в коллекцию вершин графа.
                _graph.Vertexes.Add(new Vertex("Вершина " + number, _graph.Vertexes.Count + 1, point, newvis));
            }
        }

        /// <summary>
        /// Метод осуществляет рисование ребра инструментом "Ребро".
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="weight"></param>
        private DrawingVisual _CreateRiber(Vertex v1, Vertex v2, double weight)
        {
            DrawingVisual vis = _graphicArtist.DrawRiber(v1.Point, _graphicArtist.GetCenterPointOfVisual(v2.Visual), Brushes.Black);

            drawingSurface.AddVisual(vis);

            _graph.Weights.Add(new Riber("Ребро " + _graph.Weights.Count, v1, v2, weight, vis));

            return vis;
        }

        /// <summary>
        /// Метод показывает диалог ввода данных пользователя и возвращает введенное значение.
        /// </summary>
        /// <param name="dialogCaption">Подпись текстового поля в диалоге.</param>
        /// <returns>Введенное значение.</returns>
        private double _GetNumericDataFromUser(string dialogCaption)
        {
            double data = -1;

            if (!string.IsNullOrEmpty(dialogCaption))
            {
                WeightEnterDialog dlg = new WeightEnterDialog(dialogCaption);
                dlg.ShowDialog();
                data = dlg.Data;
            }

            return data;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Коллекция горячих клавиш.
        /// </summary>
        public static InputGestureCollection gestureCollection;

        /// <summary>
        /// Команда о программе.
        /// </summary>
        public static RoutedUICommand AboutCommand =
            new RoutedUICommand("О программе", "About", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда очистки графа.
        /// </summary>
        public static RoutedUICommand ClearGraphCommand = 
            new RoutedUICommand("Очистить граф", "ClearGraph", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда обновления графа.
        /// </summary>
        public static RoutedUICommand UpdateGraphCommand = 
            new RoutedUICommand("Обновить _граф", "UpdateGraph", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда обновления матрицы.
        /// </summary>
        public static RoutedUICommand UpdateMatrixCommand = 
            new RoutedUICommand("Обновить _матрицу", "UpdateMatrix", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда сохранения проекта.
        /// </summary>
        public static RoutedUICommand NewProjectCommand = 
            new RoutedUICommand("_Новый проект", "NewProject", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда сохранения проекта.
        /// </summary>
        public static RoutedUICommand SaveProjectCommand = 
            new RoutedUICommand("_Сохранить матрицу...", "SaveProject", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда открытия проекта.
        /// </summary>
        public static RoutedUICommand OpenProjectCommand =
            new RoutedUICommand("_Открыть матрицу...", "OpenProject", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда поиска вершин недоступных вершин.
        /// </summary>
        public static RoutedUICommand FindAndDeleteVertexesCommand = 
            new RoutedUICommand("Удалить _вершины, недостижимые из заданной", "FindAndDeleteVertexes", 
                typeof(Window), gestureCollection);

        /// <summary>
        /// 
        /// </summary>
        public static RoutedUICommand ExitCommand = 
            new RoutedUICommand("_Выход", "Exit", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда генерирования матриц.
        /// </summary>
        public static RoutedUICommand GereateMatrixCommand = 
            new RoutedUICommand("_Новая матрица", "GereateMatrix", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда поиска диаметра.
        /// </summary>
        public static RoutedUICommand FindDiameterCommand = 
            new RoutedUICommand("Найти _диаметр", "FindDiameter", typeof(Window), gestureCollection);

        /// <summary>
        /// Команда поиска медиан.
        /// </summary>
        public static RoutedUICommand FindMedianCommand = 
            new RoutedUICommand("Найти _медиану", "FindMedian", typeof(Window), gestureCollection);

        #endregion

        #region Private constants

        /// <summary>
        /// Максимальное число вершин.
        /// </summary>
        private const Int32 MAX_POINTS = 100;

        /// <summary>
        /// Минимальное число вершин.
        /// </summary>
        private const Int32 MIN_POINTS = 1;

        /// <summary>
        /// Наименование колонки
        /// </summary>
        private const string COLUMN_NAME_TEMPLATE = "Вершина {0}";

        /// <summary>
        /// Наименование строки
        /// </summary>
        private const string ROW_NAME_TEMPLATE = "Вершина {0}";

        /// <summary>
        /// Общее наименование таблицы
        /// </summary>
        private const string ROW_HEADER_NAME = "Вершины";

        /// <summary>
        /// Радиус вершин.
        /// </summary>
        private readonly Int32 POINT_RADIUS = 7;

        /// <summary>
        /// Подпись диалога ввода нового веса ребра.
        /// </summary>
        private const string WEIGHT_CAPTION = "Введите вес нового ребра:";

        /// <summary>
        /// Подпись диалога ввода количества вершин в матрице.
        /// </summary>
        private const string MATRIX_CAPTION = "Введите количество вершин:";

        /// <summary>
        /// 
        /// </summary>
        private const string VERTEX_INDEX_CAPTION = "Введите номер вершины:";

        /// <summary>
        /// 
        /// </summary>
        private const string MATRIX_GENERATED = "Построена новая матрица! ";

        /// <summary>
        /// 
        /// </summary>
        private const string GRAPH_GENERATED = "Построен новый граф! ";

        /// <summary>
        /// 
        /// </summary>
        private const string MATRIX_CLEARED = "Матрица удалена! ";

        /// <summary>
        /// 
        /// </summary>
        private const string GRAPH_CLEARED = "Граф удален! ";

        /// <summary>
        /// 
        /// </summary>
        private const string OPERATION_COMPLETED = "Операция завершена! ";

        /// <summary>
        /// 
        /// </summary>
        private const string MATRIX_UPDATED = "Матрица обновлена на основе рисунка графа!";

        /// <summary>
        /// 
        /// </summary>
        private const string VERTEXES_COUNT_ERROR = "Недопустимое количество вершин!";

        #endregion

        #region Private fields

        /// <summary>
        /// Определяет, не происходит ли рисование ребра в данный момент.
        /// </summary>
        private bool _isLineDrawing = false;

        /// <summary>
        /// Первая временная точка ребра.
        /// </summary>
        private Point _firstLinePoint = new Point(0, 0);

        /// <summary>
        /// Временный визуальный объект для рисования ребер.
        /// </summary>
        private DrawingVisual _tempLineVisual;
        
        /// <summary>
        /// Граф.
        /// </summary>
        private Graph _graph = new Graph();

        /// <summary>
        /// 
        /// </summary>
        private Artist _graphicArtist;

        /// <summary>
        /// 
        /// </summary>
        private Int32 _matrixDimension;

        #endregion
    }
}
