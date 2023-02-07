﻿using Rhino.Geometry;
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

        public BeamElement() { }

        public BeamElement(int id, Line line)
        {
            this.id = id;
            this.line = line;
        }
    }
}
