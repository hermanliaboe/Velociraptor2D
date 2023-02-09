﻿using FEM.Classes;
using GH_IO.Serialization;
using Rhino.Display;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
            LA.Matrix<double> globalK = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            LA.Matrix<double> testMat = GetKel(elements[0], E, A, I);

            foreach (var element in elements)
            {
                int nDof = 3;
                //Retrive element k from function
                LA.Matrix<double> ke = GetKel(element, E, A, I);

                //Get nodeID and *3 to get globalK placement
                int idS = element.startNode.globalID * nDof;
                int idE = element.endNode.globalID * nDof;

                //divide the element matrix into four matrices
                LA.Matrix<double> ke11 = ke.SubMatrix(0, nDof, 0, nDof);
                LA.Matrix<double> ke12 = ke.SubMatrix(0, nDof, nDof, nDof);
                LA.Matrix<double> ke21 = ke.SubMatrix(nDof, nDof, 0, nDof);
                LA.Matrix<double> ke22 = ke.SubMatrix(nDof, nDof, nDof, nDof);

                //Puts the four matrices into the correct place in globalK (yes, correct buddy)
                for (int r = 0; r < nDof; r++)
                {
                    for (int c = 0; c < nDof; c++)
                    {
                        globalK[idS + r, idS + c] = globalK[idS + r, idS + c] + ke11[r, c];
                        globalK[idS + r, idE + c] = globalK[idS + r, idE + c] + ke12[r, c];
                        globalK[idE + r, idS + c] = globalK[idE + r, idS + c] + ke21[r, c];
                        globalK[idE + r, idE + c] = globalK[idE + r, idE + c] + ke22[r, c];
                    }
                }
            }

            return testMat;
        }

        //Fuction to create element k. Locally first, then adjusted to global axis with T-matrix.  
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

            //Define standard k for two node beam element. 
            LA.Matrix<double> kEl = LA.Matrix<double>.Build.Dense(nNode * 3, nNode * 3, 0);

            double ealA = (E * A) / l;
            double ealB = 12 * (E * I) / Math.Pow(l, 3);
            double ealC = 6 * (E * I) / Math.Pow(l, 2);
            double ealD = 4 * (E * I) / l;
            double ealE = 2 * (E * I) / l;



            kEl[0, 0] = ealA; kEl[0, 1] = 0;    kEl[0, 2] = 0;     kEl[0, 3] = -ealA; kEl[0, 4] = 0;     kEl[0, 5] = 0;
            kEl[1, 0] = 0;    kEl[1, 1] = ealB; kEl[1, 2] = -ealC; kEl[1, 3] = 0;     kEl[1, 4] = -ealB; kEl[1, 5] = -ealC;
            kEl[2, 0] = 0;    kEl[2, 1] = -ealC;kEl[2, 2] = ealD;  kEl[2, 3] = 0;     kEl[2, 4] = ealC;  kEl[2, 5] = ealE;
            kEl[3, 0] = -ealA;kEl[3, 1] = 0;    kEl[3, 2] = 0;     kEl[3, 3] = ealA;  kEl[3, 4] = 0;     kEl[3, 5] = 0;
            kEl[4, 0] = 0;    kEl[4, 1] = -ealB;kEl[4, 2] = ealC;  kEl[4, 3] = 0;     kEl[4, 4] = ealB;  kEl[4, 5] = ealC;
            kEl[5, 0] = 0;    kEl[5, 1] = -ealC;kEl[5, 2] = ealD;  kEl[5, 3] = 0;     kEl[5, 4] = ealC;  kEl[5, 5] = ealE;

            //Creates T-matrix to adjust k element to global axis. 
            LA.Matrix<double> tM = LA.Matrix<double>.Build.Dense(nNode * 3, nNode * 3, 0);

            double c = (x2 - x1) / l;
            double s = (z2 - z1) / l;

            tM[0, 0] = c;  tM[0, 1] = s; tM[0, 2] = 0; tM[0, 3] = 0; tM[0, 4] = 0; tM[0, 5] = 0;
            tM[1, 0] = -s; tM[1, 1] = c; tM[1, 2] = 0; tM[1, 3] = 0; tM[1, 4] = 0; tM[1, 5] = 0;
            tM[2, 0] = 0;  tM[2, 1] = 0; tM[2, 2] = 1; tM[2, 3] = 0; tM[2, 4] = 0; tM[2, 5] = 0;
            tM[3, 0] = 0;  tM[3, 1] = 0; tM[3, 2] = 0; tM[3, 3] = c; tM[3, 4] = s; tM[3, 5] = 0;
            tM[4, 0] = 0;  tM[4, 1] = 0; tM[4, 2] = 0; tM[4, 3] = -s;tM[4, 4] = c; tM[4, 5] = 0;
            tM[5, 0] = 0;  tM[5, 1] = 0; tM[5, 2] = 0; tM[5, 3] = 0; tM[5, 4] = 0; tM[5, 5] = 1;

            LA.Matrix<double> tMT = tM.Transpose();
            LA.Matrix<double> kElGtemp = tMT.Multiply(kEl);
            LA.Matrix<double> kElG = kElGtemp.Multiply(tM);
            return kElG;
        }
        /*
        public LA.Matrix<double> BuildGlobalKsup(int dof, LA.Matrix<double> globalK, List<Support> Support, List<Node> Node)
        {
            foreach (Support support in Support)
            {
                foreach (Node node in Node)
                {
                    
                    if (support.point == node.point)
                    {
                        LA.Matrix<double> col = LA.Matrix<double>.Build.Dense(dof, 1, 0);
                        LA.Matrix<double> row = LA.Matrix<double>.Build.Dense(1, dof, 0);
                        int idN = node.globalID;

                        if (support.tz == true)
                        {
                            globalK.SetSubMatrix(idN, 0, row);
                            globalK.SetSubMatrix(0, idN, col);
                            globalK[idN, idN] = 1;
                        }
                        if (support.tx == true)
                        {
                            idN = idN + 1;
                            globalK.SetSubMatrix(idN, 0, row);
                            globalK.SetSubMatrix(0, idN, col);
                            globalK[idN, idN] = 1;
                        }
                        if (support.ry == true)
                        {
                            idN = idN + 2;
                            globalK.SetSubMatrix(idN, 0, row);
                            globalK.SetSubMatrix(0, idN, col);
                            globalK[idN, idN] = 1;
                        }
                    }
                }
            }
            LA.Matrix<double> globalKsup = globalK;
            return globalKsup;
        }
        */

    }
}