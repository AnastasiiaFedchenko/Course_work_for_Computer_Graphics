using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.ListBox;
//using static System.Collections.Specialized.BitVector32;

namespace Bubbles
{
    public partial class Form1 : Form
    {
        const double EPSILON = 0.001;

        Renderer drawer;
        Vector3D ViewPoint;
        List<Contour> contours;
        int latest_id = 0;
        Vector3D initial_cursor;
        Bitmap buf_copy = null;
        int W, H;

        public Form1()
        {
            InitializeComponent();
            W = Canvas.Width;
            H = Canvas.Height;
            ViewPoint = new Vector3D(0, 0, -4);
            drawer = new Renderer(1, 1, ViewPoint, 3, W, H);
            contours = new List<Contour>();

            Canvas.Image = drawer.Canvas_Buffer;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bubble bubble1 = new Bubble(1, new Vector3D(-2.2, -0.65, 3), 0.75, System.Drawing.Color.FromArgb(0, 255, 0), 500, 0.4);
            Bubble bubble2 = new Bubble(2, new Vector3D(-2.5, 0.25, 3), 1, System.Drawing.Color.FromArgb(0, 0, 255), 500, 0.3);

            List<Obj> res1 = CombinedBubble.PositionBubbles(bubble1, bubble2, false);
            for (int i = 0; i < res1.Count; i++)
                if (res1[i] != null)
                    drawer.AddSphere(res1[i]);

            Bubble bubble3 = new Bubble(3, new Vector3D(2, -1, 3), 1, System.Drawing.Color.HotPink, 500, 0.4);
            Bubble bubble4 = new Bubble(4, new Vector3D(1, 0.25, 3), 1, System.Drawing.Color.Violet, 500, 0.3);

            List<Obj> res2 = CombinedBubble.PositionBubbles(bubble3, bubble4, false);
            for (int i = 0; i < res2.Count; i++)
                if (res2[i] != null)
                    drawer.AddSphere(res2[i]);
            Bubble bubble5 = new Bubble(5, new Vector3D(-1.5, 0.75, 3), 0.75, System.Drawing.Color.FromArgb(255, 0, 0), 500, 0.4);
            drawer.AddSphere(bubble5);

            int rc = position_bubbles(3);

            bubbles_checked_list();

            drawer.AddLight(new Light(Light.light_type.AMBIENT, 0.1, new Vector3D(0, 0, 0)));
            drawer.AddLight(new Light(Light.light_type.POINT, 0.8, new Vector3D(2, 1, 0)));
            drawer.AddLight(new Light(Light.light_type.DIRECTIONAL, 0.4, new Vector3D(1, 4, 4)));

            if (rc != -1)
                drawer.Render();
            Canvas.Invalidate();
        }

        private void bubbles_checked_list() 
        {
            checkedListBox1.Items.Clear();
            Obj obj;
            for (int i = 0; i < drawer.SpheresCount(); i++)
            {
                obj = drawer.Spheres(i);
                if (obj.Id > latest_id)
                    latest_id = obj.Id;

                if (obj is  Bubble bubble) 
                {
                    checkedListBox1.Items.Add(bubble.Id.ToString() + " пузырёк");
                }
                else if (obj is CombinedBubble c)
                {
                    checkedListBox1.Items.Add(c.Id.ToString() + " кластер");
                }
                checkedListBox1.Invalidate();
            }
            
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
            latest_id++;
            Bubble new_bubble = new Bubble(latest_id, new Vector3D(ox, oy, oz), r, ColorButton.BackColor, 500, 0.5);
            drawer.AddSphere(new_bubble);
            bubbles_checked_list();
            drawer.Render();
            Canvas.Invalidate();
        }

