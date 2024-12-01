using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            PositionBubbles();
        }

        public List<Vector3D> GetIntersectionPoint()
        {
            List<Vector3D> res = new List<Vector3D>();
            // Вычисляем расстояние между центрами сфер
            double distance = Math.Sqrt(
                             Math.Pow(bubble2.Center.X - bubble1.Center.X, 2) +
                             Math.Pow(bubble2.Center.Y - bubble1.Center.Y, 2) +
                             Math.Pow(bubble2.Center.Z - bubble1.Center.Z, 2));

            // Проверяем, пересекаются ли сферы
            if (distance > (bubble1.Radius + bubble2.Radius) || distance < Math.Abs(bubble1.Radius - bubble2.Radius)) 
            {
                res.Add(new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity));
                res.Add(new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity));
                return res;
            }
                
            // Находим точку P2, которая является центром окружности пересечения
            double a = (bubble1.Radius * bubble1.Radius - bubble2.Radius * bubble2.Radius + distance * distance) / (2 * distance);
            double h = (float)Math.Sqrt(bubble1.Radius * bubble1.Radius - a * a);

            // Находим точку P2
            Vector3D P2 = bubble1.Center + (bubble2.Center - bubble1.Center) * (a / distance);

            // Вектор перпендикулярный к вектору между центрами
            Vector3D direction = bubble2.Center - bubble1.Center;
            direction.Normalize();
            Vector3D perpendicular = Vector3D.CrossProduct(direction, new Vector3D(1, 0, 0));
            if (perpendicular.Length < 0.01f) // Если направление совпадает с осью X
            {
                perpendicular = Vector3D.CrossProduct(direction, new Vector3D(0, 1, 0));
            }
            perpendicular.Normalize();
            perpendicular = perpendicular * h;

            Vector3D point = P2 + perpendicular * (float)Math.Cos(0) + Vector3D.CrossProduct(direction, perpendicular) * (float)Math.Sin(0);
            
            res.Add(P2);
            res.Add(point);
            return res;
        }

        private double ContactAngle()
        {
            List<Vector3D> Points = GetIntersectionPoint();
            //Vector3D centerOfIntersect = Points[0];
            Vector3D commonPoint = Points[1];
            Console.WriteLine(commonPoint);
            // Векторы от центральной точки к боковым
            Vector3D vectorAB = bubble1.Center - commonPoint;
            Vector3D vectorAC = bubble2.Center - commonPoint;

            // Длина векторов
            double lengthAB = vectorAB.Length;
            double lengthAC = vectorAC.Length;

            // Скалярное произведение
            double dotProduct = Vector3D.DotProduct(vectorAB, vectorAC);

            // Вычисление косинуса угла
            double cosAngle = dotProduct / (lengthAB * lengthAC);

            // Ограничиваем значение косинуса для избежания ошибок округления
            cosAngle = Math.Max(-1, Math.Min(1, cosAngle));

            // Вычисляем угол в радианах и преобразуем в градусы
            double angleInRadians = Math.Acos(cosAngle);
            double angleInDegrees = angleInRadians * (180.0 / Math.PI);

            Console.WriteLine(angleInDegrees);
            return angleInDegrees;
        }
        private void MergeBubbles()
        {
            // Проверяем, что оба пузыря существуют
            if (bubble1 == null || bubble2 == null)
            {
                Console.WriteLine("Не удалось слить пузыри: один из пузырей отсутствует.");
                return;
            }

            // Вычисляем новый центр как среднее значение центров двух пузырей
            Vector3D newCenter = (bubble1.Center + bubble2.Center) / 2;

            // Вычисляем объёмы двух пузырей
            double volume1 = (4.0 / 3.0) * Math.PI * Math.Pow(bubble1.Radius, 3);
            double volume2 = (4.0 / 3.0) * Math.PI * Math.Pow(bubble2.Radius, 3);

            // Суммируем объёмы
            double totalVolume = volume1 + volume2;

            // Вычисляем новый радиус на основе общего объёма
            double newRadius = Math.Pow((totalVolume * 3.0) / (4.0 * Math.PI), 1.0 / 3.0);

            // Обновляем свойства первого пузыря
            bubble1.Center = newCenter;
            bubble1.Radius = newRadius;

            bubble2 = null;

            Console.WriteLine("Пузыри слиты в один большой пузырь.");
        }
        private void PushBubblesApart()
        {
            // Проверяем, что оба пузыря существуют
            if (bubble1 == null || bubble2 == null)
            {
                Console.WriteLine("Не удалось оттолкнуть пузыри: один из пузырей отсутствует.");
                return;
            }

            // Вычисляем вектор направления от bubble1 к bubble2
            Vector3D direction = bubble2.Center - bubble1.Center;

            // Вычисляем текущее расстояние между центрами пузырей
            double currentDistance = direction.Length;

            // Вычисляем необходимое расстояние между центрами пузырей
            double requiredDistance = bubble1.Radius + bubble2.Radius;

            // Если текущее расстояние меньше необходимого, отталкиваем пузыри
            if (currentDistance < requiredDistance)
            {
                // Нормализуем направление
                direction.Normalize();

                // Вычисляем, на сколько нужно оттолкнуть пузыри
                double pushDistance = requiredDistance - currentDistance;

                // Отталкиваем пузыри
                bubble1.Center -= direction * (pushDistance / 2); // Отталкиваем первый пузырь
                bubble2.Center += direction * (pushDistance / 2); // Отталкиваем второй пузырь

                Console.WriteLine("Пузыри оттолкнуты друг от друга.");
            }
            else
            {
                Console.WriteLine("Пузыри уже находятся на необходимом расстоянии друг от друга.");
            }
        }
        public void CreateClusters()
        {
            // Проверяем, что оба пузыря существуют
            if (bubble1 == null || bubble2 == null)
            {
                Console.WriteLine("Не удалось создать кластеры: один из пузырей отсутствует.");
                return;
            }

            // Находим точку пересечения
            List<Vector3D> Points = GetIntersectionPoint();
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
            // Создаем новый сегмент, копируя параметры из оригинального сегмента
            SphericalSegment newSegment = new SphericalSegment(originalSegment.Center, originalSegment.Height, membraneNormal);

            // Здесь можно добавить дополнительную логику для настройки сегмента
            // Например, если необходимо изменить радиус или высоту в зависимости от других условий

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
            // Расстояние от центра сегмента до точки пересечения
            Console.WriteLine($"intersectionPoint {intersectionPoint}, direction {direction}");

            // Вычисляем вектор от центра сегмента до точки пересечения
            Vector3D toIntersection = intersectionPoint - segment.Center;

            // Вычисляем длину этого вектора
            double distance = toIntersection.Length;

            // Определяем новую высоту на основе сонаправленности векторов
            toIntersection.Normalize();
            direction.Normalize();

            // Вычисляем скалярное произведение
            //double dotProduct = Vector3D.DotProduct(toIntersection, direction);

            // Если векторы сонаправлены, уменьшаем высоту
            double height;
            if (AreCoDirectional(toIntersection, direction)) // Векторы сонаправлены
            {
                height = distance; // Уменьшаем высоту на основе расстояния
            }
            else // Векторы не сонаправлены
            {
                height = segment.Height; // Возвращаем оригинальную высоту сегмента
            }

            Console.WriteLine($"height {height}");
            return height;
        }
        private void PositionBubbles()
        {
            double contactAngle = ContactAngle();
            if (contactAngle < 55.0) // слияние в один большой пузырь
                MergeBubbles();
            else if (contactAngle > 65.0) // отталкивание
                PushBubblesApart();
            else // образование класстера
                CreateClusters();
        }

        public Bubble Bubble1 { get => bubble1; }
        public Bubble Bubble2 { get => bubble2; }

        public bool IsWithinCombinedBubble(Vector3D point)
        {
            return bubble1.IsWithinBubble(point) || bubble2.IsWithinBubble(point);
        }
    }
}
