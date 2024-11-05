using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bubbles
{
    public class Renderer
    {
        const double EPSILON = 0.001;

        int viewport_size;
        int projection_plane_z;
        Vector3D camera_position;
        List<Sphere> spheres;
        List<Light> lights;
        int recursion_depth;

        Bitmap canvas_buffer;
        Graphics g;
        System.Drawing.Color background_color = System.Drawing.Color.Black;

        public Renderer(int viewport_size, int projection_plane_z, Vector3D camera_position, int recursive_depth, int canvas_width, int canvas_height) 
        { 
            this.viewport_size = viewport_size;
            this.projection_plane_z = projection_plane_z;
            this.camera_position = camera_position;
            spheres = new List<Sphere>();
            lights = new List<Light>();
            this.recursion_depth = recursive_depth;

            canvas_buffer = new Bitmap(canvas_width, canvas_height);
            g = Graphics.FromImage(canvas_buffer);

        }
        public Bitmap Canvas_Buffer { get { return canvas_buffer; } }
        public void AddLight(Light l) { lights.Add(l); }
        public void AddSphere(Sphere s) { spheres.Add(s); }

        public void Clean() {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Black),
                0, 0, canvas_buffer.Width, canvas_buffer.Height);
        }
        public void Render()
        {
            g.FillRectangle(new SolidBrush(Form1.DefaultBackColor),
                0, 0, canvas_buffer.Width, canvas_buffer.Height);

            for (double x = -canvas_buffer.Width / 2; x < canvas_buffer.Width / 2; x++)
            {
                for (double y = -canvas_buffer.Height / 2; y < canvas_buffer.Height / 2; y++)
                {
                    Vector3D direction = CanvasToViewport(x, y);
                    System.Drawing.Color color = TraceRay(camera_position, direction, 1, double.PositiveInfinity, recursion_depth);
                    if (color != System.Drawing.Color.Black)
                        PutPixel((int)x, (int)y, color);
                }
            }
        }

        Vector3D ReflectRay(Vector3D a, Vector3D b) { return b * (2 * dot(a, b)) - a; }
        double dot(Vector3D a, Vector3D b) { return (double)(a.X * b.X + a.Y * b.Y + a.Z * b.Z); }

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

        void PutPixel(int x, int y, System.Drawing.Color color)
        {
            x = canvas_buffer.Width / 2 + x;
            y = canvas_buffer.Height / 2 - y - 1;

            if (x < 0 || x >= canvas_buffer.Width || y < 0 || y >= canvas_buffer.Height) { return; }

            canvas_buffer.SetPixel(x, y, color);
            if (color != System.Drawing.Color.Black)
                x = 0;
        }

        Vector3D CanvasToViewport(double x, double y)
        {
            return new Vector3D(
              x * viewport_size / canvas_buffer.Width,
              y * viewport_size / canvas_buffer.Height,
              projection_plane_z
            );
        }

        double ComputeLighting(Vector3D point, Vector3D normal, Vector3D view, int specular)
        {
            double intensity = 0;
            double length_n = normal.Length;  // Should be 1.0, but just in case...
            double length_v = view.Length;

            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
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

            for (int i = 0; i < spheres.Count; i++)
            {
                List<double> ts = IntersectRaySphere(origin, direction, spheres[i]);
                if (ts[0] < closest_t && min_t < ts[0] && ts[0] < max_t)
                {
                    closest_t = ts[0];
                    closest_sphere = spheres[i];
                }
                if (ts[1] < closest_t && min_t < ts[1] && ts[1] < max_t)
                {
                    closest_t = ts[1];
                    closest_sphere = spheres[i];
                }
            }

            if (closest_sphere != null)
            {
                return new KeyValuePair<Sphere, double>(closest_sphere, closest_t);
            }
            return new KeyValuePair<Sphere, double>(null, 0);
        }
    }
}
