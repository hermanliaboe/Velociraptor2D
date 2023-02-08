﻿using System;
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
          : base("CreateBeamElements", "Nickname",
              "Line to element with two nodes",
              "Masters", "Model")
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
            DA.GetDataList(0, lines);

            List<BeamElement> beams = new List<BeamElement>();
            Dictionary<Point3d, Node> existingNodes = new Dictionary<Point3d, Node>();
            
            int idc = 0; // element global ID count
            int bidc = 0; // beam ID count
            foreach (Line line in lines)
            {
                Point3d stPt = line.From;
                Point3d ePt = line.To;
                BeamElement element = new BeamElement(bidc, line);
                bidc++;
                if (existingNodes.ContainsKey(stPt))
                {
                    element.startNode = existingNodes[stPt];
                }
                else
                {
                    Node sNode = new Node(0, idc, stPt);
                    existingNodes.Add(stPt, sNode);
                    element.startNode = sNode;
                    idc++;
                }
                if (existingNodes.ContainsKey(ePt))
                {
                    element.endNode = existingNodes[ePt];
                }
                else
                {
                    Node eNode = new Node(0, idc, ePt);
                    existingNodes.Add(ePt, eNode);
                    element.endNode = eNode;
                    idc++;
                }
                beams.Add(element);
            }

            DA.SetDataList(0, beams);

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