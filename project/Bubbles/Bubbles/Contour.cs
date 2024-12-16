using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bubbles
{
    public class Contour
    {
        int id;
        Vector3D center;
        double r;
        int k;
        public Contour(int id, Vector3D center, double r) 
        {
            this.id = id;
            this.center = center;
            this.r = r;
            this.k = 0;
        }
        public Contour(int id, Vector3D center, double r, int k)
        {
            this.id = id;
            this.center = center;
            this.r = r;
            this.k = k;
        }
        public int Id { get { return id; } }
        public Vector3D Center { get {  return center; } }
        public double R { get { return r; } }
        //public int K { get { return k; } }
    }
}
