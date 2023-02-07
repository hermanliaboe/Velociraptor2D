using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class Load
    {
        public Point3d point;
        public Vector3d vector;


        public Load(){ }
    
        public Load(Point3d point, Vector3d vector) 
        {
            this.point = point;
            this.vector = vector;
        }
    }
}
