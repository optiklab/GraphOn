using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace GraphApp
{
    /// <summary>
    /// Класс определяющий ребро графа.
    /// </summary>
    internal sealed class Riber
    {
        #region Constructor

        /// <summary>
        /// Конструктор.
        /// </summary>
        public Riber(string name, Vertex vertex1, Vertex vertex2, double weight, DrawingVisual visual)
        {
            _name = name;
            _vertex1 = vertex1;
            _vertex2 = vertex2;
            _weight = weight;
            _visual = visual;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Имя.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Вес.
        /// </summary>
        public double Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        /// <summary>
        /// Визуальный объект - отображение ребра на картинке.
        /// </summary>
        public DrawingVisual Visual
        {
            get { return _visual; }
            set { _visual = value; }
        }

        /// <summary>
        /// Вершина 1.
        /// </summary>
        public Vertex Vertex1
        {
            get { return _vertex1; }
            set { _vertex1 = value; }
        }

        /// <summary>
        /// Вершина 2.
        /// </summary>
        public Vertex Vertex2
        {
            get { return _vertex2; }
            set { _vertex2 = value; }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Вершина 1.
        /// </summary>
        private Vertex _vertex1;

        /// <summary>
        /// Вершина 2.
        /// </summary>
        private Vertex _vertex2;

        /// <summary>
        /// Имя.
        /// </summary>
        private string _name;

        /// <summary>
        /// Вес.
        /// </summary>
        private double _weight;

        /// <summary>
        /// Визуальный объект - отображение ребра на картинке.
        /// </summary>
        private DrawingVisual _visual;

        #endregion
    }
}
