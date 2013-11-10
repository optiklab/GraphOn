using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace GraphApp
{
    /// <summary>
    /// Класс определяющий вершину графа.
    /// </summary>
    internal sealed class Vertex
    {
        #region Constructor

        public Vertex()
        {
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public Vertex(string name, Int32 number, Point point, DrawingVisual visual)
        {
            _name = name;
            _number = number;
            _point = point;
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
        /// Порядковый номер.
        /// </summary>
        public Int32 Number
        {
            get { return _number; }
            set { _number = value; }
        }

        /// <summary>
        /// Визуальный объект - отображение вершины на картинке.
        /// </summary>
        public DrawingVisual Visual
        {
            get { return _visual; }
            set { _visual = value; }
        }

        /// <summary>
        /// Точка.
        /// </summary>
        public Point Point
        {
            get { return _point; }
            set { _point = value; }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Имя.
        /// </summary>
        private string _name;

        /// <summary>
        /// Порядковый номер.
        /// </summary>
        private Int32 _number;

        /// <summary>
        /// Точка.
        /// </summary>
        private Point _point;

        /// <summary>
        ///  Визуальный объект - отображение вершины на картинке.
        /// </summary>
        private DrawingVisual _visual;
        
        #endregion
    }
}
