using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class Support
    {
        public Point3d point;
        public Boolean tx;
        public Boolean tz;
        public Boolean ry;
        
        public Support() { }

        public Support(Point3d point, Boolean tx, Boolean tz, Boolean ry) 
        {
            this.point = point;
            this.tx = tx;
            this.tz = tz;
            this.ry = ry;
            

        }
    }
}
