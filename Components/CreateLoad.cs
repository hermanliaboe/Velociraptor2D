using System;
using System.Collections.Generic;
using FEM.Classes;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components
{
    public class CreateLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Load class.
        /// </summary>
        public CreateLoad()
          : base("CreateLoad", "Nickname",
              "Creates a load at given point with given load vector.",
              "Masters", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point","pt","Attact point for force vector", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector", "vec", "Vector to decribe sice and angle of force", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Loads", "Loads", "List of loads", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d loadPt = new Point3d();
            Vector3d loadVec = new Vector3d();

            DA.GetData(0, ref loadPt);
            DA.GetData(1, ref loadVec);


            List<Load> loadList = new List<Load>();
            Load load = new Load(loadPt, loadVec);
            loadList.Add(load);

            DA.SetDataList(0, loadList);


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
            get { return new Guid("C9C77D55-6065-47CB-B39B-FFF857840766"); }
        }
    }
}