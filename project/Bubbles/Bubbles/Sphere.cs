using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace Bubbles
{
    public class Sphere
    {
        Vector3D center;     // центр 
        double radius;     // радиус
        Color color;       // цвет
        int specular;  
        double reflective; 

        public Sphere(Vector3D center, double radius, Color color, int specular, double reflective)
        {
            Center = center;
            this.radius = radius;
            this.color = color;
            this.specular = specular;
            this.reflective = reflective;
        }
        public Vector3D Center { get => center; set => center = value; }
        public double Radius { get => radius; set => radius = value; }
        public Color Color { get => color; set => color = value; }
        public Color Color_mul(double n) {
            return Color.FromArgb((int)((this.color.A * n) % 256), (int)((this.color.R * n) % 256), 
                                  (int)((this.color.G * n) % 256), (int)((this.color.B * n) % 256));
        } 
        public Color Color_add(Color temp)
        {
            return Color.FromArgb(this.color.A + temp.A, this.color.R + temp.R, 
                                  this.color.G + temp.G, this.color.B + temp.B);
        }
        public int Specular { get => specular; set => specular = value; }
        public double Reflective { get => reflective; set => reflective = value; }
    }
}
