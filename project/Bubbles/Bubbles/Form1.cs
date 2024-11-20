using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
//using static System.Collections.Specialized.BitVector32;

namespace Bubbles
{
    public partial class Form1 : Form
    {
        const double EPSILON = 0.001;

        Renderer drawer;
        public Form1()
        {
            InitializeComponent();
            drawer = new Renderer(4, 1, new Vector3D(0, 0, 0), 2, Canvas.Width, Canvas.Height);
            
            Canvas.Image = drawer.Canvas_Buffer;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            drawer.AddSphere(new Sphere(new Vector3D(0, -1, 3), 2, System.Drawing.Color.Orange, 32, 0.1, 0.7, 1)); // Полупрозрачный оранжевый
            drawer.AddSphere(new Sphere(new Vector3D(-1, -1, 3), 2, System.Drawing.Color.Black, 32, 0.1, 0.7, 1)); // Полупрозрачный красный
            drawer.AddSphere(new Sphere(new Vector3D(1, -1, 3), 2, System.Drawing.Color.Red, 32, 0.1, 0.7, 1)); // Полупрозрачный красный
            //drawer.AddSphere(new Sphere(new Vector3D(0, -1, 3), 2, System.Drawing.Color.FromArgb(128, 255, 165, 0), 80, 0.7));
            //drawer.AddSphere(new Sphere(new Vector3D(0, -1, 3), 1.8, System.Drawing.Color.Yellow, 80, 0.7)); // Внутренняя стенка
            //drawer.AddSphere(new Sphere(new Vector3D(-1, -1, 3), 2, System.Drawing.Color.FromArgb(128, 255, 0, 0), 80, 0.7));
            //drawer.AddSphere(new Sphere(new Vector3D(-2, 0, 4), 1, System.Drawing.Color.Purple, 10, 0.1));
            //drawer.AddSphere(new Sphere(new Vector3D(2, 0, 4), 1, System.Drawing.Color.Blue, 500, 0.3));
            //drawer.AddSphere(new Sphere(new Vector3D(0, -5001, 0), 5000, System.Drawing.Color.Yellow, 1000, 0.5));

            drawer.AddLight(new Light(Light.light_type.AMBIENT, 0.1, new Vector3D(0, 0, 0)));
            drawer.AddLight(new Light(Light.light_type.POINT, 0.8, new Vector3D(2, 1, 0)));
            drawer.AddLight(new Light(Light.light_type.DIRECTIONAL, 0.4, new Vector3D(1, 4, 4)));

            drawer.Render();
            Canvas.Invalidate();
            
        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                ColorButton.BackColor = colorDialog1.Color;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            double ox, oy, oz;
            double r;
            if (double.TryParse(OxTextBox.Text, out ox) == false)
            {
                MessageBox.Show("Ox координата центра пузыря должна быть числом с плавающей точкой.");
                return;
            }
            if (double.TryParse(OyTextBox.Text, out oy) == false)
            {
                MessageBox.Show("Oy координата центра пузыря должна быть числом с плавающей точкой.");
                return;
            }
            if (double.TryParse(OzTextBox.Text, out oz) == false)
            {
                MessageBox.Show("Oz координата центра пузыря должна быть числом с плавающей точкой.");
                return;
            }
            if (double.TryParse(RTextBox.Text, out r) == false)
            {
                MessageBox.Show("Радиус пузыря должен быть числом с плавающей точкой.");
                return;
            }
            drawer.AddSphere(new Sphere(new Vector3D(ox, oy, oz), r, ColorButton.BackColor, 500, 0.5, 0.7, 1.5));
            drawer.Render();
            Canvas.Invalidate();
        }

        Vector3D ReflectRay(Vector3D a, Vector3D b)
        {
            return b * (2 * dot(a, b)) - a;
        }

        private void ClearSceneButton_Click(object sender, EventArgs e)
        {
            drawer = new Renderer(1, 1, new Vector3D(0, 0, 0), 3, Canvas.Width, Canvas.Height);
            drawer.AddLight(new Light(Light.light_type.AMBIENT, 0.2, new Vector3D(0, 0, 0)));
            drawer.AddLight(new Light(Light.light_type.POINT, 0.6, new Vector3D(2, 1, 0)));
            drawer.AddLight(new Light(Light.light_type.DIRECTIONAL, 0.2, new Vector3D(1, 4, 4)));
            Canvas.Image = drawer.Canvas_Buffer;

            ColorButton.BackColor = System.Drawing.Color.Magenta;

            OxTextBox.Text = "0";
            OyTextBox.Text = "0";
            OzTextBox.Text = "0";
            RTextBox.Text = "1";

            drawer.Clean();
            Canvas.Invalidate();
        }
        private double dot(Vector3D a, Vector3D b) { return (double)(a.X * b.X + a.Y * b.Y + a.Z * b.Z); }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            drawer.ViewportSize = trackBar1.Value;
            drawer.Render();
            Canvas.Invalidate();
        }
    }
    

}
