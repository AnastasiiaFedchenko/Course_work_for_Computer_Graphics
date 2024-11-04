using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bubbles
{
    public class Light
    {
        public enum light_type: ushort
        {
            AMBIENT = 0,
            POINT = 1,
            DIRECTIONAL = 2
        };
        light_type ltype;
        double intensity;
        Vector3D position;
        public Light(light_type ltype, double intensity, Vector3D position)
        {
            this.ltype = ltype;
            this.intensity = intensity;
            this.position = position;
        }

        public light_type Ltype { get { return ltype; } set {  ltype = value; } }
        public double Intensity { get { return intensity; } set {  intensity = value; } }
        public Vector3D Position { get {  return position; } set { position = value; } }
    }
}
