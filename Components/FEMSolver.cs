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
          : base("Static FEMSolver", "femmern",
            "FEM solver with Newmark method",
            "Masters", "FEM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Assembly","ass","",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            //pManager.AddPointParameter("New points","","",GH_ParamAccess.list);
            //pManager.AddLineParameter("New geometry","lines","", GH_ParamAccess.list);
            pManager.AddMatrixParameter(" Global K", "globK", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Global K supports", "globksup", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Classes.Assembly model = new Classes.Assembly();
            DA.GetData(0, ref model);
            

            List<Load> loads = model.loadList;
            List<BeamElement> elements = model.beamList;
            List<Support> supports = model.supportList;
            List<Node> nodes = model.nodeList;



            int dof = model.nodeList.Count*3;
          
            double E = 7000; //MPa
            double A = 10000; //mm^2
            double I = 12000;

            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements, E, A, I);
            //LA.Matrix<double> globalKsup = matrices.BuildGlobalKsup(dof, globalK, supports, nodes);
            LA.Matrix<double> forceVec = BuildForceVector(loads, dof);

            DA.SetData(0, globalK);
            //DA.SetData(1, globalKsup);


        }
        LA.Matrix<double> BuildForceVector(List<Load> loads, int dof)
        {
            LA.Matrix<double> forceVec = LA.Matrix<double>.Build.Dense(dof, 1);

            foreach (Load load in loads)
            {
                forceVec[load.nodeID*3, 0] = load.vector.X;
                forceVec[load.nodeID*3 + 1, 0] = load.vector.Z;
                // forceVec[load.nodeID*3 + 2, 0] = load.vector.r; moment
            }



            return forceVec;
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