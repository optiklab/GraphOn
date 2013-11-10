using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace GraphApp
{
    /// <summary>
    /// Класс осуществляет решение задач поиска диаметра и медианы графа.
    /// </summary>
    internal sealed class Solver
    {
        #region Public methods

        /// <summary>
        /// Метод поиска недостижимых вершин графа к указанной вершине.
        /// </summary>
        /// <param name="graph">Граф для поиска вершин.</param>
        /// <param name="number">Номер вершины.</param>
        /// <returns>Список вершин графа.</returns>
        public static List<Int32> FindVertexes(Graph graph, Int32 number)
        {
            // Сгенерируем матрицу.
            double[,] matrix = _GetMatrix(graph);
            // Здесь будет список вершин.
            List<Int32> vertexes = new List<Int32>();
            // Количество вершин в графе.
            int vcount = graph.Vertexes.Count;

            for (int j = 0; j < vcount; j++)
            {
                if (j != number)
                {
                    // Если кратчайший путь не найден (-1), то добавляем эту вершину,
                    // её нужно будет удалить - т.к. из неё не достижима заданная.
                    List<Int32> path = new List<Int32>();
                    if (Dijkstra(matrix, graph.Vertexes.Count, j, number, out path) == -1)
                    {
                        vertexes.Add(j);
                    }
                }
            }

            return vertexes;
        }


        /// <summary>
        /// Метод поиска медианы.
        /// </summary>
        public static List<Int32> FindMedian(Graph graph)
        {
            List<Int32> medians = new List<Int32>();

            if (Solver.IsLinked(graph))
                medians = _FindMedian(graph);

            return medians;
        }

        /// <summary>
        /// Метод поиска диаметра.
        /// </summary>
        public static double FindDiameter(Graph graph, out List<Int32> path)
        {
            path = new List<Int32>();
            double diam = 0;

            // Проверяем, что ни одна вершина графа не является "безсвязной"
            if (Solver.IsLinked(graph))
            {
                // Вычисляем диаметр
                diam = _FindDiameter(graph, out path);
            }

            return diam;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Метод определения того, что ни одна вершина не является "брошенной" - т.е. без связей.
        /// </summary>
        /// <param name="graph">Граф.</param>
        /// <returns>True - если граф связный, иначе - False.</returns>
        private static bool IsLinked(Graph graph)
        {
            bool result = true;

            foreach (Vertex v in graph.Vertexes)
            {
                bool isVertexInSomeRiber = false;

                foreach (Riber r in graph.Weights)
                {
                    if (r.Vertex1 == v || r.Vertex2 == v)
                    {
                        isVertexInSomeRiber = true;
                        break;
                    }
                }

                if (isVertexInSomeRiber == false)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Метод поиска медианы графа.
        /// </summary>
        /// <param name="graph">Граф для поиска медианы.</param>
        /// <returns>Список целочисленных номеров вершин, являющихся медианами.</returns>
        public static List<Int32> _FindMedian(Graph graph)
        {
            int vcount = graph.Vertexes.Count;// Количество вершин в графе
            double[] summas = new double[vcount]; // суммы кратчайших расстояний каждой вершины
            double[,] matrix = _GetMatrix(graph);

            // Находим суммы наикратчайших расстояний всех пар вершин.
            for (int i = 0; i < vcount; i++)
            {
                double summtemp = 0;
                summas[i] = -1;

                for (int j = 1; j < vcount + 1; j++)
                {
                    if (i != (j - 1))
                    {
                        List<Int32> path = new List<Int32>();
                        double temp1 = Dijkstra(matrix, vcount, i, j - 1, out path);

                        if (temp1 > 0)
                            summtemp += temp1;
                    }
                }

                summas[i] = summtemp;
            }

            // Находим минимальную сумму.
            double temp2 = -1;
            for (int i = 0; i < vcount; i++)
            {
                if (temp2 > summas[i] || temp2 == -1)
                    temp2 = summas[i];
            }

            // Находим список вершин с минимальными суммами - медианы.
            List<Int32> medians = new List<Int32>();

            for (int i = 0; i < vcount; i++)
            {
                if (summas[i] == temp2)// && summas[i] != 0)
                    medians.Add(i);
            }

            // Возвращаем массив медиан.
            return medians;
        }

        /// <summary>
        /// Метод поиска диаметра графа.
        /// </summary>
        /// <param name="graph">Граф для поиска медианы.</param>
        /// <returns>ВЕЛИЧИНA ДИАМЕТРА (длина пути), и в качестве  OUT параметра - путь (маршрут).</returns>
        private static double _FindDiameter(Graph graph, out List<Int32> path)
        {
            double weight = -1;
            List<Int32> resultPath = new List<Int32>();
            double[,] matrix = _GetMatrix(graph);

            int vcount = graph.Vertexes.Count;// Количество вершин в графе
            for (int i = 0; i < vcount; i++)
                for (int j = 1; j < vcount + 1; j++)
                {
                    if (i != (j - 1))
                    {
                        List<Int32> newPath = new List<Int32>();
                        double newWeight = Dijkstra(matrix, graph.Vertexes.Count, i, j - 1, out newPath);

                        if (weight < newWeight)
                        {
                            weight = newWeight;
                            resultPath = newPath;
                        }
                    }
                }

            path = resultPath;

            // Находим максимум.
            return weight;
        }

        /// <summary>
        /// Метод конвертирует матрицу смежности из DataTable типа в массив для
        /// удобства последующих вычислений.
        /// </summary>
        /// <param name="graph">Граф, откуда берется матрица смежности.</param>
        /// <returns>Двумерный массив величин являющийся матрицей смежности.</returns>
        private static double[,] _GetMatrix(Graph graph)
        {
            // Количество вершин в графе
            int vcount = graph.Vertexes.Count;
            double[,] a = new double[vcount, vcount];

            // Копируем все значения из матрицы графа в новый массив.
            for (int i = 0; i < vcount; i++)
            {
                for (int k = 0, j = 1; j < vcount + 1; k++, j++)
                {
                    string str = graph.Matrix.Rows[i][j].ToString();
                    a[i, k] = Convert.ToDouble(str);
                }
            }

            return a;
        }

        /// <summary>
        /// Метод имплементирует алгоритм Дейкстра для поиска наикратчайшего расстояния 
        /// между двумя вершинами.
        /// </summary>
        /// <param name="a">Матрица смежности графа.</param>
        /// <param name="vcount">Количество вершин.</param>
        /// <param name="start">Первая вершина.</param>
        /// <param name="end">Вторая вершина.</param>
        /// <returns>Длина маршрута.</returns>
        private static double Dijkstra(double[,] a, int vcount, int start, int end, out List<Int32> path)
        {
            path = new List<Int32>();

            int infinity=1000000; // Бесконечность - любое большое число, которое больше макс. значения веса ребра

            // Будем искать путь из вершины start в вершину end
            int[] x = new int[vcount]; //Массив, содержащий единицы и нули для каждой вершины,
                            // x[i]=0 - еще не найден кратчайший путь в i-ю вершину,
                            // x[i]=1 - кратчайший путь в i-ю вершину уже найден
            double[] t = new double[vcount];  //t[i] - длина кратчайшего пути от вершины s в i
            int[] h = new int[vcount];  //h[i] - вершина, предшествующая i-й вершине на кратчайшем пути

            // Инициализируем начальные значения массивов
            int u; // Счетчик вершин
            for (u=0; u<vcount; u++)
            {
                t[u]=infinity; //Сначала все кратчайшие пути из s в i равны бесконечности
                x[u]=0;        // и нет кратчайшего пути ни для одной вершины
            }
            h[start] = 0; // s - начало пути, поэтому этой вершине ничего не предшествует
            t[start] = 0; // Кратчайший путь из s в s равен 0
            x[start] = 1; // Для вершины s найден кратчайший путь
            int v = start;    // Делаем s текущей вершиной
   
            while(true)
            {
                // Перебираем все вершины, смежные v, и ищем для них кратчайший путь
                for (u = 0; u < vcount; u++)
                {
                    if (a[v,u] == -1) //if (a[v][u] == -1)
                        continue; // Вершины u и v несмежные
                    if (x[u] == 0 && t[u] > t[v] + a[v,u])//a[v][u]) //Если для вершины u еще не найден кратчайший путь и новый путь в u короче чем старый, то
                    {
                        t[u] = t[v] + a[v,u];// a[v][u]; //запоминаем более короткую длину пути в массив t и
                        h[u]=v; //запоминаем, что v->u часть кратчайшего пути из s->u
                    }
                }

                // Ищем из всех длин некратчайших путей самый короткий
                double w = infinity;  // Для поиска самого короткого пути
                v=-1;            // В конце поиска v - вершина, в которую будет 
                                // найден новый кратчайший путь. Она станет 
                                // текущей вершиной
                for (u = 0; u < vcount; u++) // Перебираем все вершины.
                {
                    if (x[u] == 0 && t[u] < w) // Если для вершины не найден кратчайший путь и если длина пути в вершину u меньше уже найденной, то
                    {
                        v=u; // текущей вершиной становится u-я вершина
                        w=t[u];
                    }
                }

                // Нет пути
                if(v==-1)
                    return -1;

                // Найден кратчайший путь, выводим его
                if (v == end) 
                {
                    u = end;
                    while(u!=start)
                    {
                        path.Add(u);
                        // Маршрут через ребра.
                        u=h[u];
                    }
                    path.Add(start);
                    //Длина пути
                    return t[end];
                }

                x[v]=1;
            }
        }

        #endregion
    }
}
