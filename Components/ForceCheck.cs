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
using MathNet.Numerics.Integration;

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
            pManager.AddGenericParameter("Nodal forces", "nodalForces", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("KarambaForces", "karambaForces", "Needs to by a flatten list of moments of all beams", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddNumberParameter("Error at each node", "errorNode", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Total Error", "errorTot", "", GH_ParamAccess.item);

            pManager.AddNumberParameter("moment at each node, avarage", "momNode", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DenseMatrix nodalForces = new DenseMatrix(1);
            List<Double> karambaForces = new List<Double>();   
           

            DA.GetData(0, ref nodalForces);
            DA.GetDataList(1, karambaForces);

            List<double> errorNode = new List<double>();
            double errorAvg = 0;

            List<double> M = new List<double>();

            M.Add(nodalForces[2, 0] * 0.000001);
            for (int i = 1; i < nodalForces.ColumnCount; i++)
            {
                double m = (nodalForces[5, i - 1]) * 0.000001;
                M.Add(m);
            }
            M.Add(nodalForces[5, nodalForces.ColumnCount-1] * 0.000001);
            

            int j = 1;
            errorNode.Add(100 - Math.Abs(M[0]/ karambaForces[0]) *100);          
            for (int i = 1; i < M.Count-1; i++)
            {
                errorNode.Add(100 - Math.Abs(M[i] / karambaForces[j])*100);
                j = j + 2;
            }
            errorNode.Add(100 - Math.Abs(M[M.Count-1] / karambaForces[karambaForces.Count-1])*100);
            


            errorAvg = errorNode.Average();

            
            DA.SetDataList(0, errorNode);
            DA.SetData(1, errorAvg);
            DA.SetDataList(2, M);

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
                return null;
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