        private void ClearSceneButton_Click(object sender, EventArgs e)
        {
            drawer = new Renderer(1, 1, ViewPoint, 1, W, H);
            drawer.AddLight(new Light(Light.light_type.AMBIENT, 0.2, new Vector3D(0, 0, 0)));
            drawer.AddLight(new Light(Light.light_type.POINT, 0.6, new Vector3D(2, 1, 0)));
            drawer.AddLight(new Light(Light.light_type.DIRECTIONAL, 0.2, new Vector3D(1, 4, 4)));
            Canvas.Image = drawer.Canvas_Buffer;

            ColorButton.BackColor = System.Drawing.Color.Red;

            OxTextBox.Text = "0";
            OyTextBox.Text = "0";
            OzTextBox.Text = "3";
            RTextBox.Text = "1";
            checkedListBox1.Items.Clear();
            latest_id = 0;

            drawer.Clean();
            Canvas.Invalidate();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            drawer.CameraPosition = new Vector3D(0, 0, trackBar1.Value);
            drawer.Render();
            Canvas.Invalidate();
        }

        static int ExtractLeadingNumber(string input)
        {
            Match match = Regex.Match(input, @"^\d+");
            if (match.Success)
                return int.Parse(match.Value);
            return -1;
        }
        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {   
            initial_cursor = new Vector3D(e.X, e.Y, 0);
            buf_copy = drawer.Canvas_Buffer.Clone(new Rectangle(0, 0, drawer.Canvas_Buffer.Width, drawer.Canvas_Buffer.Height), 
                                                  drawer.Canvas_Buffer.PixelFormat);
            
            Bitmap buf = buf_copy.Clone(new Rectangle(0, 0, buf_copy.Width, buf_copy.Height), buf_copy.PixelFormat);
            Canvas.Image = buf;
            Graphics g = Graphics.FromImage(buf);
            int W = buf.Width;
            int H = buf.Height;
            

            double scaleFactor = (double)(W) / 1.0; // Определяем коэффициент масштабирования

            CheckedListBox.CheckedItemCollection o = checkedListBox1.CheckedItems;
            contours = new List<Contour>();
            if (o.Count <= 0) return;

            String str = (String)o[0];
            int number = ExtractLeadingNumber(str);
            for (int j = 0; j < drawer.SpheresCount(); j++)
            {
                if (drawer.Spheres(j).Id != number)
                    continue;

                Obj obj = drawer.Spheres(j);
                List<Bubble> bubbles = new List<Bubble>();
                if (obj is Bubble bubble)
                    bubbles.Add(bubble);
                else if (obj is CombinedBubble cluster)
                {
                    bubbles.Add(cluster.Bubble1);
                    bubbles.Add(cluster.Bubble2);
                }
                for (int t = 0; t < bubbles.Count; t++)
                {
                    Vector3D center = bubbles[t].Center;
                    double radius = bubbles[t].Radius;

                    double distance = center.Z - ViewPoint.Z;

                    // Масштабируем радиус в зависимости от расстояния до экрана
                    double radius_from_physics = radius * (1.0 / distance); // Применяем перспективу

                    // Преобразуем 3D координаты в 2D
                    double x_from_p = center.X * (1.0 / distance);
                    double y_from_p = center.Y * (1.0 / distance);
                    int x = (int)(W / 2 + x_from_p * scaleFactor);
                    int y = (int)(H / 2 - y_from_p * scaleFactor);

                    // Рисуем круг
                    int r = (int)(scaleFactor * radius_from_physics); // Преобразуем радиус в пиксели
                    g.DrawEllipse(Pens.Red, x - r, y - r, r * 2, r * 2);

                    contours.Add(new Contour(number, new Vector3D(x, y, 0), r, t));
                }
                break;
            }
        
            Canvas.Invalidate();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            /*double distance = 3 - ViewPoint.Z;
            double scaleFactor = (double)(W) / 1.0; // Определяем коэффициент масштабирования

            double x_from_p = (e.X - W / 2) / scaleFactor;
            double y_from_p = (H / 2 - e.Y) / scaleFactor;

            double actual_x = x_from_p / (1.0 / distance);
            double actual_y = y_from_p / (1.0 / distance);
            
            toolStripStatusLabel1.Text = $"x: {actual_x}, y: {actual_y}";*/
            toolStripStatusLabel1.Text = e.Location.ToString();

            if (contours.Count > 0)
            {
                Bitmap buf = buf_copy.Clone(new Rectangle(0, 0, buf_copy.Width, buf_copy.Height), buf_copy.PixelFormat);
                Canvas.Image = buf;
                Graphics g = Graphics.FromImage(buf);

                int shift_x = e.X - (int)initial_cursor.X;
                int shift_y = e.Y - (int)initial_cursor.Y;
                foreach (Contour c in contours) 
                {
                    g.DrawEllipse(Pens.Red, (float)(c.Center.X + shift_x - c.R), 
                        (float)(c.Center.Y + shift_y - c.R), (float)c.R * 2, (float)c.R * 2);
                }
                Canvas.Invalidate();
            }
        }
        
