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
using Grasshopper.Kernel.Types;
using FEM.Properties;

namespace FEM.Components
{
    public class DynamicSolver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public DynamicSolver()
          : base("DynamicSolver", "femmern",
            "FEM solver with Newmark method",
            "Masters", "FEM")
        {
        }



        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Assembly", "ass", "", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("item in matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("globalK", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("globalKsup", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("forceVec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("displacements", "", "", GH_ParamAccess.list);
            pManager.AddMatrixParameter("list of rows in reduced stiffness matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("mass matrix!", "massMat", "", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Classes.Assembly model = new Classes.Assembly();
            DA.GetData(0, ref model);

            List<Load> loads = model.LoadList;
            List<BeamElement> elements = model.BeamList;
            List<Support> supports = model.SupportList;
            List<Node> nodes = model.NodeList;
            int dof = nodes.Count * 3;


            Matrices matrices = new Matrices();
            LA.Matrix<double> M = matrices.BuildGlobalM(dof, elements, true);
            LA.Matrix<double> K = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> C = LA.Matrix<double>.Build.Dense(dof, dof);
            

            DA.SetData(6, M);

        }
        void Newmark(double beta, double gamma, double dt, LA.Matrix<double> M, LA.Matrix<double> K, LA.Matrix<double> C)
        {
            // solve some shit
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SolverDyn;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6A936E0F-DBB8-4FAA-8CF4-132EB5724007"); }
        }
    }
}