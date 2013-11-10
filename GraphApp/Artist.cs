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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace GraphApp
{
    /// <summary>
    /// Класс - художник, рисующий графику.
    /// </summary>
    internal sealed class Artist
    {
        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawingSurface"></param>
        public Artist(DrawingCanvas drawingSurface)
        {
            _drawingSurface = drawingSurface;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Метод рисует вершину графа.
        /// </summary>
        /// <param name="number">Порядковый номер.</param>
        /// <param name="topLeftCorner">Координата.</param>
        /// <param name="drawBrush">Цвет рисунка.</param>
        /// <param name="textBrush">Цвет текста.</param>
        /// <param name="isLabelNeeded">Вывести ли номер вершины?</param>
        public DrawingVisual DrawVertex(string number, Point topLeftCorner, Brush drawBrush, Brush textBrush, bool isLabelNeeded)
        {
            DrawingVisual vis = new DrawingVisual();

            using (DrawingContext dc = vis.RenderOpen())
            {
                dc.DrawEllipse(drawBrush, new Pen(Brushes.SteelBlue, 2), topLeftCorner, POINT_RADIUS, POINT_RADIUS);

                if (isLabelNeeded)
                {
                    // Взять настройки шрифта.
                    System.Threading.Thread th = System.Threading.Thread.CurrentThread;
                    FlowDirection fd = new FlowDirection();
                    Typeface fc = new Typeface("Arial");

                    // Точку.
                    Point p = new Point();
                    if (number.Length == 1)
                        p.X = topLeftCorner.X - POINT_RADIUS / 2;
                    else
                        p.X = topLeftCorner.X - POINT_RADIUS;
                    p.Y = topLeftCorner.Y - POINT_RADIUS;

                    FormattedText text = new FormattedText(number, th.CurrentUICulture, fd, fc, 12, textBrush);

                    // Вывести.
                    dc.DrawText(text, p);
                }
            }

            return vis;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="drawBrush"></param>
        /// <returns></returns>
        public DrawingVisual DrawRiber(Point firstPoint, Point secondPoint, Brush drawBrush)
        {
            DrawingVisual vis = new DrawingVisual();

            using (DrawingContext dc = vis.RenderOpen())
            {
                Pen pen = new Pen(drawBrush, 2);
                Pen pen1 = new Pen(drawBrush, 10);
                pen1.EndLineCap = PenLineCap.Triangle;

                // Точки - вершины ребра.
                //Point startPoint = firstPoint;
                //Point finishPoint = secondPoint;

                // Точки для стрелочки на ребре.
                Point arrayPoint1 = new Point();
                Point arrayPoint2 = new Point();

                // Вычилить правильное расположение ребра относительно вершин, а также
                // точки стрелочек на ребре.
                //_CalculateStartPlacement(ref startPoint, ref finishPoint, ref arrayPoint1, ref arrayPoint2);
                _CalculateArrays(firstPoint, secondPoint, ref arrayPoint1, ref arrayPoint2);
                // Рисуем и линию и стрелку.
                dc.DrawLine(pen, firstPoint, secondPoint);//startPoint, finishPoint);
                dc.DrawLine(pen1, arrayPoint1, arrayPoint2);
            }

            return vis;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="drawBrush"></param>
        /// <returns></returns>
        public DrawingVisual DrawLine(Point firstPoint, Point secondPoint, Brush drawBrush)
        {
            DrawingVisual vis = new DrawingVisual();

            using (DrawingContext dc = vis.RenderOpen())
            {
                dc.DrawLine(new Pen(drawBrush, 2), firstPoint, secondPoint);
            }

            return vis;
        }

        /// <summary>
        /// Метод находит центральную точку визуального объекта.
        /// </summary>
        /// <param name="vis">Визуальный объект.</param>
        /// <returns>Центральная точка.</returns>
        public Point GetCenterPointOfVisual(DrawingVisual vis)
        {
            Point center = new Point();

            if (vis != null)
            {
                Rect cont = vis.ContentBounds;
                center = new Point(cont.TopLeft.X + cont.Width / 2, cont.TopLeft.Y + cont.Height / 2);
            }

            return center;
        }

        /// <summary>
        /// Метод вычисляет набор точек для рисования вершин по Архимедовой спирали.
        /// </summary>
        /// <param name="count">Количество точек.</param>
        /// <returns>Набор точек.</returns>
        public List<Point> GetArchimedSpiralPoints(Int32 count)
        {
            List<Point> points = new List<Point>();

            Point center = new Point(_drawingSurface.Width / 2, _drawingSurface.Height / 2);
            double radius = 90;
            double dRadius = 10;
            double angle = 0;
            double dAngle = Math.PI * 50 / 180;

            // Точка начала рисования - центр области рисования.
            Point vertexPoint = center;

            // Рассчитываем точки Архимедовой спирали.
            for (Int32 i = 0; i < count; i++)
            {
                points.Add(vertexPoint);
                double dx = radius * Math.Cos(angle);
                double dy = radius * Math.Sin(angle);
                vertexPoint = new Point(dx + center.X, dy + center.Y);
                radius += dRadius;
                dRadius *= 0.98;
                angle += dAngle;
                dAngle *= 0.97;
            }

            return points;
        }

        /// <summary>
        /// Метод рисует ребра, выделенные цветом, между вершинами, заданными порядковыми номерами.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="path"></param>
        public void DrawPath(Graph graph, List<Int32> path)
        {
            if (path.Count > 1)
                for (int i = path.Count - 1; i > 0; i--)
                {
                    Int32 prev = path[i];
                    Int32 next = path[i - 1];
                    // Найти ребро с такими вершинами
                    Riber r = graph.GetRiberWithVertexPair(graph.GetVertexOfGraph(prev + 1), graph.GetVertexOfGraph(next + 1));

                    // Перерисовать визуальный объект этого ребра
                    DrawingVisual vis = r.Visual;

                    _drawingSurface.DeleteVisual(vis);

                    r.Visual = DrawRiber(r.Vertex1.Point, r.Vertex2.Point, Brushes.Red);

                    _drawingSurface.AddVisual(r.Visual);
                }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="vertexes"></param>
        public void SelectVertexes(Graph graph, List<Int32> vertexes)
        {
            // Выделяем медианы цветом
            foreach (Int32 value in vertexes)
            {
                Vertex v = graph.GetVertexOfGraph(value + 1);
                UpdateVertex(v, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="isSelected"></param>
        public void UpdateVertex(Vertex v, bool isSelected)
        {
            _drawingSurface.DeleteVisual(v.Visual);

            DrawingVisual newvis = new DrawingVisual();

            if (isSelected)
                newvis = DrawVertex((v.Number - 1).ToString(), v.Point, Brushes.Red, Brushes.White, true);
            else
                newvis = DrawVertex((v.Number - 1).ToString(), v.Point, Brushes.LightBlue, Brushes.Black, true);

            _drawingSurface.AddVisual(newvis);

            v.Visual = newvis;
        }

        /// <summary>
        /// Метод удаляет визуальный объект, нарисованный в указанной точке.
        /// </summary>
        /// <param name="point">Точка.</param>
        /// <param name="isNeedToReorderVertexes">Нужно ли переопределять номера вершин.</param>
        public void RemoveElement(Graph graph, Point point, bool isNeedToReorderVertexes)
        {
            DrawingVisual vis = _drawingSurface.GetVisual(point);
            if (vis != null)
            {
                // Если это вершина - удаляем её, и все подходящие к ней ребра.
                // Если это ребро - удаляем только его.
                if (graph.IsVertexOfGraph(vis))
                {
                    // Берем вершину.
                    Vertex vertex = graph.GetVertexOfGraph(vis);
                    List<Riber> ribers = new List<Riber>();

                    // Находим все ребра, которые подходят к неё или отходят от неё.
                    foreach (Riber r in graph.Weights)
                    {
                        if (r.Vertex1 == vertex || r.Vertex2 == vertex)
                        {
                            ribers.Add(r);
                        }
                    }

                    // Удаляем эти ребра и их визуальные объекты.
                    for (int i = 0; i < ribers.Count; i++)
                    {
                        Riber dr = ribers[i];
                        DrawingVisual dv = dr.Visual;
                        _drawingSurface.DeleteVisual(dv);
                        graph.Weights.Remove(dr);
                    }

                    // Удаляем вершину и её визуальный объект.
                    Int32 number = vertex.Number;
                    _drawingSurface.DeleteVisual(vis);
                    graph.Vertexes.Remove(vertex);

                    // Переопределяем порядковые номера вершин в массиве.
                    if (isNeedToReorderVertexes)
                        _UpdateVertexesNumbers(number, graph);
                }
                else
                {
                    // Удаляем ребро и его визуальный объект.
                    Riber riber = graph.GetRiberOfGraph(vis);
                    graph.Weights.Remove(riber);
                    _drawingSurface.DeleteVisual(vis);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="numbers"></param>
        public void RemoveVertexes(Graph graph, List<Int32> numbers)
        {
            // Нарисовать результат - удаляем вершины.
            foreach (Int32 value in numbers)
            {
                Vertex vertex = graph.GetVertexOfGraph(value + 1);
                RemoveElement(graph, vertex.Point, false);
            }
        }

        /// <summary>
        /// Обновляем нумерацию ВСЕХ вершин.
        /// </summary>
        /// <param name="graph">Граф.</param>
        public void UpdateAllVertexesNumbers(Graph graph)
        {
            int counter = 1;

            foreach (Vertex vertex in graph.Vertexes)
            {
                vertex.Number = counter++;
            }

            _UpdateLayout(graph);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Обновляем нумерацию вершин, начиная с некоторой.
        /// </summary>
        /// <param name="index">Номер вершины.</param>
        /// <param name="graph">Граф.</param>
        private void _UpdateVertexesNumbers(Int32 index, Graph graph)
        {
            for (int i = index - 1; i < graph.Vertexes.Count; i++)
                graph.Vertexes[i].Number = graph.Vertexes[i].Number - 1;

            _UpdateLayout(graph);
        }

        /// <summary>
        /// Метод вычисляет точки начала и конца ребра, и возвращает их по ссылке.
        /// Ребра ориентированных графов ВСЕГДА рисуются по часовой стрелке.
        /// Так как между 2мя вершинами может быть 2 ребра, то они рисуются с двух сторон
        /// от кружков вершин, чтобы не сливались друг с другом.
        /// С помощью Четвертей Декартовой Координатной Системы определяем, как следует
        /// сместить точки ребер относительно центров вершин.
        /// </summary>
        /// <param name="startPoint">ЦЕНТР первой вершины.</param>
        /// <param name="finishPoint">ЦЕНТР второй вершины.</param>
        /// <param name="startArrayPoint">Первая вершина стрелочки.</param>
        /// <param name="finishArrayPoint">Вторая вершина стрелочки.</param>
        //private void _CalculateStartPlacement(ref Point startPoint, ref Point finishPoint, ref Point startArrayPoint, ref Point finishArrayPoint)
        //{
        //    // Допущение.
        //    double act = POINT_RADIUS * 5;

        //    // I четверть.
        //    if ((startPoint.X <= finishPoint.X || (startPoint.X - finishPoint.X) < act) && startPoint.Y > finishPoint.Y)
        //    {
        //        // Ребро рисуется правее вершин.
        //        startPoint.X += POINT_RADIUS;
        //        finishPoint.X += POINT_RADIUS;

        //        // Центр ребра.
        //        double projectionX = finishPoint.X - startPoint.X;
        //        double projectionY = finishPoint.Y - startPoint.Y;
        //        Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);

        //        // Координаты стрелочки.
        //        double k = 0.1;
        //        double otn = projectionY;

        //        if (projectionX != 0)
        //            otn = projectionY / projectionX;

        //        if (otn > 10)
        //            otn = 10;
        //        else if (otn < -10)
        //            otn = -10;

        //        startArrayPoint = center;
        //        finishArrayPoint = center;
        //        finishArrayPoint.X += k;
        //        finishArrayPoint.Y -= k * Math.Abs(otn);
        //    }
        //    // II четверть.
        //    else if (startPoint.X > finishPoint.X && (startPoint.Y >= finishPoint.Y || (finishPoint.Y - startPoint.Y) < act))
        //    {
        //        // Ребро рисуется выше вершин.
        //        startPoint.Y -= POINT_RADIUS;
        //        finishPoint.Y -= POINT_RADIUS;

        //        // Центр ребра.
        //        double projectionX = finishPoint.X - startPoint.X;
        //        double projectionY = finishPoint.Y - startPoint.Y;
        //        Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);

        //        // Координаты стрелочки.
        //        double k = 0.1;
        //        double otn = projectionY;

        //        if (projectionX != 0)
        //            otn = projectionY / projectionX;
        //        startArrayPoint = center;
        //        finishArrayPoint = center;
        //        finishArrayPoint.X -= k;
        //        finishArrayPoint.Y -= k * Math.Abs(otn);

        //    }
        //    // III четверть.
        //    else if ((startPoint.X >= finishPoint.X || (finishPoint.X - startPoint.X) < act) && startPoint.Y <= finishPoint.Y)
        //    {
        //        // Ребро рисуется левее вершин.
        //        startPoint.X -= POINT_RADIUS;
        //        finishPoint.X -= POINT_RADIUS;

        //        double projectionX = finishPoint.X - startPoint.X;
        //        double projectionY = finishPoint.Y - startPoint.Y;
        //        Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);
        //        double k = 0.1;

        //        double otn = projectionY;

        //        if (projectionX != 0)
        //            otn = projectionY / projectionX;

        //        if (otn > 10)
        //            otn = 10;
        //        else if (otn < -10)
        //            otn = -10;

        //        startArrayPoint = center;
        //        finishArrayPoint = center;
        //        finishArrayPoint.X -= k;
        //        finishArrayPoint.Y += k * Math.Abs(otn);
        //    }
        //    // IV четверть.
        //    else if (startPoint.X < finishPoint.X && (startPoint.Y <= finishPoint.Y || (startPoint.Y - finishPoint.Y) < act))
        //    {
        //        // Ребро рисуется ниже вершин.
        //        startPoint.Y += POINT_RADIUS;
        //        finishPoint.Y += POINT_RADIUS;

        //        double projectionX = finishPoint.X - startPoint.X;
        //        double projectionY = finishPoint.Y - startPoint.Y;
        //        Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);
        //        double k = 0.1;
        //        double otn = projectionY;

        //        if (projectionX != 0)
        //            otn = projectionY / projectionX;

        //        startArrayPoint = center;
        //        finishArrayPoint = center;
        //        finishArrayPoint.X += k;
        //        finishArrayPoint.Y += k * Math.Abs(otn);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="finishPoint"></param>
        /// <param name="startArrayPoint"></param>
        /// <param name="finishArrayPoint"></param>
        private void _CalculateArrays(Point startPoint, Point finishPoint, ref Point startArrayPoint, ref Point finishArrayPoint)
        {
            // Допущение.
            double act = POINT_RADIUS * 5;

            // I четверть.
            if ((startPoint.X <= finishPoint.X || (startPoint.X - finishPoint.X) < act) && startPoint.Y > finishPoint.Y)
            {
                // Ребро рисуется правее вершин.
                startPoint.X += POINT_RADIUS;
                finishPoint.X += POINT_RADIUS;

                // Центр ребра.
                double projectionX = finishPoint.X - startPoint.X;
                double projectionY = finishPoint.Y - startPoint.Y;
                Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);

                // Координаты стрелочки.
                double k = 0.1;
                double otn = projectionY;

                if (projectionX != 0)
                    otn = projectionY / projectionX;

                if (otn > 10)
                    otn = 10;
                else if (otn < -10)
                    otn = -10;

                startArrayPoint = center;
                finishArrayPoint = center;
                finishArrayPoint.X += k;
                finishArrayPoint.Y -= k * Math.Abs(otn);
            }
            // II четверть.
            else if (startPoint.X > finishPoint.X && (startPoint.Y >= finishPoint.Y || (finishPoint.Y - startPoint.Y) < act))
            {
                // Ребро рисуется выше вершин.
                startPoint.Y -= POINT_RADIUS;
                finishPoint.Y -= POINT_RADIUS;

                // Центр ребра.
                double projectionX = finishPoint.X - startPoint.X;
                double projectionY = finishPoint.Y - startPoint.Y;
                Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);

                // Координаты стрелочки.
                double k = 0.1;
                double otn = projectionY;

                if (projectionX != 0)
                    otn = projectionY / projectionX;
                startArrayPoint = center;
                finishArrayPoint = center;
                finishArrayPoint.X -= k;
                finishArrayPoint.Y -= k * Math.Abs(otn);

            }
            // III четверть.
            else if ((startPoint.X >= finishPoint.X || (finishPoint.X - startPoint.X) < act) && startPoint.Y <= finishPoint.Y)
            {
                // Ребро рисуется левее вершин.
                startPoint.X -= POINT_RADIUS;
                finishPoint.X -= POINT_RADIUS;

                double projectionX = finishPoint.X - startPoint.X;
                double projectionY = finishPoint.Y - startPoint.Y;
                Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);
                double k = 0.1;

                double otn = projectionY;

                if (projectionX != 0)
                    otn = projectionY / projectionX;

                if (otn > 10)
                    otn = 10;
                else if (otn < -10)
                    otn = -10;

                startArrayPoint = center;
                finishArrayPoint = center;
                finishArrayPoint.X -= k;
                finishArrayPoint.Y += k * Math.Abs(otn);
            }
            // IV четверть.
            else if (startPoint.X < finishPoint.X && (startPoint.Y <= finishPoint.Y || (startPoint.Y - finishPoint.Y) < act))
            {
                // Ребро рисуется ниже вершин.
                startPoint.Y += POINT_RADIUS;
                finishPoint.Y += POINT_RADIUS;

                double projectionX = finishPoint.X - startPoint.X;
                double projectionY = finishPoint.Y - startPoint.Y;
                Point center = new Point(startPoint.X + projectionX / 2, startPoint.Y + projectionY / 2);
                double k = 0.1;
                double otn = projectionY;

                if (projectionX != 0)
                    otn = projectionY / projectionX;

                startArrayPoint = center;
                finishArrayPoint = center;
                finishArrayPoint.X += k;
                finishArrayPoint.Y += k * Math.Abs(otn);
            }
        }


        /// <summary>
        /// Метод находит центральную точку визуального объекта.
        /// </summary>
        /// <param name="vis">Визуальный объект.</param>
        /// <returns>Центральная точка.</returns>
        private Point _GetCenterPointOfVisual(DrawingVisual vis)
        {
            Point center = new Point();

            if (vis != null)
            {
                Rect cont = vis.ContentBounds;
                center = new Point(cont.TopLeft.X + cont.Width / 2, cont.TopLeft.Y + cont.Height / 2);
            }

            return center;
        }

        /// <summary>
        /// Метод обновляет картинку графа.
        /// </summary>
        private void _UpdateLayout(Graph graph)
        {
            foreach (Vertex v in graph.Vertexes)
                UpdateVertex(v, false);

            foreach (Riber r in graph.Weights)
            {
                _drawingSurface.DeleteVisual(r.Visual);

                DrawingVisual newvis = DrawRiber(r.Vertex1.Point, r.Vertex2.Point, Brushes.Black);

                _drawingSurface.AddVisual(newvis);

                r.Visual = newvis;
            }
        }

        #endregion

        #region Private constants

       /// <summary>
        /// Радиус вершин.
        /// </summary>
        private readonly Int32 POINT_RADIUS = 7;

        #endregion

        #region Private fields

        /// <summary>
        /// 
        /// </summary>
        private DrawingCanvas _drawingSurface;

        #endregion
    }
}
