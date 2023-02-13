using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class BeamElement
    {
        public string type;
        public int id;
        public List<Node> nodes;
        public Node startNode;
        public Node endNode;
        public Line line;
        public double length;
        public double height;
        public double width;
        public double youngsMod;
        public double selfWeight;

        public BeamElement() { }

        public BeamElement(int id, Line line)
        {
            this.id = id;
            this.line = line;
            this.length= getElementLength(line);
        }

        public double getElementLength(Line line)
        {
            //gets length of element
            Point3d startNode = line.To;
            double z1 = startNode.Z;
            double x1 = startNode.X;

            Point3d endNode = line.From;
            double z2 = endNode.Z;
            double x2 = endNode.X;

            double l = (Math.Sqrt(Math.Pow(z2 - z1, 2.0) + Math.Pow(x2 - x1, 2.0)));

            return l;
        }



    }
}
