﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LA = MathNet.Numerics.LinearAlgebra;

namespace FEM.Classes
{
    internal class Matrices
    {
        public LA.Matrix<double> globalK;
        public LA.Matrix<double> globalM;
        public LA.Matrix<double> globalC;
        public LA.Matrix<double> globalN;
        public LA.Matrix<double> globalF;

        public Matrices(int dof, List<BeamElement> elements, double E, double A, double I)
        {
            this.globalK = BuildGlobalK(dof, elements, E, A, I);

        }

        public Matrices()
        {

        }

        public LA.Matrix<double> BuildGlobalK(int dof, List<BeamElement> elements, double E, double A, double I)
        {

            LA.Matrix<double> globalK = LA.Matrix<double>.Build.Dense(dof, dof);
            foreach (var element in elements)
            {
                int inod = element.startNode.globalID;
                int jnod = element.endNode.globalID;
                LA.Matrix<double> ke = GetKel(element, E, A, I);
                LA.Matrix<double> globalKcopy1 = globalK.Clone();
                globalKcopy1.SetSubMatrix(inod*3,jnod*3,ke.SubMatrix(0,3,0,3));

                /* shit from console app
                 int inod = 3;
            int jnod = 5;
            LA.Matrix<double> globalK = LA.Matrix<double>.Build.Dense(12,12,1);
            LA.Matrix<double> ke11 = LA.Matrix<double>.Build.Dense(3, 3, 1);
            LA.Matrix<double> submat = globalK.SubMatrix(inod, 3, jnod, 3);
            globalK.SetSubMatrix(inod, jnod, submat + ke11);
                 */

            }

            return globalK;
        }

        public LA.Matrix<double> GetKel(BeamElement beam, double E, double A, double I)
        {
            int nNode = 2;  //how many nodes per element


            //gets length of element

            Node startNode = beam.startNode;
            double z1 = startNode.point.Z;
            double x1 = startNode.point.X;

            Node endNode = beam.endNode;
            double z2 = startNode.point.Z;
            double x2 = startNode.point.X;

            double l = Math.Sqrt(Math.Pow(z2 - z1, 2) + Math.Pow(x2 - x1, 2));

            LA.Matrix<double> kEl = LA.Matrix<double>.Build.Dense(nNode * 3, nNode * 3);

            double ealA = (E * A) / l;
            double ealB = 12 * (E * I) / Math.Pow(l, 3);
            double ealC = 6 * (E * I) / Math.Pow(l, 2);
            double ealD = 4 * (E * I) / l;
            double ealE = 2 * (E * I) / l;

            kEl[0, 1] = ealA; kEl[1, 1] = 0; kEl[2, 1] = 0; kEl[3, 1] = -ealA; kEl[4, 1] = 0; kEl[5, 1] = 0;
            kEl[0, 2] = 0; kEl[1, 2] = ealB; kEl[2, 2] = -ealC; kEl[3, 2] = 0; kEl[4, 2] = -ealB; kEl[5, 2] = -ealC;
            kEl[0, 3] = 0; kEl[1, 3] = -ealC; kEl[2, 3] = ealD; kEl[3, 3] = 0; kEl[4, 3] = ealC; kEl[5, 3] = ealE;
            kEl[0, 4] = -ealA; kEl[1, 4] = 0; kEl[2, 4] = 0; kEl[3, 4] = ealA; kEl[4, 4] = 0; kEl[5, 4] = 0;
            kEl[0, 5] = 0; kEl[1, 5] = -ealB; kEl[2, 5] = ealC; kEl[3, 5] = 0; kEl[4, 5] = ealB; kEl[5, 5] = ealC;
            kEl[0, 6] = 0; kEl[1, 6] = -ealC; kEl[2, 6] = ealD; kEl[3, 6] = 0; kEl[4, 6] = ealC; kEl[5, 6] = ealE;


            LA.Matrix<double> tM = LA.Matrix<double>.Build.Dense(nNode * 3, nNode * 3); 

            double c = Math.Cos((x2 - x1) / l);
            double s = Math.Sin((z2 - z1) / l);
            tM[0, 1] = c; tM[1, 1] = s; tM[2, 1] = 0; tM[3, 1] = 0; tM[4, 1] = 0; tM[5, 1] = 0;
            tM[0, 2] = -s; tM[1, 2] = c; tM[2, 2] = 0; tM[3, 2] = 0; tM[4, 2] = 0; tM[5, 2] = 0;
            tM[0, 3] = 0; tM[1, 3] = 0; tM[2, 3] = 1; tM[3, 3] = 0; tM[4, 3] = 0; tM[5, 3] = 0;
            tM[0, 4] = 0; tM[1, 4] = 0; tM[2, 4] = 0; tM[3, 4] = c; tM[4, 4] = s; tM[5, 4] = 0;
            tM[0, 5] = 0; tM[1, 5] = 0; tM[2, 5] = 0; tM[3, 5] = -s; tM[4, 5] = c; tM[5, 5] = 0;
            tM[0, 6] = 0; tM[1, 6] = 0; tM[2, 6] = 0; tM[3, 6] = 0; tM[4, 6] = 0; tM[5, 6] = 1; 
            
            LA.Matrix<double> tMT = tM.Transpose(); 
            LA.Matrix<double> kElG = tMT.Multiply(kEl).Multiply(tM); 
            
            return kElG;
        }
    }
}
