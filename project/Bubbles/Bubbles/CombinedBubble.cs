using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using static Bubbles.Bubble;

namespace Bubbles
{
    public class CombinedBubble : Obj
    {
        private Bubble bubble1;
        private Bubble bubble2;

        public CombinedBubble(int id, Bubble bubble1, Bubble bubble2) : base(id)
        {
            this.bubble1 = bubble1;
            this.bubble2 = bubble2;
            //this.bubble2.Id = bubble1.Id;
        }

        private static List<Vector3D> GetIntersectionPoint(Bubble b1, Bubble b2)
        {
            List<Vector3D> res = new List<Vector3D>();
            // Вычисляем расстояние между центрами сфер
            double distance = Math.Sqrt(
                             Math.Pow(b2.Center.X - b1.Center.X, 2) +
                             Math.Pow(b2.Center.Y - b1.Center.Y, 2) +
                             Math.Pow(b2.Center.Z - b1.Center.Z, 2));

            // Проверяем, пересекаются ли сферы
            if (distance > (b1.Radius + b2.Radius) || distance < Math.Abs(b1.Radius - b2.Radius)) 
            {
                res.Add(new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity));
                res.Add(new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity));
                return res;
            }
                
            // Находим точку P2, которая является центром окружности пересечения
            double a = (b1.Radius * b1.Radius - b2.Radius * b2.Radius + distance * distance) / (2 * distance);
            double h = (float)Math.Sqrt(b1.Radius * b1.Radius - a * a);

            // Находим точку P2
            Vector3D P2 = b1.Center + (b2.Center - b1.Center) * (a / distance);

            // Вектор перпендикулярный к вектору между центрами
            Vector3D direction = b2.Center - b1.Center;
            direction.Normalize();
            Vector3D perpendicular = Vector3D.CrossProduct(direction, new Vector3D(1, 0, 0));
            if (perpendicular.Length < 0.01f) // Если направление совпадает с осью X
                perpendicular = Vector3D.CrossProduct(direction, new Vector3D(0, 1, 0));

            perpendicular.Normalize();
            perpendicular = perpendicular * h;

            Vector3D point = P2 + perpendicular * (float)Math.Cos(0) + Vector3D.CrossProduct(direction, perpendicular) * (float)Math.Sin(0);
            
