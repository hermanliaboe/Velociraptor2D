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
            pManager.AddGenericParameter("global K", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("global Ksup", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("force Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter(" Lumped Mass matrix", "LumpMassMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("consistent Mass matrix", "ConsMassMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Damping matrix", "DampMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements", "DispMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Velocity", "VeloMat", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("mass matric C# matrix","lumpedMass","",GH_ParamAccess.item);
            pManager.AddGenericParameter("consistent mass matrix C# matrix", "consMass", "", GH_ParamAccess.item);
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
            LA.Matrix<double> globalKsup = matrices.BuildSupMat(dof, globalK, supports, nodes);

            LA.Matrix<double> M = matrices.BuildGlobalM(dof, elements, true);
            LA.Matrix<double> consistentM = matrices.BuildGlobalM(dof, elements, false);
            LA.Matrix<double> globalMsup = matrices.BuildSupMat(dof, M, supports, nodes);   
            LA.Matrix<double> globalMsupC = matrices.BuildSupMat(dof, consistentM, supports, nodes);

            LA.Matrix<double> C = matrices.BuildC(M,globalKsup,0.05,0.1,100);
            LA.Matrix<double> supC = matrices.BuildSupMat(dof, C, supports, nodes);
            LA.Matrix<double> f0 = matrices.BuildForceVector(loads, dof);
           
            //Usage of newmark

            double T = 5.0;
            double dt = 0.01;
            double beta = 1 / 4;
            double gamma = 1 / 2;

            LA.Matrix<double> d0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);
            LA.Matrix<double> v0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);


            //Newmark(beta, gamma, dt, globalMsupC, globalKsup, supC, f0, d0,v0,T, out LA.Matrix<double> displacements, out LA.Matrix<double> velocities);
            Newmark(beta, gamma, dt, consistentM, globalKsup, C, f0, d0, v0, T, out LA.Matrix<double> displacements, out LA.Matrix<double> velocities);


            Rhino.Geometry.Matrix rhinoMatrixK = new Rhino.Geometry.Matrix(dof, dof);
            for (int i = 0; i < globalKsup.RowCount; i++)
            {
                for (int j = 0; j < globalKsup.ColumnCount; j++)
                {
                    rhinoMatrixK[i, j] = globalKsup[i, j];
                }
            }
            Rhino.Geometry.Matrix rhinoMatrixM = new Rhino.Geometry.Matrix(dof, dof);
            for (int i = 0; i < globalMsup.RowCount; i++)
            {
                for (int j = 0; j < globalMsup.ColumnCount; j++)
                {
                    rhinoMatrixM[i, j] = globalMsup[i, j];
                }
            }
            Rhino.Geometry.Matrix rhinoMatrixMcons = new Rhino.Geometry.Matrix(dof, dof);
            for (int i = 0; i < consistentM.RowCount; i++)
            {
                for (int j = 0; j < consistentM.ColumnCount; j++)
                {
                    rhinoMatrixMcons[i, j] = consistentM[i, j];
                }
            }
            Rhino.Geometry.Matrix rhinoMatrixC = new Rhino.Geometry.Matrix(dof, dof);
            for (int i = 0; i < C.RowCount; i++)
            {
                for (int j = 0; j < C.ColumnCount; j++)
                {
                    rhinoMatrixC[i, j] = C[i, j];
                }
            }

      
            DA.SetData(0, globalK);
            DA.SetData(1, rhinoMatrixK);
            DA.SetData(2, f0);
            DA.SetData(3, rhinoMatrixM);
            DA.SetData(4, rhinoMatrixMcons);
            DA.SetData(5, rhinoMatrixC);
            DA.SetData(6, displacements);
            DA.SetData(7, velocities);
            DA.SetData(8, globalMsup);
            DA.SetData(9, globalMsupC);
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

            for (int n = 0; n < d.ColumnCount-1; n++)
            {
                // predictor step
                var dPred = d.SubMatrix(0,dof, n, 1) + dt * v.SubMatrix(0, dof, n, 1) + 0.5*(1 - 2*beta)*Math.Pow(dt, 2)*a.SubMatrix(0, dof, n, 1);
                var vPred = v.SubMatrix(0, dof, n, 1) + (1 - gamma) * dt * a.SubMatrix(0, dof, n, 1);

                // solution step
                // if force is a function of time, set F_n+1 to updated value (not f0)
                //fTime.SetSubMatrix(0,dof,n+1,1, fZeros);
                var fPrime = fTime.SubMatrix(0,dof, n+1, 1) - C.Multiply(vPred) - K.Multiply(dPred);
                var mPrime = M + gamma * dt * C + beta * Math.Pow(dt, 2) * K;
                LA.Matrix<double> mPrimeInv = mPrime.Inverse();
                a.SetSubMatrix(0,n + 1, mPrimeInv.Multiply(fPrime));

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