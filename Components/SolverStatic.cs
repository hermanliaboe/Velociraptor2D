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
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Numerics;
using Grasshopper.Kernel.Types.Transforms;
using static Rhino.Render.TextureGraphInfo;
using System.Xml.Linq;
using MathNet.Numerics.LinearAlgebra;

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
            pManager.AddGenericParameter("Assembly","Assemb.","",GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "Scale", "", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Item","item","item",GH_ParamAccess.item);
            pManager.AddGenericParameter("Global K","","",GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Ksup", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements List", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Displacements Node z", "", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("New lines", "lines", "", GH_ParamAccess.list);
             pManager.AddGenericParameter("Beam Forces","","",GH_ParamAccess.item);
            pManager.AddMatrixParameter("Beam Forces RM", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Nodal Forces RM", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            Classes.Assembly model = new Classes.Assembly();
            double scale = 0.0;

            DA.GetData(0, ref model);
            DA.GetData(1, ref scale);

            //Define input
            List<Load> loads = model.LoadList;
            List<BeamElement> elements = model.BeamList;
            List<Support> supports = model.SupportList;
            List<Node> nodes = model.NodeList;
       
            //Building matrices
            int dof = model.NodeList.Count*3;
            
            Matrices matrices = new Matrices();
            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildGlobalKsup(dof, globalK, supports, nodes);
            LA.Matrix<double> forceVec = matrices.BuildForceVector(loads, dof);
            LA.Matrix<double> displacements = globalKsup.Solve(forceVec);
            // LA.Matrix<double> connectivityMatrix = matrices.CalculateConnectivityMatrix(nodes, elements);


            //Calculation forces
            GetBeamForces(displacements, elements, out LA.Matrix<double> beamForces);

            //CalculateNodalForces(globalK, displacements, beamForces, connectivityMatrix);


            LA.Matrix<double> nodeForces2 = globalK.Multiply(displacements);





            List<string> dispList = new List<string>();
            for (int i = 0; i < dof; i=i+3)
            {
                var nodeDisp = "{" + displacements[i, 0] + ", " + displacements[i + 1, 0] + ", " + displacements[i + 2, 0] + "}";
                dispList.Add(nodeDisp);
            }

            List<double> dispNode = new List<Double>();
            for (int i = 0; i < dof; i = i + 3)
            {
                var nodeDisp =  displacements[i + 1, 0];
                dispNode.Add(nodeDisp);
            }

            Rhino.Geometry.Matrix rhinoMatrix = new Rhino.Geometry.Matrix(dof, dof);
            for (int i = 0; i < globalKsup.RowCount; i++)
            {
                for (int j = 0; j < globalKsup.ColumnCount; j++)
                {
                    rhinoMatrix[i,j] = globalKsup[i,j];
                }
            }
            

            //Drawing new geometry
            List<NurbsCurve> lineList1 = new List<NurbsCurve>();
            getNewGeometry(scale, displacements, elements, out lineList1);

            Rhino.Geometry.Matrix beamForcesRM= CreateRhinoMatrix(beamForces);
            Rhino.Geometry.Matrix nodeForcesRM = CreateRhinoMatrix(nodeForces2);


            //DA.SetData(0, item);
            DA.SetData(1, globalK);
            DA.SetData(2, globalKsup);
            DA.SetData(3, forceVec);
            DA.SetData(4, displacements);
            DA.SetDataList(5, dispList);
            DA.SetDataList(6, dispNode);
            DA.SetDataList(7, lineList1);
            DA.SetData(8, beamForces);
            DA.SetData(9, beamForcesRM);
            DA.SetData(10, nodeForcesRM);


        }

        void GetBeamForces(LA.Matrix<double> displacements, List<BeamElement> elements, out LA.Matrix<double> beamForces)
        {

            int dof = 6;
            int i = 3;
            int j = 0;
            LA.Matrix<double> beamF = LA.Matrix<double>.Build.Dense(dof, elements.Count);

            foreach (BeamElement beam in elements)
            {
                LA.Matrix<double> beamDispEl = LA.Matrix<double>.Build.Dense(dof, 1);

                int startId = beam.StartNode.GlobalID;
                beamDispEl[0, 0] = displacements[startId * i, 0];
                beamDispEl[1, 0] = displacements[startId * i + 1, 0];
                beamDispEl[2, 0] = displacements[startId * i + 2, 0];

                int endId = beam.EndNode.GlobalID;
                beamDispEl[3, 0] = displacements[endId * i, 0];
                beamDispEl[4, 0] = displacements[endId * i + 1, 0];
                beamDispEl[5, 0] = displacements[endId * i + 2, 0];

                Matrices vecT = new Matrices();
                LA.Matrix<double> beamdispT = vecT.TransformVector(beamDispEl, beam.StartNode.Point.X, beam.EndNode.Point.X, beam.StartNode.Point.Z, beam.EndNode.Point.Z, beam.Length);
                LA.Matrix<double> bf = beam.kel.Multiply(beamdispT);
                beam.ForceList = vecT.GetForceList(bf);
                beamF.SetSubMatrix(0, dof, j, 1, bf);
                j++;
            }

            beamForces = beamF;

        }


            void getNewGeometry(double scale, LA.Matrix<double> displacements, List<BeamElement> beams, out List<NurbsCurve> lineList)
        {
            List<Line> linelist2 = new List<Line>();
            List<NurbsCurve> linelist3 = new List<NurbsCurve>();

            int i = 3;

            foreach (BeamElement beam in beams)
            {
                Vector3d v1 = new Vector3d(0, 0, 0);
                Vector3d v2 = new Vector3d(0, 0, 0);
                double scale1 = scale;
                double scale2 = scale;

                int startId = beam.StartNode.GlobalID;
                double X1 = beam.StartNode.Point.X;
                double Z1 = beam.StartNode.Point.Z;

                int endId = beam.EndNode.GlobalID;
                double X2 = beam.EndNode.Point.X;
                double Z2 = beam.EndNode.Point.Z;

                double x1 = displacements[startId * i,  0];
                double z1 = displacements[startId * i +1,0];
                double r1 = displacements[startId * i + 2,0];
                Point3d sP = new Point3d(X1 + x1 * scale, 0, Z1 + z1 * scale);

                double x2 = displacements[endId * i,   0];
                double z2 = displacements[endId * i + 1, 0];
                double r2 = displacements[endId * i + 2, 0];
                Point3d eP = new Point3d(X2 + x2 * scale, 0,Z2 + z2 * scale);

                Vector3d yVec = new Vector3d(0, 1, 0);

                Vector3d sV1 = new Vector3d((X2 - X1), 0, Z2 - Z1);
                Vector3d sV2 = new Vector3d((X2 - X1), 0, Z2 - Z1);
                sV1.Rotate(r1 * scale1, yVec);
                sV2.Rotate(r2 * scale2, yVec);

                List<Point3d> pts = new List<Point3d>() { sP, eP };
                NurbsCurve nc = NurbsCurve.CreateHSpline(pts, sV1, sV2);
                linelist3.Add(nc);
            }
            lineList = linelist3;
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

        public Rhino.Geometry.Matrix CreateRhinoMatrixINT(LA.Matrix<int> matrix)
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
        /*
        public LA.Vector<double> CalculateNodalForces(LA.Matrix<double> globalStiffnessMatrix, LA.Matrix<double> nodalDisplacements, LA.Matrix<double> beamForces, LA.Matrix<double> connectivityMatrix)
        {
            int numNodes = connectivityMatrix.ColumnCount;
            LA.Matrix<double> nodalForces = Matrix<double>.Build.Dense(numNodes * 3,1);

            for (int i = 0; i < numNodes; i++)
            {
                for (int j = 0; j < numNodes; j++)
                {
                    double elementIndex = connectivityMatrix[i, j];

                    if (elementIndex != -1)
                    {
                        LA.Matrix<double> elementDisplacements = nodalDisplacements.submatrix(3 * i, 6);
                        Vector<double> elementForces = globalStiffnessMatrix.SubMatrix(3 * i, 6, 3 * i, 6).Multiply(elementDisplacements);
                        Vector<double> elementNodeForces = elementForces.SubVector(0, 3);

                        int[] elementNodeIndices = { i, i + 1 };
                        Vector<double> elementNodeDisplacements = Vector<double>.Build.Dense(6);
                        for (int k = 0; k < elementNodeIndices.Length; k++)
                        {
                            int nodeIndex = elementNodeIndices[k];
                            int rowIndex = 3 * nodeIndex;

                            for (int l = 0; l < 3; l++)
                            {
                                elementNodeDisplacements[3 * k + l] = nodalDisplacements[rowIndex + l];
                            }
                        }



                        Vector<double> elementNodeForcesWithMoments = globalStiffnessMatrix.SubMatrix(3 * i, 6, Convert.ToInt32(3 * elementIndex), 6).Multiply(elementNodeDisplacements);
                        elementNodeForcesWithMoments = LA.Vector<double>.Build.DenseOfArray(new double[] { elementNodeForcesWithMoments[0], elementNodeForcesWithMoments[1], elementNodeForcesWithMoments[2], 0, 0, 0 }).Add(elementForces.SubVector(0, 3));

                        for (int k = 0; k < elementNodeIndices.Length; k++)
                        {
                            int nodeIndex = elementNodeIndices[k];
                            int rowIndex = 3 * nodeIndex;
                            nodalForces[rowIndex] += elementNodeForcesWithMoments[3 * k];
                            nodalForces[rowIndex + 1] += elementNodeForcesWithMoments[3 * k + 1];
                            nodalForces[rowIndex + 2] += elementNodeForcesWithMoments[3 * k + 2];
                        }
                    }
                }
            }

            return nodalForces;
        }
        */
        























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