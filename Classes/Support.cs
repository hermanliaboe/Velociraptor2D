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
        public bool tx;
        public bool tz;
        public bool ry;
        public Point3d point;

        public Support() { }

        public Support(bool tx, bool tz, bool ry, Point3d point)
        {
            this.tx = tx;
            this.tz = tz;
            this.ry = ry;
            

        }
    }
}
