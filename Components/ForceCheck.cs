using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using LA = MathNet.Numerics.LinearAlgebra;
using FEM.Classes;
using FEM.Properties;
using System.Linq;

namespace FEM.Components
{
    public class ForceCheck : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ForceCheck()
          : base("ForceCheck", "ForceCheck",
              "Description",
              "Masters", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            List<double> l0 = new List<double>();
            pManager.AddGenericParameter("Beam forces", "BeamF", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("My - Kara", "My-Kara", "", GH_ParamAccess.list, l0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("error, avarage", "momNode", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("moments", "M", "", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input

            LA.Double.DenseMatrix bfV = new LA.Double.DenseMatrix(2);
            List<double> myK = new List<double>();
            
            DA.GetData(0, ref bfV);
            DA.GetDataList(1, myK);

            List<double> fBeam = new List<double>();

            for (int i = 0; i < bfV.ColumnCount; i++)
            {
                fBeam.Add(errorFunc(Math.Round(bfV[  2, i], 6), Math.Round(myK[i * 2] * 1000000, 6)));
                fBeam.Add(errorFunc(Math.Round(bfV[  5, i], 6), Math.Round(myK[i * 2 +1] * 1000000, 6)));
            }

            Double error = fBeam.Sum() / fBeam.Count;


            List<double> M = new List<double>();

            M.Add(bfV[2, 0] * 0.000001);
            for (int i = 1; i < bfV.ColumnCount; i++)
            {
                double m = (bfV[5, i - 1]) * 0.000001;
                M.Add(m);
            }
            M.Add(bfV[5, bfV.ColumnCount-1] * 0.000001);


            DA.SetData(0, error);
            DA.SetDataList(1, M);



        }

        public double errorFunc(double V, double K)
        {
            double error = 0.0;

            V += 0.00000001;
            K += 0.00000001;
            //double error = 0.0;
            error = Math.Round(100.0 * Math.Abs((Math.Abs(V) - Math.Abs(K)) / Math.Abs(V)), 6);

            return error;
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
                return Resources.checky;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8D2B9547-04EB-4FD0-8E7A-23C462662086"); }
        }
    }
}