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
using static System.Collections.Specialized.BitVector32;

namespace Bubbles
{
    public partial class Form1 : Form
    {
        const double EPSILON = 0.001;

        int ViewportSize;
        int ProjectionPlaneZ;
        Vector3D CameraPosition;
        List<Sphere> Spheres;
        List<Light> Lights;
        int RecursionDepth;

        Bitmap canvas_buffer;
        Graphics g;
        System.Drawing.Color background_color = System.Drawing.Color.Black;
        public Form1()
        {
            InitializeComponent();
            
            ViewportSize = 1;
            ProjectionPlaneZ = 1;
            CameraPosition = new Vector3D(0, 0, 0);
            Spheres = new List<Sphere>();
            Lights = new List<Light>();
            RecursionDepth = 3;

            canvas_buffer = new Bitmap(Canvas.Width, Canvas.Height);
            g = Graphics.FromImage(canvas_buffer);
            Canvas.Image = canvas_buffer;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*Spheres.Add(new Sphere(new Vector3D(0, -1, 3), 1, System.Drawing.Color.Red, 500, 0.2));
            Spheres.Add(new Sphere(new Vector3D(-2, 0, 4), 1, System.Drawing.Color.Green, 10, 0.4));
            Spheres.Add(new Sphere(new Vector3D(2, 0, 4), 1, System.Drawing.Color.Blue, 500, 0.3));
            Spheres.Add(new Sphere(new Vector3D(0, -5001, 0), 5000, System.Drawing.Color.Yellow, 1000, 0.5));*/

            Lights.Add(new Light(Light.light_type.AMBIENT, 0.2, new Vector3D(0, 0, 0)));
            Lights.Add(new Light(Light.light_type.POINT, 0.6, new Vector3D(2, 1, 0)));
            Lights.Add(new Light(Light.light_type.DIRECTIONAL, 0.2, new Vector3D(1, 4, 4)));

            Render();
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
                MessageBox.Show("Problem with xlh. The value should be integer");
                return;
            }
            if (double.TryParse(OyTextBox.Text, out oy) == false)
            {
                MessageBox.Show("Problem with xlh. The value should be integer");
                return;
            }
            if (double.TryParse(OzTextBox.Text, out oz) == false)
            {
                MessageBox.Show("Problem with xlh. The value should be integer");
                return;
            }
            if (double.TryParse(RTextBox.Text, out r) == false)
            {
                MessageBox.Show("Problem with xlh. The value should be integer");
                return;
            }
            Spheres.Add(new Sphere(new Vector3D(ox, oy, oz), r, System.Drawing.Color.Red, 500, 0.2));
            Render();
            Canvas.Invalidate();
        }

        double ComputeLighting(Vector3D point, Vector3D normal, Vector3D view, int specular)
        {
            double intensity = 0;
            double length_n = normal.Length;  // Should be 1.0, but just in case...
            double length_v = view.Length;

            for (int i = 0; i < Lights.Count; i++)
            {
                Light light = Lights[i];
                if (light.Ltype == Light.light_type.AMBIENT)
                {
                    intensity += light.Intensity;
                    continue;
                }

                Vector3D vec_l;
                double t_max;
                if (light.Ltype == Light.light_type.POINT)
                {
                    vec_l = light.Position - point;
                    t_max = 1.0;
                }
                else
                {  // Light.DIRECTIONAL
                    vec_l = light.Position;
                    t_max = double.PositiveInfinity;
                }

                // Shadow check.
                KeyValuePair<Sphere, double> blocker = ClosestIntersection(point, vec_l, EPSILON, t_max);
                if (blocker.Key != null)
                {
                    continue;
                }

                // Diffuse reflection.
                double n_dot_l = dot(normal, vec_l);
                if (n_dot_l > 0)
                {
                    intensity += light.Intensity * n_dot_l / (length_n * vec_l.Length);
                }

                // Specular reflection.
                if (specular != -1)
                {
                    Vector3D vec_r = (normal * 2.0 * n_dot_l) - vec_l;
                    double r_dot_v = dot(vec_r, view);
                    if (r_dot_v > 0)
                    {
                        intensity += light.Intensity * Math.Pow(r_dot_v / (vec_r.Length * length_v), specular);
                    }
                }
            }

            return intensity;
        }

        List<double> IntersectRaySphere(Vector3D origin, Vector3D direction, Sphere sphere)
        {
            Vector3D oc = origin - sphere.Center;

            double k1 = dot(direction, direction);
            double k2 = 2 * dot(oc, direction);
            double k3 = dot(oc, oc) - sphere.Radius * sphere.Radius;

            double discriminant = k2 * k2 - 4 * k1 * k3;
            List<double> res = new List<double>();
            if (discriminant < 0)
            {
                res.Add(double.PositiveInfinity);
                res.Add(double.PositiveInfinity);
            }
            else
            {
                res.Add((-k2 + Math.Sqrt(discriminant)) / (2 * k1));
                res.Add((-k2 - Math.Sqrt(discriminant)) / (2 * k1));
            }
            return res;
        }


