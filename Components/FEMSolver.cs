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
            //pManager.AddMatrixParameter(" Global K", "globK", "", GH_ParamAccess.item);
            //pManager.AddMatrixParameter("Global K supports", "globksup", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("item in matrix","","",GH_ParamAccess.item);
            pManager.AddGenericParameter("globalK","","",GH_ParamAccess.item);
            pManager.AddGenericParameter("globalKsup", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("forceVec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("displacements", "", "", GH_ParamAccess.list);
            //pManager.AddNumberParameter("list of rows in reduced stiffness matrix","","",GH_ParamAccess.list);
            pManager.AddMatrixParameter("list of rows in reduced stiffness matrix", "", "", GH_ParamAccess.item);
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
          
            double E = 210000.0; //MPa
            double A = 100.0; //mm^2
            double I = (1.0/12.0)*Math.Pow(10.0,4.0);

            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildGlobalKsup(dof, globalK, supports, nodes);
            LA.Matrix<double> forceVec = BuildForceVector(loads, dof);
           
            LA.Matrix<double> displacements = globalKsup.Solve(forceVec);

            List<string> dispList = new List<string>();

            for (int i = 0; i < dof/3; i++)
            {
                var nodeDisp = "{" + displacements[i, 0] + ", " + displacements[i + 1, 0] + ", " + displacements[i + 2, 0] + "}";
                dispList.Add(nodeDisp);
            }

            Rhino.Geometry.Matrix rhinoMatrix = new Rhino.Geometry.Matrix(dof, dof);
            for (int i = 0; i < globalKsup.RowCount; i++)
            {
                for (int j = 0; j < globalKsup.ColumnCount; j++)
                {
                    rhinoMatrix[i,j] = globalKsup[i,j];
                }
            }

            //double[,] csGlobalKsup = globalKsup.ToArray();
            


            //DA.SetData(0, globalK);
            //DA.SetData(1, globalKsup);
            DA.SetData(0, globalKsup[9,9]);
            DA.SetData(1, globalK);
            DA.SetData(2, globalKsup);
            DA.SetData(3, forceVec);
            DA.SetDataList(4, dispList);
            DA.SetData(5, rhinoMatrix);
        }

        LA.Matrix<double> BuildForceVector(List<Load> loads, int dof)
        {
            LA.Matrix<double> forceVec = LA.Matrix<double>.Build.Dense(dof, 1, 0);

            foreach (Load load in loads)
            {
                forceVec[load.nodeID * 3, 0] = load.vector.X;
                forceVec[load.nodeID * 3 + 1, 0] = load.vector.Z;
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