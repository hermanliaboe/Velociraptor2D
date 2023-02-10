using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class CrossSection
    {
        public double height;
        public double width;
        public double youngsMod;

        public CrossSection() { }

        public CrossSection(double height, double width, double youngsMod)
        {
            this.height = height;
            this.width = width;
            this.youngsMod = youngsMod;
        }
    }
}
