using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace GraphApp
{
    /// <summary>
    /// Класс определяет все свойста и необходимые внутренние метода графа.
    /// </summary>
    internal sealed class Graph
    {
        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        public Graph()
        {
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Метод построения матрицы по умолчанию.
        /// </summary>
        /// <param name="countOfVertexes"></param>
        public void BuildDefaultMatrix(Int32 countOfVertexes)
        {
            _matrix = new DataTable();

            // Добавить первую колонку - с именами вершин.
            _matrix.Columns.Add(new DataColumn(ROW_HEADER_NAME, typeof(String)));

            // Добавить все колонки.
            _BuildColumns(countOfVertexes);

            // Добавить все строки и заполнить их значениями.
            _BuildRows(countOfVertexes);

            // Установить ограницения на операции в DataTable.
            _matrix.DefaultView.AllowNew = false;
            _matrix.DefaultView.AllowDelete = false;
            _matrix.DefaultView.AllowEdit = true;
        }

        /// <summary>
        /// Метод определяет является ли визуальный объект - одной из вершин графа.
        /// </summary>
        /// <param name="visual">визуальный объект</param>
        /// <returns>Да - является, Нет - не является</returns>
        public bool IsVertexOfGraph(DrawingVisual visual)
        {
            // Метод LINQ для поиска данного объекта в коллекции
            var result = from v in _vertexes
                         where (v.Visual == visual)
                         select v;

            return result.Any();
        }

        /// <summary>
        /// Метод определяет является ли точка одной из вершин графа.
        /// </summary>
        /// <param name="point">точка</param>
        /// <returns>Да - является, Нет - не является</returns>
        public bool IsVertexOfGraph(Point point)
        {
            // Метод LINQ для поиска данного объекта в коллекции
            var result = from v in _vertexes
                         where (v.Point == point)
                         select v;

            return result.Any();
        }

        /// <summary>
        /// Метод определяет есть ли в графе ребро, соединяющее именно эти две вершины
        /// (и именно в таком порядке, т.к. это ОРИЕНТИРОВАННЫЙ граф)
        /// </summary>
        /// <param name="v1">вершина 1</param>
        /// <param name="v2">вершина 2</param>
        /// <returns>Да - найдена, иначе - нет.</returns>
        public bool IsRiberWithVertexPair(Vertex v1, Vertex v2)
        {
            // Метод LINQ для поиска объектов в коллекции
            var result = from r in _weights
                         where (r.Vertex1 == v1 && r.Vertex2 == v2)
                         select r;

            return result.Any();
        }

        /// <summary>
        /// Метод находит вершину по её визуальному объекту в списке вершин графа.
        /// </summary>
        /// <param name="visual">визуальный объект</param>
        /// <returns>Первая встреченная вершина - если есть хотя бы одна, либо null.</returns>
        public Vertex GetVertexOfGraph(DrawingVisual visual)
        {
            // Метод LINQ для поиска данного объекта в коллекции
            var result = from v in _vertexes
                         where (v.Visual == visual)
                         select v;

            // Если хотя бы одна вершина найдена, возвращаем первую найденную.
            if (result.Any())
                return result.ElementAt(0);

            return null;
        }

        /// <summary>
        /// Метод находит вершину по её точке в списке вершин графа.
        /// </summary>
        /// <param name="point">точка</param>
        /// <returns>Первая встреченная вершина - если есть хотя бы одна, либо null.</returns>
        public Vertex GetVertexOfGraph(Point point)
        {
            // Метод LINQ для поиска данного объекта в коллекции
            var result = from v in _vertexes
                         where (v.Point == point)
                         select v;

            // Если хотя бы одна вершина найдена, возвращаем первую найденную.
            if (result.Any())
                return result.ElementAt(0);

            return null;
        }

        /// <summary>
        /// Метод находит вершину по её порядковому номеру в списке вершин графа.
        /// </summary>
        /// <param name="number">Порядковый номер</param>
        /// <returns>Вершина - если есть такая, либо null.</returns>
        public Vertex GetVertexOfGraph(Int32 number)
        {
            // Метод LINQ для поиска данного объекта в коллекции
            var result = from v in _vertexes
                         where (v.Number == number)
                         select v;

            // Если хотя бы одна вершина найдена, возвращаем
            if (result.Any())
                return result.ElementAt(0);

            return null;
        }

        /// <summary>
        /// Найти ребро графа по его визуальному объекту.
        /// </summary>
        /// <param name="visual"></param>
        /// <returns>Первое встреченное ребро - если есть хотя бы одно, либо null.</returns>
        public Riber GetRiberOfGraph(DrawingVisual visual)
        {
            // Метод LINQ для поиска данного объекта в коллекции
            var result = from v in _weights
                         where (v.Visual == visual)
                         select v;

            // Если хотя бы одно ребро найдено, возвращаем первое найденное.
            if (result.Any())
                return result.ElementAt(0);

            return null;
        }

        /// <summary>
        /// Метод ищет ребро, в котором присутствуют вершины (в указанном порядке).
        /// </summary>
        /// <param name="v1">Вершина 1.</param>
        /// <param name="v2">Вершина 2.</param>
        /// <returns>Ребро.</returns>
        public Riber GetRiberWithVertexPair(Vertex v1, Vertex v2)
        {
            // Метод LINQ для поиска объектов в коллекции
            var result = from r in _weights
                         where (r.Vertex1 == v1 && r.Vertex2 == v2)
                         select r;

            if (result.Any())
                return result.ElementAt(0);

            return null;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Добавить колонки для всех вершин.
        /// </summary>
        /// <param name="count">Количество колонок.</param>
        private void _BuildColumns(Int32 count)
        {
            for (int i = 0; i < count; i++)
            {
                string name = String.Format(COLUMN_NAME_TEMPLATE, i.ToString());

                // Добавление колонки в DataTable.
                _matrix.Columns.Add(new DataColumn(name, typeof(Int32)));
            }
        }

        /// <summary>
        /// Добавить строки для всех вершин и установить значения по умолчанию для каждой ячейки.
        /// </summary>
        /// <param name="count">Количество строк.</param>
        private void _BuildRows(Int32 count)
        {
            for (int i = 0; i < count; i++)
            {
                DataRow dr = _matrix.NewRow();
                dr[0] = String.Format(ROW_NAME_TEMPLATE, i.ToString());

                // Установить значения по умолчанию для каждой ячейки.
                for (int j = 1; j < count + 1; j++)
                {
                    dr[j] = -1;
                }

                _matrix.Rows.Add(dr);
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Матрица графа.
        /// </summary>
        public DataTable Matrix
        {
            get { return _matrix; }
            set { _matrix = value; }
        }

        /// <summary>
        /// Вершины.
        /// </summary>
        public List<Vertex> Vertexes
        {
            get { return _vertexes; }
            set { _vertexes = value; }
        }

        /// <summary>
        /// Ребра - они же, веса.
        /// </summary>
        public List<Riber> Weights
        {
            get { return _weights; }
            set { _weights = value; }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Наименование колонок.
        /// </summary>
        private const string COLUMN_NAME_TEMPLATE = "Вершина {0}";

        /// <summary>
        /// Наименование строк.
        /// </summary>
        private const string ROW_NAME_TEMPLATE = "Вершина {0}";

        /// <summary>
        /// Наименование заголовка строк.
        /// </summary>
        private const string ROW_HEADER_NAME = "Вершины";

        #endregion

        #region Private fields

        /// <summary>
        /// Матрица графа.
        /// </summary>
        private DataTable _matrix = new DataTable();

        /// <summary>
        /// Список ребер графа.
        /// </summary>
        private List<Riber> _weights = new List<Riber>();

        /// <summary>
        /// Список вершин графа.
        /// </summary>
        private List<Vertex> _vertexes = new List<Vertex>();

        #endregion
    }
}