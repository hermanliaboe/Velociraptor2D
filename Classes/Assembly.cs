using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class Assembly
    {
        public List<BeamElement> beamList;
        public List<Support> supportList;
        public List<Load> loadList;
        public List<Node> nodeList;


        public Assembly() { }

        public Assembly(List<BeamElement> beamList, List<Support> supportList, List<Load> loadList, List<Node> nodeList)
        {
            this.beamList = beamList;
            this.supportList = supportList;
            this.loadList = loadList;
            this.nodeList = nodeList;
        }
    }
}
