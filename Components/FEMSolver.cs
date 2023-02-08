using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FEM.Components
{
    public class FEMSolver : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FEMSolver()
          : base("Dynamic FEMSolver", "femmern",
            "FEM solver with Newmark method",
            "Masters", "FEM")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            //kode kode kode
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get input and deconstruct assembly into beams, loads and supports. 
            List<Assembly> assembly = new List<Assembly>();
            DA.GetData(0, ref assembly);


            beamList = assembly.beamList;
            supportList = assembly.supportList;
            loadList = assembly.loadList;

            //Input for element definition:
            double e = 1;   //E-moduls
            double a = 0.1; //Areal
            double v = 0.3; //Poisson-ratio, if needed

            int nDOF = 4;
            int nNodes = 


            //End

            //Assembly of global stiffness matrix






            //End

            //Applying BC to K
            //End

            //Calculate element stresses
            //End
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e9d8a089-9a52-4aaf-84a9-1fb4630d5e14");
    }
}