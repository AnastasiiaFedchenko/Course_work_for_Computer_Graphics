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
        int projection_plane_z;
        Vector3D camera_position;
        List<Sphere> spheres;
        List<Light> lights;
        int recursion_depth;

        Bitmap canvas_buffer;
        Bitmap tmp;
        Graphics g;
        Graphics g_tmp;
        System.Drawing.Color background_color = System.Drawing.Color.Black;

        private readonly object locker;
        private readonly object log_locker;

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

            locker = new object();
            log_locker = new object();
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

            double w = canvas_buffer.Width / 2;
            double h = canvas_buffer.Height / 2;
            int N = 2;
            Thread[] p = new Thread[N];
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

        public void MeasureTime() 
        {
            g.FillRectangle(new SolidBrush(Form1.DefaultBackColor),
                0, 0, canvas_buffer.Width, canvas_buffer.Height);

            double w = canvas_buffer.Width / 2;
            double h = canvas_buffer.Height / 2;


            tmp = new Bitmap(canvas_buffer.Width, canvas_buffer.Height);
            g = Graphics.FromImage(tmp);

            int N = 2;
            while (N <= (4 * 16)) 
            {
                double sum = 0;
                for (int k = 0; k < 10; k++)
                {
                    Console.WriteLine($"{N}");
                    Thread[] p = new Thread[N];

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    for (int i = 0; i < N; i++)
                    {
                        p[i] = new Thread(new ParameterizedThreadStart(RenderPiece));
                        p[i].Name = String.Format("Thread{0}", i + 1);
                    }
                    int cur_x_s = (int)-w;
                    int step = (int)(2 * w) / N;
                    for (int i = 0; i < N; i++, cur_x_s += step)
                    {
                        p[i].Start(new RenderPieceArgs(cur_x_s, cur_x_s + step, (int)-h, (int)h, (int)w, (int)h, false));
                        //Console.WriteLine($"{i}");
                    }
                    
                    while (true)
                    {
                        bool anyAlive = false;

                        foreach (var thread in p)
                        {
                            if (thread.IsAlive)
                            {
                                anyAlive = true;
                                break;
                            }
                        }

                        if (!anyAlive)
                        {
                            stopWatch.Stop();
                            TimeSpan ts = stopWatch.Elapsed;

                            double elapsedTime = Convert.ToDouble(String.Format("{0:00},{1:00}",ts.Seconds, ts.Milliseconds / 10));
                            sum += elapsedTime;
                            Console.WriteLine("RunTime " + elapsedTime);

                            break;
                        }
                        // Можно добавить задержку, чтобы не нагружать процессор
                        //Thread.Sleep(100);
                    }
                    
                    //while (p[0].IsAlive || p[1].IsAlive) { }
                    //tmp.Save("img2.jpg", ImageFormat.Jpeg);
                }
                Console.WriteLine("ResTime {0}", sum / 10);
                N *= 2;
            }
        }

        private void RenderPiece(Object obj) 
        {
            if (obj is RenderPieceArgs args)
            for (int x = args.x_start; x < args.x_finish; x++) 
            {
                for (int y = args.y_start; y < args.y_finish; y++) 
                {
                    Vector3D direction = CanvasToViewport(args.w, args.h, x, y);
                    System.Drawing.Color color = TraceRay(new TraceRayArgs((int)x, (int)y, camera_position, direction, 1, double.PositiveInfinity, recursion_depth));
                    
                    PutPixel((int)x, (int)y, color, args.DO_LOG);
                }
            }
        }

        Vector3D ReflectRay(Vector3D a, Vector3D b) { return b * (2 * dot(a, b)) - a; }
        double dot(Vector3D a, Vector3D b) { return (double)(a.X * b.X + a.Y * b.Y + a.Z * b.Z); }

        public delegate System.Drawing.Color ThreadTraceRay(Vector3D origin, Vector3D direction, double min_t, double max_t, int depth);
        System.Drawing.Color TraceRay(Object obj)
        {
            if (obj is TraceRayArgs args) 
            {
                KeyValuePair<Sphere, double> intersection = ClosestIntersection(args.origin, args.direction, args.min_t, args.max_t);
                if (intersection.Key == null)
                    return background_color;

                Sphere closest_sphere = intersection.Key;
                double closest_t = intersection.Value;

                Vector3D point = args.origin + args.direction * closest_t;
                Vector3D normal = point - closest_sphere.Center;
                normal = normal * (1.0 / normal.Length);

                Vector3D view = args.direction * (-1);
                double lighting = ComputeLighting(point, normal, view, closest_sphere.Specular);
                System.Drawing.Color local_color = closest_sphere.Color_mul(lighting);

                if (closest_sphere.Reflective <= 0 || args.depth <= 0)
                    return local_color;

                Vector3D reflected_ray = ReflectRay(view, normal);
                System.Drawing.Color reflected_color = TraceRay(new TraceRayArgs(args.x, args.y, point, reflected_ray, EPSILON,
                                                                double.PositiveInfinity, args.depth - 1));

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
                System.Drawing.Color res_color = System.Drawing.Color.FromArgb((local_con.A + reflected_con.A) % 256,
                                                     (local_con.R + reflected_con.R) % 256,
                                                     (local_con.G + reflected_con.G) % 256,
                                                     (local_con.B + reflected_con.B) % 256);
                return res_color;
            }
            return background_color;
        }

        System.Drawing.Color ReadPixel(int x, int y) 
        {
            x = canvas_buffer.Width / 2 + x;
            y = canvas_buffer.Height / 2 - y - 1;

            if (x < 0 || x >= canvas_buffer.Width || y < 0 || y >= canvas_buffer.Height) { return background_color; }

            return canvas_buffer.GetPixel(x, y);
        }
        void PutPixel(int x, int y, System.Drawing.Color color, /*ref Mutex mut, ref Mutex mutlog,*/ bool dolog)
        {
            int W, H;

            lock (locker)
            {
                W = tmp.Width;
                H = tmp.Height;
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
                tmp.SetPixel(x, y, color);
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
