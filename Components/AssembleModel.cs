using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using FEM.Classes;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components
{
    public class AssembleModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AssembleModel class.
        /// </summary>
        public AssembleModel()
          : base("AssembleModel", "Nickname",
              "Description",
              "Masters", "FEM solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //trying some stuff here
            pManager.AddGenericParameter("Beams", "beams", "Input for all beams", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "sups", "Input for all supports", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "loads", "Input for all loads", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Modell", "modell", "Assembled modell", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BeamElement> beams = new List<BeamElement>();
            List<Support> supports = new List<Support>();
            List<Load> loads = new List<Load>();

            DA.GetData(0, ref beams);
            DA.GetData(1, ref supports);
            DA.GetData(2, ref loads);

            //Check for where the support is located, and if found the correct beam gets new BC

            foreach (Support sup in supports)
            {
                foreach (BeamElement b in beams) 
                {
                    Node startNode = b.startNode;
                    if (startNode.point == sup.point)
                    {
                        startNode.zBC = sup.tz;
                        startNode.xBC = sup.tx;
                        startNode.ry = sup.ry;

                    }

                    Node endNode = b.endNode;
                    if (endNode.point == sup.point)
                    {
                        endNode.zBC = sup.tz;
                        endNode.xBC = sup.tx;
                        endNode.ry = sup.ry;

                    }
                }                    
             }

            Assembly assembly = new Assembly(beams, supports, loads);
            DA.SetData(0, assembly);


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
            get { return new Guid("8F0FF416-72DD-4B79-9F02-1A30BCEE2AE9"); }
        }
    }
}