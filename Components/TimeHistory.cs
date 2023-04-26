using System;
using System.Collections.Generic;
using LA = MathNet.Numerics.LinearAlgebra;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEM.Classes;
using FEM.Properties;

namespace FEM.Components
{
    public class TimeHistory : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TimeHistory class.
        /// </summary>
        public TimeHistory()
          : base("TimeHistory", "Nickname",
              "Description",
              "Masters", "FEM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Displacements", "", "Displacements output from SolverDynamic", GH_ParamAccess.item);
            pManager.AddGenericParameter("Node", "", "Node of interest", GH_ParamAccess.item);
            pManager.AddIntegerParameter("DOF", "", "Degree of freedom of interest. x=0, z=1, My = 2", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacements", "", "Displacements for chosen DOF.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var displacementsDOF = LA.Matrix<double>.Build.Dense(0, 0);
            var nodePlot = new Node();
            int dof = 0;
            DA.GetData(0, ref displacementsDOF);
            DA.GetData(1, ref nodePlot);
            DA.GetData(2, ref dof);

            List<double> displacementsPlot = new List<double>();

            for (int i = 0; i < displacementsDOF.ColumnCount; i++)
            {
                displacementsPlot.Add(displacementsDOF[nodePlot.GlobalID * 3 + dof, i]);
            }

            DA.SetDataList(0, displacementsPlot);
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
                return Resources.th;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C5A01D24-1929-4BF6-A0E0-48DA18DEBDBB"); }
        }
    }
}