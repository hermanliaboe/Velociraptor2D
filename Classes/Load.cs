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
        public Vector3d fVector;
        public Vector3d mVector;
        public int nodeID;


        public Load(){ }
    
        public Load(Point3d point, Vector3d fVector, Vector3d mVector) 
        {
            this.point = point;
            this.fVector = fVector;
            this.mVector = mVector;
        }
    }
}
