using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Bubbles
{
    public class Renderer
    {
        struct TraceRayArgs
        {
            public int x;
            public int y;
            public Vector3D origin;
            public Vector3D direction;
            public double min_t;
            public double max_t;
            public int depth;
            public TraceRayArgs(int x, int y, Vector3D origin, Vector3D direction, double min_t, double max_t, int depth) 
            {
                this.x = x;
                this.y = y; 
                this.origin = origin;
                this.direction = direction;
                this.min_t = min_t;
                this.max_t = max_t;
                this.depth = depth;
            }
        }

        struct RenderPieceArgs
        {
            public int x_start;
            public int x_finish;
            public int y_start; 
            public int y_finish;
            public int w;
            public int h;
            public bool DO_LOG;

            public RenderPieceArgs(int x_start, int x_finish, int y_start, int y_finish, int w, int h) 
            {
                this.x_start = x_start;
                this.x_finish = x_finish;
                this.y_start = y_start;
                this.y_finish = y_finish;
                this.w = w;
                this.h = h;

                DO_LOG = false;
            }
            public RenderPieceArgs(int x_start, int x_finish, int y_start, int y_finish, int w, int h, bool do_log)
            {
                this.x_start = x_start;
                this.x_finish = x_finish;
                this.y_start = y_start;
                this.y_finish = y_finish;
                this.w = w;
                this.h = h;

                this.DO_LOG = do_log;
            }

        }

        const double EPSILON = 0.001;

        int viewport_size;
        int d;
        Vector3D camera_position;
        List<Sphere> spheres;
        List<Light> lights;
        int recursion_depth;

        Bitmap canvas_buffer;
        Graphics g;
        System.Drawing.Color background_color = System.Drawing.Color.FromArgb(255, 220, 240, 255);

        private readonly object locker;
        private readonly object log_locker;

        public Renderer(int viewport_size, int d, Vector3D camera_position, int recursive_depth, int canvas_width, int canvas_height) 
        { 
            this.viewport_size = viewport_size;
            this.d = d;
            this.camera_position = camera_position;
            spheres = new List<Sphere>();
            lights = new List<Light>();
            this.recursion_depth = recursive_depth;

            canvas_buffer = new Bitmap(canvas_width, canvas_height);
            g = Graphics.FromImage(canvas_buffer);

            locker = new object();
            log_locker = new object();
        }
        public Bitmap Canvas_Buffer { get { return canvas_buffer; } }
        public void AddLight(Light l) { lights.Add(l); }
        public void AddSphere(Sphere s) { spheres.Add(s); }

        public int ViewportSize { get { return viewport_size; }  set { viewport_size = value; } }

        public void Clean() {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Black),
                0, 0, canvas_buffer.Width, canvas_buffer.Height);
        }
        public void Render()
        {
            g.FillRectangle(new SolidBrush(Form1.DefaultBackColor),
                0, 0, canvas_buffer.Width, canvas_buffer.Height);

            int w = canvas_buffer.Width / 2;
            int h = canvas_buffer.Height / 2;

            for (double x = -w; x < w; x++)
            {
                for (double y = -h; y < h; y++)
                {
                    Vector3D direction = CanvasToViewport(w, h, x, y);
                    System.Drawing.Color color = TraceRay(new TraceRayArgs((int)x, (int)y, camera_position, direction, 1, double.PositiveInfinity, recursion_depth));
                    PutPixel((int)x, (int)y, color, false);
                }
            }
        }

        Vector3D ReflectRay(Vector3D a, Vector3D b) { return b * (2 * dot(a, b)) - a; }
        Vector3D RefractRay(Vector3D direction, Vector3D normal, double refractiveIndex)
        {
            double n1 = 1.0; // Показатель преломления воздуха
            double n2 = refractiveIndex; // Показатель преломления материала
            double cosI = -dot(normal, direction);

            // Вычисление синуса угла преломления
            double sin2T = (n1 / n2) * (n1 / n2) * (1 - cosI * cosI);

            // Проверка на полное внутреннее отражение
            if (sin2T > 1.0) return new Vector3D(0, 0, 0); // Возвращаем нулевой вектор, если полное внутреннее отражение

            double cosT = Math.Sqrt(1.0 - sin2T);

            // Используем формулу для расчета преломленного луча
            return (direction * (n1 / n2)) + (normal * (n1 / n2 * cosI - cosT));
        }
        double dot(Vector3D a, Vector3D b) { return (double)(a.X * b.X + a.Y * b.Y + a.Z * b.Z); }

        public delegate System.Drawing.Color ThreadTraceRay(Vector3D origin, Vector3D direction, double min_t, double max_t, int depth);
        System.Drawing.Color TraceRay(Object obj)
        {
            if (obj is TraceRayArgs args)
            {
                if (args.depth <= 0)
                {
                    return background_color; // Возвращаем фон, если достигли максимальной глубины рекурсии
                }
                List<KeyValuePair<Sphere, double>> intersection = ClosestIntersection(args.origin, args.direction, args.min_t, args.max_t);
                if (intersection[0].Key == null)
                    return background_color;

                Sphere closest_sphere = intersection[0].Key;
                double closest_t = intersection[0].Value;

                // Проверка на наличие второй сферы
                Sphere closest_sphere2 = intersection.Count > 1 ? intersection[1].Key : null;
                double closest_t2 = intersection.Count > 1 ? intersection[1].Value : double.MaxValue;

                Vector3D point = args.origin + args.direction * closest_t;
                Vector3D normal = point - closest_sphere.Center;
                normal = normal * (1.0 / normal.Length);

                Vector3D view = args.direction * (-1);
                double lighting = ComputeLighting(point, normal, view, closest_sphere.Specular);
                System.Drawing.Color local_color = closest_sphere.Color_mul(lighting);

                // Обработка отражения
                System.Drawing.Color reflected_color = System.Drawing.Color.Black;
                if (closest_sphere.Reflective > 0)
                {
                    Vector3D reflected_ray = ReflectRay(view, normal);
                    reflected_color = TraceRay(new TraceRayArgs(args.x, args.y, point, reflected_ray, EPSILON,
                                                                double.PositiveInfinity, args.depth - 1));
                    reflected_color = System.Drawing.Color.FromArgb(
                        (int)(reflected_color.A * closest_sphere.Reflective),
                        (int)(reflected_color.R * closest_sphere.Reflective),
                        (int)(reflected_color.G * closest_sphere.Reflective),
                        (int)(reflected_color.B * closest_sphere.Reflective)
                    );
                }

                // Обработка преломления
                System.Drawing.Color refracted_color = System.Drawing.Color.Black;
                if (closest_sphere.Transparency > 0)
                {
                    Vector3D refracted_ray = RefractRay(view, normal, closest_sphere.RefractiveIndex);
                    refracted_color = TraceRay(new TraceRayArgs(args.x, args.y, point, refracted_ray, EPSILON,
                                                                double.PositiveInfinity, args.depth - 1));
                    refracted_color = System.Drawing.Color.FromArgb(
                        (int)(refracted_color.A * closest_sphere.Transparency),
                        (int)(refracted_color.R * closest_sphere.Transparency),
                        (int)(refracted_color.G * closest_sphere.Transparency),
                        (int)(refracted_color.B * closest_sphere.Transparency)
                    );
                }

                // Учитываем цвет второй сферы, если она существует и находится "за" первой
                System.Drawing.Color second_sphere_color = System.Drawing.Color.Black;
                if (closest_sphere2 != null && closest_t2 < double.MaxValue)
                {
                    // Вычисляем цвет второй сферы
                    Vector3D point2 = args.origin + args.direction * closest_t2;
                    Vector3D normal2 = point2 - closest_sphere2.Center;
                    normal2 = normal2 * (1.0 / normal2.Length);
                    double lighting2 = ComputeLighting(point2, normal2, view, closest_sphere2.Specular);
                    second_sphere_color = closest_sphere2.Color_mul(lighting2);
                    second_sphere_color = System.Drawing.Color.FromArgb(
                        (int)(second_sphere_color.A * closest_sphere2.Transparency),
                        (int)(second_sphere_color.R * closest_sphere2.Transparency),
                        (int)(second_sphere_color.G * closest_sphere2.Transparency),
                        (int)(second_sphere_color.B * closest_sphere2.Transparency)
                    );
                }
                // Смешивание цветов
                System.Drawing.Color result_color = System.Drawing.Color.FromArgb(
                    Math.Min(255, local_color.A + reflected_color.A + refracted_color.A + second_sphere_color.A),
                    Math.Min(255, local_color.R + reflected_color.R + refracted_color.R + second_sphere_color.R),
                    Math.Min(255, local_color.G + reflected_color.G + refracted_color.G + second_sphere_color.G),
                    Math.Min(255, local_color.B + reflected_color.B + refracted_color.B + second_sphere_color.B)
                );

                // Возвращаем окончательный цвет
                return result_color;
            }
            return background_color;
        }

        void PutPixel(int x, int y, System.Drawing.Color color, /*ref Mutex mut, ref Mutex mutlog,*/ bool dolog)
        {
            int W, H;

            lock (locker)
            {
                W = canvas_buffer.Width;
                H = canvas_buffer.Height;
            }

            x = W / 2 + x;
            y = H / 2 - y - 1;

            if (x < 0 || x >= W || y < 0 || y >= H)
            {
                if (dolog)
                {
                    lock (log_locker)
                    {
                        Console.WriteLine("{0} problem with x and y; x = {1} y = {2}, W = {3}, H = {4}", Thread.CurrentThread.Name, x, y, W, H);
                    }
                }
                return;
            }

            if (dolog)
            {
                lock (log_locker)
                {
                    Console.WriteLine("{0} is requesting the mutex", Thread.CurrentThread.Name);
                    Console.WriteLine("{0} has entered the protected area", Thread.CurrentThread.Name);
                }
            }
            lock (locker)
            {
                canvas_buffer.SetPixel(x, y, color);
            }
            if (dolog)
            {
                lock (log_locker)
                {
                    Console.WriteLine("{0} has released the mutex", Thread.CurrentThread.Name);
                }
            }
        }

        Vector3D CanvasToViewport(double w, double h, double x, double y)
        {
            return new Vector3D(
              x * viewport_size / (2 * w),
              y * viewport_size / (2 * h),
              d
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
                KeyValuePair<Sphere, double> blocker = ClosestIntersection(point, vec_l, EPSILON, t_max)[0];
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
                double t1 = (-k2 + Math.Sqrt(discriminant)) / (2 * k1);
                double t2 = (-k2 - Math.Sqrt(discriminant)) / (2 * k1);
                res.Add(t1);
                res.Add(t2);
            }

            return res;
        }


        List<KeyValuePair<Sphere, double>> ClosestIntersection(Vector3D origin, Vector3D direction,
                                                 double min_t, double max_t)
        {
            double closest_t = double.PositiveInfinity;
            Sphere closest_sphere = null;

            double closest_t2 = double.PositiveInfinity;
            Sphere closest_sphere2 = null;

            for (int i = 0; i < spheres.Count; i++)
            {
                List<double> ts = IntersectRaySphere(origin, direction, spheres[i]);

                // Проверка пересечений
                foreach (double t in ts)
                {
                    if (min_t < t && t < max_t) // Проверка диапазона
                    {
                        if (t < closest_t)
                        {
                            // Обновляем ближайшую сферу
                            closest_t2 = closest_t; // Сохраняем предыдущее ближайшее
                            closest_sphere2 = closest_sphere; // Сохраняем предыдущее ближайшее
                            closest_t = t;
                            closest_sphere = spheres[i];
                        }
                        else if (t < closest_t2)
                        {
                            // Обновляем вторую ближайшую сферу
                            closest_t2 = t;
                            closest_sphere2 = spheres[i];
                        }
                    }
                }
            }

            List<KeyValuePair<Sphere, double>> res = new List<KeyValuePair<Sphere, double>>();
            res.Add(new KeyValuePair<Sphere, double>(closest_sphere, closest_t));
            res.Add(new KeyValuePair<Sphere, double>(closest_sphere2, closest_t2));
            return res;
        }
    }
}
