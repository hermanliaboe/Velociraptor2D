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
using Grasshopper.GUI;
using MathNet.Numerics.Interpolation;
using Grasshopper.Kernel.Geometry;

namespace FEM.Components
{
    public class SolverStatic : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SolverStatic()
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
            pManager.AddNumberParameter("Scale", "Scale", "", GH_ParamAccess.item);
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
            pManager.AddGenericParameter("mass matrix!", "massMat", "", GH_ParamAccess.item);
            pManager.AddLineParameter("lines baby", "lines", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Classes.Assembly model = new Classes.Assembly();
            int scale = 0;

            DA.GetData(0, ref model);
            DA.GetData(1, ref scale);


            List<Load> loads = model.LoadList;
            List<BeamElement> elements = model.BeamList;
            List<Support> supports = model.SupportList;
            List<Node> nodes = model.NodeList;
       



            int dof = model.NodeList.Count*3;
          
         
            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildGlobalKsup(dof, globalK, supports, nodes);
            LA.Matrix<double> forceVec = matrices.BuildForceVector(loads, dof);



            LA.Matrix<double> displacements = globalKsup.Solve(forceVec);

            List<string> dispList = new List<string>();
            for (int i = 0; i < dof; i=i+3)
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
            List<Line> lineList1 = new List<Line>();
            getNewGeometry(scale, displacements, elements, out lineList1);

            //DA.SetData(0, globalK);
            //DA.SetData(1, globalKsup);
            DA.SetData(0, globalKsup[9,9]);
            DA.SetData(1, globalK);
            DA.SetData(2, globalKsup);
            DA.SetData(3, forceVec);
            DA.SetDataList(4, dispList);
            DA.SetData(6, displacements);
            DA.SetDataList(7, lineList1);
            
        }



        void getNewGeometry(int scale, LA.Matrix<double> displacements, List<BeamElement> beams, out List<Line> lineList)
        {
            List<Line> linelist2 = new List<Line>();
            int i = 3;
            foreach (BeamElement beam in beams)
            {
                int startId = beam.StartNode.GlobalID;
                double sX1 = beam.StartNode.Point.X;
                double sZ1 = beam.StartNode.Point.Z;

                int endId = beam.EndNode.GlobalID;
                double sX2 = beam.EndNode.Point.X;
                double sZ2 = beam.EndNode.Point.Z;

                double x1 = displacements[startId * i,  0];
                double z1 = displacements[startId * i +1,0];
                //double r1 = displacements[startId * i + 2,0];
                Point3d sP = new Point3d(sX1 + x1 * scale, 0, sZ1 + z1 * scale);

                double x2 = displacements[endId * i,   0];
                double z2 = displacements[endId * i + 1, 0];
                //double r2 = displacements[endId * i + 2, 0];
                Point3d eP = new Point3d(sX2 + x2 * scale, 0,sZ2 + z2 * scale);

                linelist2.Add(new Line(sP, eP));
            }
            lineList = linelist2;
        }

       

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SolverStatic_main_;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e9d8a089-9a52-4aaf-84a9-1fb4630d5e14");
    }
}