        private List<List<int>> counting_contacts(ref bool any_intersection, ref bool any_more_than_2)
        {
            List<List<int>> a = new List<List<int>>();
            List<int> count = new List<int>(); // построчная сумма a

            for (int i = 0; i < drawer.SpheresCount(); i++)
            {
                List<int> temp = new List<int>();
                for (int j = 0; j < drawer.SpheresCount(); j++)
                    temp.Add(0);
                a.Add(temp);
            }
            for (int i = 0; i < drawer.SpheresCount(); i++)
            {
                count.Add(0);
                for (int j = 0; j < drawer.SpheresCount(); j++)
                {
                    if (i == j) continue;
                    
                    int res = CombinedBubble.AmountOfContacts(drawer.Spheres(i), drawer.Spheres(j));
                    a[i][j] = res;
                    if (res == -1)
                        count[i] += 2;
                    else
                    {
                        if (res == 1)
                        {
                            Console.WriteLine($"id: {drawer.Spheres(i).Id} касается id: {drawer.Spheres(j).Id}");
                            any_intersection = true;
                        }
                        count[i] += res;
                    }
                }
                if (count[i] >= 2)
                    any_more_than_2 = true;
                Console.WriteLine($"id: {drawer.Spheres(i).Id} всего {count[i]} соприкосновений.");
            }
            for (int i = 0; i < count.Count; i++)
                Console.Write($"{count[i]} ");
            Console.Write("\n");
            
            return a;
        }
        private int position_two_bubbles(int i, int j, Bubble b1, Bubble b2)
        {
            int id1 = drawer.Spheres(i).Id;
            int id2 = drawer.Spheres(j).Id;
            List<Obj> r = new List<Obj>();
            r = CombinedBubble.PositionBubbles(b1, b2, false);
            for (int k = 0; r != null && k < r.Count; k++)
                drawer.ChangeSphere(r[k].Id, r[k]);
            if (r != null && r.Count == 1)
            {
                if (r[0].Id == id1)
                    drawer.DeleteSphere(id2);
                else
                    drawer.DeleteSphere(id1);
            }

            return 0;
        }
        private int position_cluster_and_bubble(int i, int j, CombinedBubble cb, Bubble b)
        {
            List<Obj> r1 = new List<Obj>();
            List<Obj> r2 = new List<Obj>();
            int id1 = drawer.Spheres(i).Id;
            int id2 = drawer.Spheres(j).Id;

            r1 = CombinedBubble.PositionBubbles(cb.Bubble1, b, true);
            r2 = CombinedBubble.PositionBubbles(cb.Bubble2, b, true);
            if (r1 == null && r2 == null) // ситуация с тройным кластером
            {
                MessageBox.Show(
                    $"Угол соприкосновения принадлежит [55°, 65°]. Невозможно образование кластера из трёх пузырей.",
                    "ERROR");
                Console.WriteLine("ERROR! 3 bubble cluster");
                return -1;
            }
            else if (r1 != null && r2 == null && r1.Count == 1 && r1[0] is Bubble) // произошло объединение пузырька b с cb.Bubble1
            {
                r1 = CombinedBubble.PositionBubbles((Bubble)r1[0], cb.Bubble2, false); // проверяем позицию нового пузыря относительно cb.Bubble2
                if (r1.Count == 2)
                {
                    r1[0].Id = id1;
                    r1[1].Id = id2;
                }
                else if (r1.Count == 1)
                {
                    if (r1[0].Id == id1)
                        drawer.DeleteSphere(id2);
                    else
                        drawer.DeleteSphere(id1);
                }
            }
            else if (r2 != null && r1 == null && r2.Count == 1 && r2[0] is Bubble) // произошло объединение пузырька b с cb.Bubble2
            {
                r2 = CombinedBubble.PositionBubbles((Bubble)r2[0], cb.Bubble1, false); // проверяем позицию нового пузыря относительно cb.Bubble1
                if (r2.Count == 2)
                {
                    r2[0].Id = id1;
                    r2[1].Id = id2;
                }
                else if ( r2.Count == 1)
                {
                    if (r2[0].Id == id1)
                        drawer.DeleteSphere(id2);
                    else
                        drawer.DeleteSphere(id1);
                }
            }
            else if (r1 != null && r2 == null && r1.Count == 2 && r1[0] is Bubble && r1[1] is Bubble) // произошло отталкивание пузырька b от cb.Bubble1
            {
                // r1[0] - это пузырёк являющийся частью кластера, cb.Bubble1
                Bubble bb = (Bubble)r1[0];
                Vector3D dist = cb.Bubble1.Center - bb.Center;
                cb.Bubble2.Center = cb.Bubble2.Center + dist;
                List<Obj> res = CombinedBubble.PositionBubbles((Bubble)r1[0], cb.Bubble2, false);
                if (res != null && res.Count == 1 && res[0] is CombinedBubble)
                    r1[0] = res[0];
            }
            else if (r2 != null && r1 == null && r2.Count == 2 && r2[0] is Bubble && r2[1] is Bubble) // произошло отталкивание пузырька b от cb.Bubble2
            {
                // r2[0] - это пузырёк являющийся частью кластера, cb.Bubble2
                Bubble bb = (Bubble)r2[0];
                Vector3D dist = cb.Bubble2.Center - bb.Center;
                cb.Bubble1.Center = cb.Bubble1.Center + dist;
                List<Obj> res = CombinedBubble.PositionBubbles((Bubble)r2[0], cb.Bubble1, false);
                if (res != null && res.Count == 1 && res[0] is CombinedBubble)
                    r2[0] = res[0];
            }

            for (int k = 0; r1 != null && k < r1.Count; k++)
                drawer.ChangeSphere(r1[k].Id, r1[k]);
            for (int k = 0; r2 != null && k < r2.Count; k++)
                drawer.ChangeSphere(r2[k].Id, r2[k]);

            return 0;
        }
        private int position_bubbles(int n)
        {
            if (n <= 0)
            {
                Console.WriteLine("Превышен лимит рекурсии.");
                return -1;
            }
            bool any_intersection = false;
            bool any_more_than_2 = false;
            List<List<int>> a = counting_contacts(ref any_intersection, ref  any_more_than_2);
           
            if (any_more_than_2)
            {
                Console.WriteLine("Больше чем 2 пересечения.");
                MessageBox.Show(
                    "Больше чем одно пересечение",
                    "ERROR");
                return -1;
            }
            else if (any_intersection == false)
            {
                Console.WriteLine("no intersections");
                return 0;
            }
            else
            {
                for (int i = 0; i < a.Count; i++)
                    for (int j = 0; j < a[i].Count; j++)
                    {
                        if (a[i][j] == 1)
                        {
                            int rc = 0;
                            if (drawer.Spheres(i) is Bubble && drawer.Spheres(j) is Bubble)
                                rc = position_two_bubbles(i, j, (Bubble)drawer.Spheres(i), (Bubble)drawer.Spheres(j));
                            else if (drawer.Spheres(i) is CombinedBubble && drawer.Spheres(j) is Bubble)
                                rc = position_cluster_and_bubble(i, j, (CombinedBubble)drawer.Spheres(i), (Bubble)drawer.Spheres(j));
                            else if (drawer.Spheres(i) is Bubble b4 && drawer.Spheres(j) is CombinedBubble cb2)
                                rc = position_cluster_and_bubble(j, i, (CombinedBubble)drawer.Spheres(j), (Bubble)drawer.Spheres(i));
                            if (rc == -1)
                                return -1;
                            a[i][j] = 0;
                            a[j][i] = 0;
                            position_bubbles(n);
                        }
                    }
            }
            
            return position_bubbles(n - 1);
        }
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int ix = 0; ix < checkedListBox1.Items.Count; ++ix)
                if (ix != e.Index) checkedListBox1.SetItemChecked(ix, false);
        }
        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            double scaleFactor = (double)(drawer.Canvas_Buffer.Width) / 1.0; // Определяем коэффициент масштабирования
            int shift_x = e.X - (int)initial_cursor.X;
            int shift_y = e.Y - (int)initial_cursor.Y;
            Bitmap buf = buf_copy.Clone(new Rectangle(0, 0, buf_copy.Width, buf_copy.Height), buf_copy.PixelFormat);
            Canvas.Image = buf;
            Graphics g = Graphics.FromImage(buf);
            int W = buf.Width;
            int H = buf.Height;

            int k = 0;

            int actual_i;
            List<Vector3D> initial_centers = new List<Vector3D>();
            if (contours.Count > 0)
            {
                actual_i = 0;
                foreach (Contour c in contours)
                {
                    int id = c.Id;
                    double x = c.Center.X + shift_x;
                    double y = c.Center.Y + shift_y;
                    double x_from_p = (x - W / 2) / scaleFactor;
                    double y_from_p = (H / 2 - y) / scaleFactor;
                    for (int i = 0; i < drawer.SpheresCount(); i++) 
                    {
                        if (drawer.Spheres(i).Id == id) 
                        {
                            actual_i = i;
                            if (drawer.Spheres(i) is Bubble b)
                            {
                                initial_centers.Add(b.Center);
                                double distance = b.Center.Z - ViewPoint.Z;

                                double actual_x = x_from_p / (1.0 / distance);
                                double actual_y = y_from_p / (1.0 / distance);

                                drawer.SpheresChangeCenter(i, new Vector3D(actual_x, actual_y, b.Center.Z), 0);
                            }
                            else if (drawer.Spheres(i) is CombinedBubble cb)
                            {                              
                                k = k % 2;
                                Bubble bubble = (k == 0) ? cb.Bubble1 : cb.Bubble2;
                                initial_centers.Add(bubble.Center);
                                double distance = bubble.Center.Z - ViewPoint.Z;

                                double actual_x = x_from_p / (1.0 / distance);
                                double actual_y = y_from_p / (1.0 / distance);

                                drawer.SpheresChangeCenter(i, new Vector3D(actual_x, actual_y, bubble.Center.Z), k);
                                k++;
                            }
                        }
                    }
                }
                contours = new List<Contour>();
                int rc = position_bubbles(3);
                if (rc != 0) 
                {
                    if (drawer.Spheres(actual_i) is Bubble b)
                        drawer.SpheresChangeCenter(actual_i, initial_centers[0], 0);
                    else if (drawer.Spheres(actual_i) is CombinedBubble cb)
                    {
                        drawer.SpheresChangeCenter(actual_i, initial_centers[0], 0);
                        drawer.SpheresChangeCenter(actual_i, initial_centers[1], 1);
                    }
                }
                bubbles_checked_list();
                drawer.Render();
                buf_copy = drawer.Canvas_Buffer.Clone(new Rectangle(0, 0, drawer.Canvas_Buffer.Width, drawer.Canvas_Buffer.Height),
                                                  drawer.Canvas_Buffer.PixelFormat);
                //buf_copy.Save("copy2.jpg");
                Canvas.Image = drawer.Canvas_Buffer;
                Canvas.Invalidate();
            }
        }
    }
    

}
