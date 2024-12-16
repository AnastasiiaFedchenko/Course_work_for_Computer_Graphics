using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.Reflection;

namespace Bubbles
{
    abstract public class Obj
    {
        protected int id;
        public Obj(int id)
        {
            this.id = id;
        }
        public int Id { get { return id; } set { id = value; } } 
    }
    public class Bubble : Obj
    {
        SphericalSegment part1;
        SphericalSegment part2;
        Circle3D membrane;

        private Vector3D center;     // Центр сегмента
        private double radius;        // Радиус сферы
        private Color color;          // Цвет
        private int specular;         // Спекулярность
        private double reflective;    // Отражательная способность

        public Bubble(int id, Vector3D center, double radius, Color color, int specular, double reflective) : base(id)
        {
            this.center = center;
            this.radius = radius;
            this.color = color;
            this.specular = specular;
            this.reflective = reflective;
            //this.transparency = transparency;
            //this.refractive_index = refractiveIndex;

            this.part1 = new SphericalSegment(center, radius, new Vector3D(-1, -1, 0));
            this.part2 = new SphericalSegment(center, radius, new Vector3D(1, 1, 0));

            // Задаем параметры круга
            Vector3D circleCenter = center + part2.Height * part2.Direction; // Центр круга на границе сегмента
            double circleRadius = Math.Sqrt(radius * radius - part2.Height * part2.Height); // Радиус круга
            Vector3D circleNormal = part2.Direction; // Нормаль к плоскости круга

            this.membrane = new Circle3D(circleCenter, circleRadius, circleNormal);
        }
        public SphericalSegment Part1 { get { return this.part1; } set { this.part1 = value; } }
        public SphericalSegment Part2 { get { return this.part2; } set { this.part2 = value; } }
        //public double Transparency { get { return transparency; } set {transparency = value; } }
        //public double RefractiveIndex { get {return refractive_index; } set {refractive_index = value; } }
        public Vector3D Center { get { return center; } set { center = value; part1.Center = value; part2.Center = value; } }
        public double Radius { get { return radius; } set { radius = value; part1.Height = radius; part2.Height = radius; } }
        
        public Color Color { get => color; set => color = value; }

        public Circle3D Membrane { get { return  membrane; } set { this.membrane = value; } }
        public Vector3D MembraneNormal { get { return membrane.Normal; } set { this.membrane.Normal = value; } }
        //public Vector3D MembraneCenter { get { return membrane.Center; } }
        public Double MembraneRadius { get { return membrane.Radius; } }

        public Color Color_mul(double n)
        {
            return Color.FromArgb((byte)Math.Min(255, (this.color.R * n)),
                                  (byte)Math.Min(255, (this.color.G * n)),
                                  (byte)Math.Min(255, (this.color.B * n)));
        }

        /*public Color Color_add(Color temp)
        {
            return Color.FromArgb((byte)Math.Min(255, this.color.R + temp.R),
                                  (byte)Math.Min(255, this.color.G + temp.G),
                                  (byte)Math.Min(255, this.color.B + temp.B));
        }*/

        public int Specular { get => specular; set => specular = value; }
        public double Reflective { get => reflective; set => reflective = value; }

        public bool IsWithinBubble(Vector3D point) 
        {
            return part1.IsWithinSegment(point) || part2.IsWithinSegment(point) || membrane.IsWithinCircle(point);
        }
        
    }
    public class SphericalSegment
    {
        private Vector3D center;     // Центр сегмента
        private double height;        // Высота сегмента
        private Vector3D direction;   // Направление сегмента
        internal SphericalSegment(Vector3D center, double height, Vector3D direction)
        {
            this.center = center;
            this.height = height;
            this.direction = direction;
            this.direction.Normalize();
        }
        public Vector3D Center { get => center; set => center = value; }
        public double Height { get => height; set => height = value; }
        public Vector3D Direction { get { return direction; } set { direction = value; } }
        internal bool IsWithinSegment(Vector3D point) 
        {
            direction.Normalize();

            double heightAtPoint = Vector3D.DotProduct(point - this.center, direction);

            return heightAtPoint >= 0 && heightAtPoint <= height;
        }
    }
    public class Circle3D
    {
        private Vector3D center;
        private double radius;
        private Vector3D normal; // Нормаль к плоскости круга

        internal Circle3D(Vector3D center, double radius, Vector3D normal)
        {
            this.center = center;
            this.radius = radius;
            normal.Normalize();
            this.normal = normal; // Нормализуем нормаль
        }
        public Vector3D Normal { get { return normal; } set { this.normal = value; } }
        public double Radius { get { return radius; } }
        public Vector3D Center { get {  return center; } }

        // Метод для проверки, находится ли точка внутри круга
        public bool IsWithinCircle(Vector3D point)
        {
            // Вычисляем вектор от центра круга до точки
            Vector3D toPoint = point - center;

            // Проверяем, перпендикулярен ли вектор нормали
            double dotProduct = Vector3D.DotProduct(toPoint, normal);
            if (Math.Abs(dotProduct) > 0.001f) // Проверка на близость к нулю
            {
                return false; // Точка не лежит в плоскости круга
            }

            // Проверяем, находится ли точка внутри радиуса
            return toPoint.LengthSquared <= radius * radius;
        }
    }
}
