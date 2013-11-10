using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Media;
using System.Windows.Media;

namespace GraphApp
{
    /// <summary>
    /// Класс описывает область рисования.
    /// </summary>
    internal sealed class DrawingCanvas : Canvas
    {
        #region Protected methods

        /// <summary>
        /// Возвращает количество визуальных объектов.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }

        /// <summary>
        /// Возвращает визуальный объект по индексу.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Добавляет новый визуальный объект
        /// </summary>
        /// <param name="visual"></param>
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);
            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        /// <summary>
        /// Возвращает визуальный объект по точке.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            return hitResult.VisualHit as DrawingVisual;
        }

        /// <summary>
        /// Возвращает визуальные объекты из региона.
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public List<DrawingVisual> GetVisuals(Geometry region)
        {
            hits.Clear();

            GeometryHitTestParameters parameters = new GeometryHitTestParameters(region);
            HitTestResultCallback callback = new HitTestResultCallback(this.HitTestCallback);
            VisualTreeHelper.HitTest(this, null, callback, parameters);

            return hits;
        }

        /// <summary>
        /// Удаляет визуальный объект из области рисования.
        /// </summary>
        /// <param name="visual"></param>
        public void DeleteVisual(Visual visual)
        {
            foreach (Visual v in visuals)
            {
                if (v == visual)
                {
                    visuals.Remove(visual);
                    base.RemoveVisualChild(visual);
                    base.RemoveLogicalChild(visual);
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearGraphic()
        {
            Int32 count = visuals.Count;

            for (Int32 i = 0; i < count; i++)
            {
                Visual visual = visuals[0];
                visuals.Remove(visual);
                base.RemoveVisualChild(visual);
                base.RemoveLogicalChild(visual);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Находит все визуальные объекты, попадающие под клик.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            GeometryHitTestResult geometryResult = (GeometryHitTestResult)result;
            DrawingVisual vis = result.VisualHit as DrawingVisual;

            if (vis != null && geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
            {
                hits.Add(vis);
            }
            return HitTestResultBehavior.Continue;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Коллекция визуальных объектов
        /// </summary>
        private List<Visual> visuals = new List<Visual>();

        /// <summary>
        /// Коллекция визуальных объектов - результатов хит-теста (кликов)
        /// </summary>
        private List<DrawingVisual> hits = new List<DrawingVisual>();

        #endregion
    }
}
