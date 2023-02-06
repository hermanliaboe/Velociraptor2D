﻿using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace FEM.Classes
{
    internal class Node
    {
        public int localID;
        public Point3d point;
        public int globalID;
        public bool xBC;
        public bool zBC;
        public bool ry;


        public Node(int localID, Point3d point, int globalID, bool xBC, bool zBC, bool ry) 
        {
            this.localID = localID;
            this.point = point;
            this.globalID = globalID;
            this.xBC = xBC;
            this.zBC = zBC;
            this.ry = ry;
        }

        public Node() { }

    }
}