            res.Add(P2); // центр окружности пересечения
            res.Add(point); // точка на окружности
            return res;
        }
        private static List<Vector3D> GetIntersectionPoint(CombinedBubble cb, Bubble b)
        {
            /*
             * Эта функция проверяет на пересечение пузырьковый кластер и отдельный пузырёк.
             * Отдельно проверяется пересечение с каждой частью кластера.
             * Далее 
             * - если отдельный пузырёк пересекается только с один пузырьком из кластера,
             *   то мы возвращаем это пересечение;
             * - если пересечений нет, то возвращает массив с двумя точками в + бесконечности;
             * - если точек пересечения две, возвращаем null как индикатор ошибки.
             */

            List<Vector3D> res1 = GetIntersectionPoint(cb.Bubble1, b);
            List<Vector3D> res2 = GetIntersectionPoint(cb.Bubble2, b);
            if (res1[0].X == double.PositiveInfinity && res2[0].X != double.PositiveInfinity)
                return res2;
            else if (res1[0].X != double.PositiveInfinity && res2[0].X == double.PositiveInfinity)
                return res1;
            else if (res1[0].X == double.PositiveInfinity && res2[0].X == double.PositiveInfinity)
                return res1;
            else 
                return null;
        }

        private static double ContactAngle(Bubble b1, Bubble b2)
        {
            List<Vector3D> Points = GetIntersectionPoint(b1, b2);
            Vector3D commonPoint = Points[1];
            Console.WriteLine(commonPoint);
            Vector3D vectorAB = b1.Center - commonPoint;
            Vector3D vectorAC = b2.Center - commonPoint;

            double lengthAB = vectorAB.Length;
            double lengthAC = vectorAC.Length;

            double dotProduct = Vector3D.DotProduct(vectorAB, vectorAC);

            double cosAngle = dotProduct / (lengthAB * lengthAC);

            cosAngle = Math.Max(-1, Math.Min(1, cosAngle));

            double angleInRadians = Math.Acos(cosAngle);
            double angleInDegrees = angleInRadians * (180.0 / Math.PI);

            Console.WriteLine(angleInDegrees);
            return angleInDegrees;
        }
        private static Bubble MergeBubbles(Bubble b1, Bubble b2)
        {
            if (b1 == null || b2 == null)
            {
                Console.WriteLine("Не удалось слить пузыри: один из пузырей отсутствует.");
                return null;
            }

            Vector3D newCenter = (b1.Center + b2.Center) / 2;

            double volume1 = (4.0 / 3.0) * Math.PI * Math.Pow(b1.Radius, 3);
            double volume2 = (4.0 / 3.0) * Math.PI * Math.Pow(b2.Radius, 3);

            double totalVolume = volume1 + volume2;

            double newRadius = Math.Pow((totalVolume * 3.0) / (4.0 * Math.PI), 1.0 / 3.0);

            b1.Center = newCenter;
            b1.Radius = newRadius;

            b2 = null;

            Console.WriteLine("Пузыри слиты в один большой пузырь.");
            return b1;
        }
        private static List<Bubble> PushBubblesApart(Bubble b1, Bubble b2, bool from_combined)
        {
            List<Bubble> res = new List<Bubble>();
            if (b1 == null || b2 == null)
            {
                Console.WriteLine("Не удалось оттолкнуть пузыри: один из пузырей отсутствует.");
                res.Add(b1);
                res.Add(b2);
                return res;
            }

            Vector3D direction = b2.Center - b1.Center;

            double currentDistance = direction.Length;

            double requiredDistance = b1.Radius + b2.Radius;

            if (currentDistance < requiredDistance)
            {
                direction.Normalize();

                double pushDistance = requiredDistance - currentDistance;
                if (from_combined)
                {
                    b2.Center += direction * (pushDistance + 0.0000002); // Отталкиваем второй пузырь
                }
                else
                {
                    b1.Center -= (direction * (pushDistance / 2 + 0.0000001)); // Отталкиваем первый пузырь
                    b2.Center += direction * (pushDistance / 2 + 0.0000001); // Отталкиваем второй пузырь
                }
                
                Console.WriteLine("Пузыри оттолкнуты друг от друга.");
            }
            else
            {
                Console.WriteLine("Пузыри уже находятся на необходимом расстоянии друг от друга.");
            }
            res.Add(b1);
            res.Add(b2);
            return res;
        }
        private void CreateClusters()
        {
            // Проверяем, что оба пузыря существуют
            if (bubble1 == null || bubble2 == null)
            {
                Console.WriteLine("Не удалось создать кластеры: один из пузырей отсутствует.");
                return;
            }

            // Находим точку пересечения
            List<Vector3D> Points = GetIntersectionPoint(bubble1, bubble2);
            Vector3D centerOfIntersect = Points[0];
            Vector3D intersectionPoint = Points[1];
            //Console.WriteLine(commonPoint);
            if (intersectionPoint == new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity))
            {
                Console.WriteLine("Сферы не пересекаются, кластеры не могут быть созданы.");
                return;
            }

            // Вычисляем векторы от точек пересечения до центров пузырей
            Vector3D vectorToBubble1 = bubble1.Center - intersectionPoint;
            Vector3D vectorToBubble2 = bubble2.Center - intersectionPoint;

            // Нормализуем векторы
            vectorToBubble1.Normalize();
            vectorToBubble2.Normalize();

            // Проверяем угол между векторами
            double angle = Vector3D.DotProduct(vectorToBubble1, vectorToBubble2);

            // Ограничиваем значение угла в диапазоне [-1, 1]
            if (angle < -1.0) angle = -1.0;
            if (angle > 1.0) angle = 1.0;

            angle = Math.Acos(angle) * (180.0 / Math.PI); // Угол в градусах

            if (Math.Abs(angle - 60.0) > 0.01) // Если угол не равен 60 градусам
            {
                Console.WriteLine("Угол между центрами не равен 60 градусам, кластеры не могут быть созданы.");
                return;
            }
            
            Vector3D membraneNormal = bubble2.Center - bubble1.Center;
            membraneNormal.Normalize();

            bubble1.MembraneNormal = membraneNormal;
            bubble2.MembraneNormal = membraneNormal;
            // Создаем новые сегменты для первого пузыря
            SphericalSegment newPart1A = CreateSegment(bubble1.Part1, bubble1.MembraneNormal);
            SphericalSegment newPart1B = CreateSegment(bubble1.Part2, bubble1.MembraneNormal);

            // Создаем новые сегменты для второго пузыря
            SphericalSegment newPart2A = CreateSegment(bubble2.Part1, bubble2.MembraneNormal);
            SphericalSegment newPart2B = CreateSegment(bubble2.Part2, bubble2.MembraneNormal);

            
            // Устанавливаем направления сегментов
            // Устанавливаем направления для part1
            newPart1A.Direction = membraneNormal;
            newPart1A.Direction.Normalize();
            newPart1B.Direction = -newPart1A.Direction; // Направление должно быть противоположным

            // Устанавливаем направления для part2
            newPart2A.Direction = -newPart1A.Direction;
            newPart2B.Direction = -newPart1B.Direction; // Направление должно быть противоположным

            Console.WriteLine($"newPart1A.Direction = {newPart1A.Direction}, \nnewPart1B.Direction = {newPart1B.Direction},\n " +
                $"newPart1A.Direction = {newPart2A.Direction}, \nnewPart1B.Direction = {newPart2B.Direction}\n");


            // Обновляем высоты сегментов
            newPart1A.Height = CalculateHeight(newPart1A, centerOfIntersect, newPart1A.Direction);
            newPart1B.Height = CalculateHeight(newPart1B, centerOfIntersect, newPart1B.Direction);
            newPart2A.Height = CalculateHeight(newPart2A, centerOfIntersect, newPart2A.Direction);
            newPart2B.Height = CalculateHeight(newPart2B, centerOfIntersect, newPart2B.Direction);

            // Обновляем ссылки на сегменты
            bubble1.Part1 = newPart1A;
            bubble1.Part2 = newPart1B;
            bubble2.Part1 = newPart2A;
            bubble2.Part2 = newPart2B;

            // Обновляем мембраны
            double membraneRadius = bubble1.MembraneRadius; // Можно использовать радиус одного из сегментов
            bubble1.Membrane = new Circle3D(centerOfIntersect, membraneRadius, bubble1.MembraneNormal);
            bubble2.Membrane = new Circle3D(centerOfIntersect, membraneRadius, bubble2.MembraneNormal);

            Console.WriteLine("Кластеры созданы: два пузыря, каждый из которых состоит из двух обрезанных сегментов с общей мембраной.");
        }

        private SphericalSegment CreateSegment(SphericalSegment originalSegment, Vector3D membraneNormal)
        {
            SphericalSegment newSegment = new SphericalSegment(originalSegment.Center, originalSegment.Height, membraneNormal);

            return newSegment;
        }
        public bool AreCoDirectional(Vector3D a, Vector3D b)
        {
            // Проверяем, что оба вектора не равны нулю
            if (a.X == 0 && a.Y == 0 && a.Z == 0 || b.X == 0 && b.Y == 0 && b.Z == 0)
            {
                return false; // Один из векторов является нулевым
            }

            // Проверяем, имеют ли компоненты одинаковые знаки
            return (a.X >= 0 && b.X >= 0 || a.X <= 0 && b.X <= 0) &&
                   (a.Y >= 0 && b.Y >= 0 || a.Y <= 0 && b.Y <= 0) &&
                   (a.Z >= 0 && b.Z >= 0 || a.Z <= 0 && b.Z <= 0);
        }
        private double CalculateHeight(SphericalSegment segment, Vector3D intersectionPoint, Vector3D direction)
        {
            Console.WriteLine($"intersectionPoint {intersectionPoint}, direction {direction}");

            Vector3D toIntersection = intersectionPoint - segment.Center;
            double distance = toIntersection.Length;
            toIntersection.Normalize();
            direction.Normalize();

            // Если векторы сонаправлены, уменьшаем высоту
            double height;
            if (AreCoDirectional(toIntersection, direction))
                height = distance;
            else
                height = segment.Height;

            Console.WriteLine($"height {height}");
            return height;
        }

        public static List<Obj> PositionBubbles(Bubble b1, Bubble b2, bool from_combined/*, ref int what_happend*/)
        {
            double contactAngle = ContactAngle(b1, b2);
            List<Obj> res = new List<Obj>();
            if (contactAngle.Equals(double.NaN))
                return null;
            if (contactAngle < 55.0) // слияние в один большой пузырь
            {
                //what_happend = 1;
                Bubble res_b = MergeBubbles(b1, b2);
                res.Add(res_b);
            }
            else if (contactAngle > 65.0) // отталкивание
            {
                List<Bubble> bubbles = PushBubblesApart(b1, b2, from_combined);
                res.Add(bubbles[0]);
                res.Add(bubbles[1]);
            }
            else // образование кластера
            {
                if (from_combined)
                {
                    MessageBox.Show(
                    $"Угол равен {contactAngle}. Невозможно образование кластера из трёх пузырей.",
                    "ERROR");
                    return null;
                }
                CombinedBubble cluster = new CombinedBubble(b1.Id, b1, b2);
                cluster.CreateClusters();
                //cluster.Bubble2.Id = cluster.Bubble1.Id; 
                res.Add(cluster);
            }
            return res;
        }

        public static int AmountOfContacts(Obj o1, Obj o2)
        {
            List<Vector3D> res1 = new List<Vector3D>();   
            List<Vector3D> res2 = new List<Vector3D>();
            int count = 0;
            if (o1 is CombinedBubble cb)
            {

                if (o2 is Bubble b)                    // кластер и пузырёк
                {
                    res1 = GetIntersectionPoint(cb, b);
                    if (res1 == null)
                    {
                        count = -1;
                        return count;
                    }
                    else if (res1[0].X != double.PositiveInfinity)
                        count++;
                }
                else if (o2 is CombinedBubble cb2)     // два кластера
                {
                    res1 = GetIntersectionPoint(cb, cb2.Bubble1);
                    res2 = GetIntersectionPoint(cb, cb2.Bubble2);
                    if (res1 == null || res2 == null) // если одна из частей одного кластера пересекается с другим кластером дважды
                    {
                        count = -1;
                        return count;
                    }
                    if (res1[0].X != double.PositiveInfinity || res2[0].X != double.PositiveInfinity) // если вообще есть пересечения
                    {
                        count = -1;
                        return count;
                    }
                }  
            }
            else if (o1 is Bubble b1)
            {
                if (o2 is Bubble b2)
                {
                    res1 = GetIntersectionPoint(b1, b2);
                    if (res1[0].X != double.PositiveInfinity)
                        count++;
                }
                else if (o2 is CombinedBubble cb2)
                {
                    res1 = GetIntersectionPoint(cb2, b1);
                    if (res1 == null)
                    {
                        count = -1;
                        return count;
                    }
                    else if (res1[0].X != double.PositiveInfinity)
                        count++;
                }
            }
            return count;                
        }

        public Bubble Bubble1 { get => bubble1; }
        public Bubble Bubble2 { get => bubble2; }

        public bool IsWithinCombinedBubble(Vector3D point)
        {
            return bubble1.IsWithinBubble(point) || bubble2.IsWithinBubble(point);
        }
    }
}
