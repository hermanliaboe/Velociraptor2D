using FEM.Classes;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;
using MathNet.Numerics;
using MathNet.Numerics.OdeSolvers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using LA = MathNet.Numerics.LinearAlgebra;


using Rhino.Commands;
using Rhino.Render;
using System.IO;
using Grasshopper.Kernel.Types;
using FEM.Properties;
using System.Numerics;
using System.Linq;
using GH_IO;
using MathNet.Numerics.LinearAlgebra.Complex;

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
            pManager.AddNumberParameter("item", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("global K", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("global Ksup", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("force Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mass matrix", "massMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Damping matrix", "DampMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements", "DispMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Velocity", "VeloMat", "", GH_ParamAccess.item);

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

            //Creation of matrices
            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildGlobalKsup(dof, globalK, supports, nodes);

            LA.Matrix<double> M = matrices.BuildGlobalM(dof, elements, true);
            LA.Matrix<double> globalMsup = matrices.BuildGlobalKsup(dof, M, supports, nodes);

            LA.Matrix<double> C = matrices.BuildC(M,globalKsup,0.05,0.1,100);
            LA.Matrix<double> f0 = matrices.BuildForceVector(loads, dof);
           
            //Usage of newmark

            double T = 5.0;
            double dt = 0.1;
            double beta = 1 / 6;
            double gamma = 1 / 2;

            LA.Matrix<double> d0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);
            LA.Matrix<double> v0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);


            Newmark(beta, gamma, dt, globalMsup, globalKsup, C, f0, d0,v0,T, out LA.Matrix<double> displacements, out LA.Matrix<double> velocities);

            //DA.SetData(0, item);
            DA.SetData(1, globalK);
            DA.SetData(2, globalKsup);
            DA.SetData(3, f0);
            DA.SetData(4, globalMsup);
            DA.SetData(5, C);
            DA.SetData(6, displacements);
            DA.SetData(7, velocities);


        }
        void Newmark(double beta, double gamma, double dt, LA.Matrix<double> M, LA.Matrix<double> K, LA.Matrix<double> C, 
           LA.Matrix<double> f0, LA.Matrix<double> d0, LA.Matrix<double> v0, double T, out LA.Matrix<double> displacements, 
           out LA.Matrix<double> velocities)
        {
            // d0 and v0 inputs are (dof, 1) matrices
            int dof = K.RowCount;
            var d = LA.Matrix<double>.Build.Dense(dof ,((int)(T / dt)), 0);
            var v = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);
            var a = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);
            LA.Matrix<double> fTime = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);


            d.SetSubMatrix(0,dof, 0, 1, d0);
            v.SetSubMatrix(0, dof, 0, 1 , v0);
            fTime.SetSubMatrix(0, dof, 0, 1 , f0);


            // Initial calculation
            LA.Matrix<double> Minv = M.Inverse();
            var a0 = Minv.Multiply(f0 - C.Multiply(v0) - K.Multiply(d0));
            a.SetSubMatrix(0,dof,0,1, a0);
            v.SetSubMatrix(0, dof, 0, 1, v0 + dt * (1 - gamma) * a0);

            for (int n = 0; n < d.ColumnCount-1; n++)
            {
                // predictor step
                var dPred = d.SubMatrix(0,dof, n, 1) + dt * v.SubMatrix(0, dof, n, 1) + 0.5*(1 - 2*beta)*Math.Pow(dt, 2)*a.SubMatrix(0, dof, n, 1);
                var vPred = v.SubMatrix(0, dof, n, 1) + (1 - gamma) * dt * a.SubMatrix(0, dof, n, 1);

                // solution step
                // if force is a function of time, set F_n+1 to updated value (not f0)
                fTime.SetSubMatrix(0,dof,n+1,1, f0);
                var fPrime = fTime.SubMatrix(0,dof, n+1, 1 ) - C.Multiply(vPred) - K.Multiply(dPred);
                var Mprime = M + gamma * dt * C + beta * Math.Pow(dt, 2) * K;
                LA.Matrix<double> MprimeInv = Mprime.Inverse();
                a.SetSubMatrix(0,n + 1, MprimeInv.Multiply(fPrime));

                // connector step
                d.SetSubMatrix(0, dof, n+1,1, dPred + beta * Math.Pow(dt, 2) * a.SubMatrix(0,dof,n+1,1));
                v.SetSubMatrix(0, dof, n+1,1, vPred + gamma * dt * a.SubMatrix(0,dof,n+1,1));
            }
            velocities = v;
            displacements = d;
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