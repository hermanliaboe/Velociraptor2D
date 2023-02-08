using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LA = MathNet.Numerics.LinearAlgebra;

namespace FEM.Classes
{
    internal class Matrices
    {
        public LA.Matrix<Double> globalK;
        public LA.Matrix<Double> globalM;
        public LA.Matrix<Double> globalC;
        public LA.Matrix<Double> globalN;
        public LA.Matrix<Double> globalF;

        public Matrices()
        {

        }

        private LA.Matrix<Double> buildGlobalK(int dof, List<BeamElement> elements, double E, double A)
        {

            LA.Matrix<Double> K = LA.Matrix<Double>.Build.Dense(dof, dof);

            return K;
        }
    }
}
