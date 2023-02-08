using FEM.Classes;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;

using LA = MathNet.Numerics.LinearAlgebra;
using Rhino.Commands;
using Rhino.Render;
using System.IO;

namespace FEM.Components
{
    public class FEMSolver : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FEMSolver()
          : base("Dynamic FEMSolver", "femmern",
            "FEM solver with Newmark method",
            "Masters", "FEM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            //kode kode kode
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get input and deconstruct assembly into beams, loads and supports. 
            /*
          List<Assembly> assembly = new List<Assembly>();
          DA.GetData(0, ref assembly);


          beamList = assembly.beamList;
          supportList = assembly.supportList;
          loadList = assembly.loadList;

          //Input for element definition:
          double e = 1;   //E-moduls
          double a = 0.1; //Areal
          double v = 0.3; //Poisson-ratio, if neede

          */

            //End

            //Assembly of global stiffness matrix






            //End

            //Applying BC to K
            //End

            //Calculate element stresses
            //End
        }

        LA.Matrix<double> getKel(BeamElement beam, double e, double a)
        {
            int nNode = 2;  //how many nodes per element


            //gets length of element

            Node startNode = beam.startNode;
            double z1 = startNode.point.Z;
            double x1 = startNode.point.X;

            Node endNode = beam.endNode;
            double z2 = startNode.point.Z;
            double x2 = startNode.point.X;

            double l = Math.Sqrt(Math.Pow(z2 - z1, 2) + Math.Pow(x2 - x1, 2));

            LA.Matrix<double> kEl = LA.Matrix<double>.Build.Dense(nNode * 3, nNode * 3);

            double ealA =    (e * a) / l;
            double ealB = 12*(e * a) / Math.Pow(l,3);
            double ealC = 6* (e * a) / Math.Pow(l, 2);
            double ealD = 4* (e * a) / l;
            double ealE = 2* (e * a) / l;

            kEl[0, 1] = ealA;  kEl[1, 1] = 0;     kEl[2, 1] = 0;     kEl[3, 1] = -ealA; kEl[4, 1] = 0;     kEl[5, 1] = 0;
            kEl[0, 2] = 0;     kEl[1, 2] = ealB;  kEl[2, 2] = -ealC; kEl[3, 2] = 0;    kEl[4, 2] = -ealB; kEl[5, 2] = -ealC;
            kEl[0, 3] = 0;     kEl[1, 3] = -ealC; kEl[2, 3] = ealD;  kEl[3, 3] = 0;    kEl[4, 3] = ealC;  kEl[5, 3] = ealE;
            kEl[0, 4] = -ealA; kEl[1, 4] = 0;     kEl[2, 4] = 0;     kEl[3, 4] = ealA; kEl[4, 4] = 0;     kEl[5, 4] = 0;
            kEl[0, 5] = 0;     kEl[1, 5] = -ealB; kEl[2, 5] = ealC;  kEl[3, 5] = 0;    kEl[4, 5] = ealB;  kEl[5, 5] = ealC;
            kEl[0, 6] = 0;     kEl[1, 6] = -ealC; kEl[2, 6] = ealD;  kEl[3, 6] = 0;    kEl[4, 6] = ealC;  kEl[5, 6] = ealE;







            return kEl;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e9d8a089-9a52-4aaf-84a9-1fb4630d5e14");
    }
}