        KeyValuePair<Sphere, double> ClosestIntersection(Vector3D origin, Vector3D direction,
                                                         double min_t, double max_t)
        {
            double closest_t = double.PositiveInfinity;
            Sphere closest_sphere = null;

            for (int i = 0; i < Spheres.Count; i++)
            {
                List<double> ts = IntersectRaySphere(origin, direction, Spheres[i]);
                if (ts[0] < closest_t && min_t < ts[0] && ts[0] < max_t)
                {
                    closest_t = ts[0];
                    closest_sphere = Spheres[i];
                }
                if (ts[1] < closest_t && min_t < ts[1] && ts[1] < max_t)
                {
                    closest_t = ts[1];
                    closest_sphere = Spheres[i];
                }
            }

            if (closest_sphere != null)
            {
                return new KeyValuePair<Sphere, double>(closest_sphere, closest_t);
            }
            return new KeyValuePair<Sphere, double>(null, 0);
        }

        Vector3D ReflectRay(Vector3D a, Vector3D b)
        {
            return b * (2 * dot(a, b)) - a;
        }
        System.Drawing.Color TraceRay(Vector3D origin, Vector3D direction, double min_t, double max_t, int depth)
        {
            KeyValuePair<Sphere, double> intersection = ClosestIntersection(origin, direction, min_t, max_t);
            if (intersection.Key == null)
            {
                return background_color;
            }

            Sphere closest_sphere = intersection.Key;
            double closest_t = intersection.Value;

            Vector3D point = origin + direction * closest_t;
            Vector3D normal = point - closest_sphere.Center;
            normal = normal * (1.0 / normal.Length);

            Vector3D view = direction * (-1);
            double lighting = ComputeLighting(point, normal, view, closest_sphere.Specular);
            System.Drawing.Color local_color = closest_sphere.Color_mul(lighting);

            if (closest_sphere.Reflective <= 0 || depth <= 0)
            {
                return local_color;
            }

            Vector3D reflected_ray = ReflectRay(view, normal);
            System.Drawing.Color reflected_color = TraceRay(point, reflected_ray, EPSILON,
                                                            double.PositiveInfinity, depth - 1);

            double k = (1 - closest_sphere.Reflective);
            System.Drawing.Color local_con = System.Drawing.Color.FromArgb((int)(local_color.A * k) % 256, 
                                                                (int)(local_color.R * k) % 256,
                                                                (int)(local_color.G * k) % 256,
                                                                (int)(local_color.B * k) % 256);
            double r = closest_sphere.Reflective;
            System.Drawing.Color reflected_con = System.Drawing.Color.FromArgb((int)(reflected_color.A * r),
                                                                (int)(reflected_color.R * r) % 256,
                                                                (int)(reflected_color.G * r) % 256,
                                                                (int)(reflected_color.B * r) % 256);
            return System.Drawing.Color.FromArgb((local_con.A + reflected_con.A) % 256,
                                                 (local_con.R + reflected_con.R) % 256,
                                                 (local_con.G + reflected_con.G) % 256,
                                                 (local_con.B + reflected_con.B) % 256);
        }

        Vector3D CanvasToViewport(double x, double y)
        {
            return new Vector3D(
              x * ViewportSize / Canvas.Width,
              y * ViewportSize / Canvas.Height,
              ProjectionPlaneZ
            );
        }

        void PutPixel(int x, int y, System.Drawing.Color color)
        {
            x = Canvas.Width / 2 + x;
            y = Canvas.Height / 2 - y - 1;

            if (x < 0 || x >= Canvas.Width || y < 0 || y >= Canvas.Height) { return; }

            canvas_buffer.SetPixel(x, y, color);
            if (color != System.Drawing.Color.Black)
                x = 0;
        }

        private void Render()
        {
            g.FillRectangle(new SolidBrush(Form1.DefaultBackColor),
                0, 0, Canvas.Width, Canvas.Height);

            for (double x = -Canvas.Width / 2; x < Canvas.Width / 2; x++)
            {
                for (double y = -Canvas.Height / 2; y < Canvas.Height / 2; y++)
                {
                    Vector3D direction = CanvasToViewport(x, y);
                    System.Drawing.Color color = TraceRay(CameraPosition, direction, 1, double.PositiveInfinity, RecursionDepth);
                    if (color != System.Drawing.Color.Black) 
                        PutPixel((int)x, (int)y, color);
                }
            }
        }

        private void ClearSceneButton_Click(object sender, EventArgs e)
        {
            ViewportSize = 1;
            ProjectionPlaneZ = 1;
            CameraPosition = new Vector3D(0, 0, 0);
            Spheres = new List<Sphere>();
            RecursionDepth = 3;

            ColorButton.BackColor = System.Drawing.Color.Magenta;

            OxTextBox.Text = "0";
            OyTextBox.Text = "0";
            OzTextBox.Text = "0";
            RTextBox.Text = "1";

            g.FillRectangle(new SolidBrush(System.Drawing.Color.Black),
                0, 0, Canvas.Width, Canvas.Height);
            Canvas.Invalidate();
        }
        private double dot(Vector3D a, Vector3D b) { return (double)(a.X * b.X + a.Y * b.Y + a.Z * b.Z); }
    }
    

}
