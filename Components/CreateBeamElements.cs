using System;
using System.Collections.Generic;
using FEM.Classes;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components
{
    public class CreateBeamElements : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Beam class.
        /// </summary>
        public CreateBeamElements()
          : base("Beam", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "ls", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements","els","", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List <Line> lines = new List<Line>();
            DA.GetData(0, ref lines);

            List<Node> nodes = new List<Node> ();
            List<BeamElement> beams = new List<BeamElement>();
            
            int idc = 0;
            foreach (Line line in lines)
            {
                Point3d stPt = line.From;
                Point3d ePt = line.To;
                BeamElement element = new BeamElement();

                foreach (Node node in nodes)
                {
                    if (stPt.CompareTo(node.point) == 0)
                    {
                        element.startNode = node;
                    }
                    else 
                    {
                        Node stNode = new Node(0, idc, stPt);
                        element.startNode = stNode;
                        idc++;
                    }
                    if (ePt.CompareTo(node.point) == 0)
                    {
                        element.endNode = node;
                    }
                    else
                    {
                        Node eNode = new Node(0, idc, ePt);
                        element.endNode = eNode;
                        idc++;
                    }
                }
                beams.Add(element);
            }

            DA.SetData(0, beams);

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
            get { return new Guid("36B2609B-0264-4FD9-AFE9-631B3E6CACB5"); }
        }
    }
}