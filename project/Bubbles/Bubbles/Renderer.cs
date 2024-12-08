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

        const double EPSILON = 0.001;

        int viewport_size;
        int d;
        Vector3D camera_position;
        List<Obj> spheres;
        List<Light> lights;
        int recursion_depth;

        Bitmap canvas_buffer;
        Graphics g;
        System.Drawing.Color background_color = System.Drawing.Color.FromArgb(255, 220, 240, 255); //Black;

        public Renderer(int viewport_size, int d, Vector3D camera_position, int recursive_depth, int canvas_width, int canvas_height) 
        { 
            this.viewport_size = viewport_size;
            this.d = d;
            this.camera_position = camera_position;
            spheres = new List<Obj>();
            lights = new List<Light>();
            this.recursion_depth = recursive_depth;

            canvas_buffer = new Bitmap(canvas_width, canvas_height);
            g = Graphics.FromImage(canvas_buffer);
        }
        public Bitmap Canvas_Buffer { get { return canvas_buffer; } }
        public void AddLight(Light l) { lights.Add(l); }
        public Obj Spheres(int i) { return spheres[i]; }
        public void SpheresChangeCenter(int i, Vector3D c, int k) 
        { 
            if (spheres[i] is Bubble b)
                b.Center = c;
            else if (spheres[i] is CombinedBubble cb) 
            { 
                if (k == 0) cb.Bubble1.Center = c;
                else if (k == 1) cb.Bubble2.Center = c;
            }
        }
        public void ChangeSphere(int id, Obj obj)
        {
            for (int i = 0; i < spheres.Count; i++) 
            { 
                if (spheres[i].Id == id)
                {
                    spheres[i] = obj;
                    return;
                }
            }
        }
        public int SpheresCount() { return spheres.Count; }
        public void AddSphere(Obj s) { spheres.Add(s); }

        public int ViewportSize { get { return viewport_size; }  set { viewport_size = value; } }
        public Vector3D CameraPosition { get { return camera_position; } set {  camera_position = value; } }
        public void Clean() {
            g.FillRectangle(new SolidBrush(background_color),
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

        Vector3D ReflectRay(Vector3D a, Vector3D b) { return b * (2 * Vector3D.DotProduct(a, b)) - a; }

        public delegate System.Drawing.Color ThreadTraceRay(Vector3D origin, Vector3D direction, double min_t, double max_t, int depth);
        System.Drawing.Color TraceRay(Object obj)
        {
            if (obj is TraceRayArgs args)
            {
                KeyValuePair<Bubble, double> intersection = ClosestIntersection(args.origin, args.direction, args.min_t, args.max_t);
                if (intersection.Key == null)
                    return background_color;

                Bubble closest = intersection.Key;
                double closest_t = intersection.Value;

                Vector3D point = args.origin + args.direction * closest_t;
                Vector3D normal = point - closest.Center;
                normal.Normalize();

                Vector3D view = args.direction * (-1);
                double lighting = ComputeLighting(point, normal, view, closest.Specular);
                System.Drawing.Color local_color = closest.Color_mul(lighting);

                if (closest.Reflective <= 0 || args.depth <= 0)
                    return local_color;

                double koef = 1 - closest.Reflective;
                System.Drawing.Color local_contribution = System.Drawing.Color.FromArgb(
                        (int)(local_color.R * koef),
                        (int)(local_color.G * koef),
                        (int)(local_color.B * koef)
                    );

                // Обработка отражения
                Vector3D reflected_ray = ReflectRay(view, normal);
                System.Drawing.Color reflected_color = TraceRay(new TraceRayArgs(args.x, args.y, point, reflected_ray, EPSILON,
                                                            double.PositiveInfinity, args.depth - 1));
                System.Drawing.Color reflected_contribution = System.Drawing.Color.FromArgb(
                    (int)(reflected_color.R * closest.Reflective),
                    (int)(reflected_color.G * closest.Reflective),
                    (int)(reflected_color.B * closest.Reflective)
                );

                System.Drawing.Color result_color = System.Drawing.Color.FromArgb(
                    Math.Min(255, local_contribution.R + reflected_contribution.R),
                    Math.Min(255, local_contribution.G + reflected_contribution.G),
                    Math.Min(255, local_contribution.B + reflected_contribution.B)
                );
                // Возвращаем окончательный цвет
                return result_color;
            }
            return background_color;
        }

        void PutPixel(int x, int y, System.Drawing.Color color, /*ref Mutex mut, ref Mutex mutlog,*/ bool dolog)
        {

            int W = canvas_buffer.Width;
            int H = canvas_buffer.Height;

            x = W / 2 + x;
            y = H / 2 - y - 1;

            if (x < 0 || x >= W || y < 0 || y >= H)
                return;
            canvas_buffer.SetPixel(x, y, color);
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
                KeyValuePair<Bubble, double> blocker = ClosestIntersection(point, vec_l, EPSILON, t_max);
                if (blocker.Key != null)
                {
                    continue;
                }

                // Diffuse reflection.
                double n_dot_l = Vector3D.DotProduct(normal, vec_l);
                if (n_dot_l > 0)
                {
                    intensity += light.Intensity * n_dot_l / (length_n * vec_l.Length);
                }

                // Specular reflection.
                if (specular != -1)
                {
                    Vector3D vec_r = (normal * 2.0 * n_dot_l) - vec_l;
                    double r_dot_v = Vector3D.DotProduct(vec_r, view);
                    if (r_dot_v > 0)
                    {
                        intensity += light.Intensity * Math.Pow(r_dot_v / (vec_r.Length * length_v), specular);
                    }
                }
            }

            return intensity;
        }

        public List<double> IntersectRayBubble(Vector3D origin, Vector3D direction, Bubble bubble)
        {
            Vector3D oc = origin - bubble.Center;

            double k1 = Vector3D.DotProduct(direction, direction);
            double k2 = 2.0 * Vector3D.DotProduct(oc, direction);
            double k3 = Vector3D.DotProduct(oc, oc) - bubble.Radius * bubble.Radius;

            double discriminant = k2 * k2 - 4 * k1 * k3;

            List<double> res = new List<double>();
            if (discriminant < 0)
            {
                // Нет пересечения
                res.Add(double.PositiveInfinity);
                res.Add(double.PositiveInfinity);
            }
            else
            {
                // Есть пересечение, находим точки пересечения
                double t1 = (-k2 + Math.Sqrt(discriminant)) / (2 * k1);
                double t2 = (-k2 - Math.Sqrt(discriminant)) / (2 * k1);
                res.Add(t1);
                res.Add(t2);

                // Проверяем, попадают ли точки пересечения в высоту сегмента
                Vector3D intersectionPoint1 = origin + direction * t1;
                Vector3D intersectionPoint2 = origin + direction * t2;

                // Проверка по высоте сегмента
                if (!bubble.IsWithinBubble(intersectionPoint1))
                    res[0] = double.PositiveInfinity; // Убираем первую точку пересечения

                if (!bubble.IsWithinBubble(intersectionPoint2))
                    res[1] = double.PositiveInfinity; // Убираем вторую точку пересечения
            }
            return res;
        }


        KeyValuePair<Bubble, double> ClosestIntersection(Vector3D origin, Vector3D direction,
                                                         double min_t, double max_t)
        {
            double closest_t = double.PositiveInfinity;
            Bubble closest_sphere = null;
            for (int i = 0; i < spheres.Count; i++)
            {
                if (spheres[i] is Bubble bubble)
                { 
                    List<double> ts = IntersectRayBubble(origin, direction, bubble);
                    for (int j = 0; j < ts.Count; j++)
                    {
                        if (ts[j] < closest_t && min_t < ts[j] && ts[j] < max_t)
                        {
                            closest_t = ts[j];
                            closest_sphere = bubble;
                        }
                    }
                }
                else if (spheres[i] is CombinedBubble cluster) 
                {
                    List<double> ts1 = IntersectRayBubble(origin, direction, cluster.Bubble1);
                    List<double> ts2 = IntersectRayBubble(origin, direction, cluster.Bubble2);
                    for (int j = 0; j < ts1.Count; j++)
                    {
                        if (ts1[j] < closest_t && min_t < ts1[j] && ts1[j] < max_t)
                        {
                            closest_t = ts1[j];
                            closest_sphere = cluster.Bubble1;
                        }
                    }
                    for (int j = 0; j < ts2.Count; j++)
                    {
                        if (ts2[j] < closest_t && min_t < ts2[j] && ts2[j] < max_t)
                        {
                            closest_t = ts2[j];
                            closest_sphere = cluster.Bubble2;
                        }
                    }
                }
            }
            if (closest_sphere != null)
            {
                return new KeyValuePair<Bubble, double>(closest_sphere, closest_t);
            }
            return new KeyValuePair<Bubble, double>(null, 0);
        }
    }
}
