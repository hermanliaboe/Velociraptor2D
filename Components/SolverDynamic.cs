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
using MathNet.Numerics.LinearAlgebra;

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
            pManager.AddNumberParameter("Time Step", "", "Time step for the Newmark method. Default 0.01", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Beta", "", "Beta value for the Newmark method. Default 1/4 (average acceleration)", GH_ParamAccess.item, 1.0/4.0);
            pManager.AddNumberParameter("Gamma", "", "Gamme value for the Newmark method. Default 1/2 (average acceleration)", GH_ParamAccess.item, 1.0 / 2.0);
            pManager.AddNumberParameter("Time", "", "Run time for the Newmark method. Default 5 seconds", GH_ParamAccess.item, 5.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Global Stiffness Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Stiffness Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Applied Force Vector", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Lumped Mass Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Lumped Mass Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Consistent Mass Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Consistent Mass Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Damping Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Damping Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Velocity", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Nodal Forces", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Natural Frequencies [Hz]", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Classes.Assembly model = new Classes.Assembly();
            double T = 5.0;
            double dt = 0.01;
            double beta = 1.0 / 4.0;
            double gamma = 1.0 / 2.0;
            DA.GetData(0, ref model);
            DA.GetData(1, ref dt);
            DA.GetData(2, ref beta);
            DA.GetData(3, ref gamma);
            DA.GetData(4, ref T);

            List<Load> loads = model.LoadList;
            List<BeamElement> elements = model.BeamList;
            List<Support> supports = model.SupportList;
            List<Node> nodes = model.NodeList;
            int dof = nodes.Count * 3;

            //Creation of matrices
            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildSupMat(dof, globalK, supports, nodes);

            LA.Matrix<double> globalLumpedM = matrices.BuildGlobalM(dof, elements, true);
            LA.Matrix<double> globalConsistentM = matrices.BuildGlobalM(dof, elements, false);
            LA.Matrix<double> globalLumpedMsup = matrices.BuildSupMat(dof, globalLumpedM, supports, nodes);   
            LA.Matrix<double> globalConsistentMsup = matrices.BuildSupMat(dof, globalConsistentM, supports, nodes);

            LA.Matrix<double> globalC = matrices.BuildC(globalLumpedM,globalKsup,0.05,0.1,100.0);
            LA.Matrix<double> supC = matrices.BuildSupMat(dof, globalC, supports, nodes);
            LA.Matrix<double> f0 = matrices.BuildForceVector(loads, dof);
           
            //Usage of newmark

       

            LA.Matrix<double> d0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);
            LA.Matrix<double> v0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);

            Newmark(beta, gamma, dt, globalConsistentMsup, globalKsup, supC, f0, d0,v0,T, out LA.Matrix<double> displacements, out LA.Matrix<double> velocities);



            //Eigenvalues 
            var eigs = EigenValues(globalKsup, globalConsistentMsup);
            var natFreq = LA.Matrix<double>.Build.Dense(1, eigs.ColumnCount, 0);
            // Sort the natFreq matrix from smallest to largest using an array


            for (int i = 0; i < eigs.ColumnCount; i++)
            {
                natFreq[0, i] = Math.Sqrt(eigs[0, i]);
            }

            var sortedNatFreqArray = natFreq.ToRowMajorArray();
            Array.Sort(sortedNatFreqArray);

            // Convert the sorted array back into a MathNet.Numerics.LinearAlgebra.Matrix<double>
            var sortedNatFreqMatrix = Matrix<double>.Build.Dense(1, sortedNatFreqArray.Length);
            sortedNatFreqMatrix.SetRow(0, sortedNatFreqArray);



            // Creation of RhinoMatrix
            Rhino.Geometry.Matrix rhinoMatrixK = CreateRhinoMatrix(globalK);
            Rhino.Geometry.Matrix rhinoMatrixKred = CreateRhinoMatrix(globalKsup);
            Rhino.Geometry.Matrix rhinoMatrixAppF = CreateRhinoMatrix(f0);
            Rhino.Geometry.Matrix rhinoMatrixLumpedM = CreateRhinoMatrix(globalLumpedM);
            Rhino.Geometry.Matrix rhinoMatrixLumpedMred = CreateRhinoMatrix(globalLumpedMsup);
            Rhino.Geometry.Matrix rhinoMatrixConsistentM = CreateRhinoMatrix(globalConsistentM);
            Rhino.Geometry.Matrix rhinoMatrixConsistentMred = CreateRhinoMatrix(globalConsistentMsup);
            Rhino.Geometry.Matrix rhinoMatrixC = CreateRhinoMatrix(globalC);
            Rhino.Geometry.Matrix rhinoMatrixCred = CreateRhinoMatrix(supC);
            Rhino.Geometry.Matrix rhinoMatrixSortedNatFreq = CreateRhinoMatrix(sortedNatFreqMatrix);



            //Nodal forces incoming:
            GetBeamForces(displacements, elements, T, dt, out LA.Matrix<double> beamForces);

            DA.SetData(0, rhinoMatrixK);
            DA.SetData(1, rhinoMatrixKred);
            DA.SetData(2, rhinoMatrixAppF);
            DA.SetData(3, rhinoMatrixLumpedM);
            DA.SetData(4, rhinoMatrixLumpedMred);
            DA.SetData(5, rhinoMatrixConsistentM);
            DA.SetData(6, rhinoMatrixConsistentMred);
            DA.SetData(7, rhinoMatrixC);
            DA.SetData(8, rhinoMatrixCred);
            DA.SetData(9, displacements);
            DA.SetData(10, velocities);
            DA.SetData(11, beamForces);
            DA.SetData(12, rhinoMatrixSortedNatFreq);
        }

        //Newmark function
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
            LA.Matrix<double> mInv = M.Inverse();
            var a0 = mInv.Multiply(f0 - C.Multiply(v0) - K.Multiply(d0));
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

        public Rhino.Geometry.Matrix CreateRhinoMatrix(LA.Matrix<double> matrix)
        {
            Rhino.Geometry.Matrix rhinoMatrix = new Rhino.Geometry.Matrix(matrix.RowCount, matrix.ColumnCount);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    rhinoMatrix[i, j] = matrix[i, j];
                }
            }
            return rhinoMatrix;
        }

        public LA.Matrix<double> EigenValues(LA.Matrix<double> K, LA.Matrix<double> M)
        {
            /*
             * int dof, List<Support> supports, List<Node> nodes
            for (int j = 0; j < K.RowCount; j++)
            {
                LA.Vector<double> Ms = M.Row(j);
                LA.Vector<double> Ks = K.Row(j);

                double Mss = Ms.Sum();
                double Kss = Ks.Sum();

                if (Mss== 1)
                {
                    K = K.RemoveRow(j);
                    K = K.RemoveColumn(j);
                    M = M.RemoveRow(j);
                    M = M.RemoveColumn(j);
                }
            }
            
            
            LA.Matrix<double> supMatrix = K.Clone();
            foreach (Support support in supports)
            {
                foreach (Node node in nodes)
                {

                    if (support.Point == node.Point)
                    {
                        LA.Matrix<double> col = LA.Matrix<double>.Build.Dense(dof, 1, 0);
                        LA.Matrix<double> row = LA.Matrix<double>.Build.Dense(1, dof, 0);
                        int idN = node.GlobalID;


                        if (support.Tx == true)
                        {
                            supMatrix.SetSubMatrix(idN * 3, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 3, col);
                            supMatrix[idN * 3, idN * 3] = 1;
                        }
                        if (support.Tz == true)
                        {
                            supMatrix.SetSubMatrix(idN * 3 + 1, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 3 + 1, col);
                            supMatrix[idN * 3 + 1, idN * 3 + 1] = 1;
                        }
                        if (support.Ry == true)
                        {
                            supMatrix.SetSubMatrix(idN * 3 + 2, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 3 + 2, col);
                            supMatrix[idN * 3 + 2, idN * 3 + 2] = 1;
                        }
                    }
                }
            }
            */
            // Solve the generalized eigenvalue problem
            var factorizedM = M.QR();
            var factorizedK = factorizedM.Solve(K);
            var evd = factorizedK.Evd(LA.Symmetricity.Asymmetric);

            // Extract the eigenvalues and eigenvectors
            double[] ev = evd.EigenValues.Select(x => x.Real).ToArray();
            LA.Matrix<double> V = evd.EigenVectors;
            var W = LA.Matrix<double>.Build.Dense(1, ev.Length);
            int i = 0;
            foreach (double w in ev)
            {
                W[0, i] = w;
                i++;
            }
            return W;
        }

        void GetBeamForces(LA.Matrix<double> displacements, List<BeamElement> elements, double T, double dt, out LA.Matrix<double> beamForces0)
        {
            int dof = 6;
            int i = 3;
            int j = 0;
            int r = 0;
            LA.Matrix<double> beamDisp = LA.Matrix<double>.Build.Dense(dof , elements.Count * ((int)(T / dt)));

            for (int c = 0; c < displacements.ColumnCount ;c++)
            {
                
                foreach (BeamElement beam in elements)
                {
                    LA.Matrix<double> beamDispEl = LA.Matrix<double>.Build.Dense(dof, 1);

                    int startId = beam.StartNode.GlobalID;
                    beamDispEl[0, 0] = displacements[startId * i, c];
                    beamDispEl[1, 0] = displacements[startId * i + 1, c];
                    beamDispEl[2, 0] = displacements[startId * i + 2, c];

                    int endId = beam.EndNode.GlobalID;
                    beamDispEl[3, 0] = displacements[endId * i, c];
                    beamDispEl[4, 0] = displacements[endId * i + 1, c];
                    beamDispEl[5, 0] = displacements[endId * i + 2, c];

                    Matrices vecT = new Matrices();
                    LA.Matrix<double> beamdispT = vecT.TransformVector(beamDispEl, beam.StartNode.Point.X, beam.EndNode.Point.X, beam.StartNode.Point.Z, beam.EndNode.Point.Z, beam.Length);
                    beamDisp.SetSubMatrix(0, dof, j, 1, beam.kel.Multiply(beamDispEl));
                    j++;
                }
            }
            beamForces0 = beamDisp;
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