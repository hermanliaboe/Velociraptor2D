using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace FEM
{
    public class FEMInfo : GH_AssemblyInfo
    {
        public override string Name => "FEM";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("885d0b84-699d-4935-b0d2-158dc76c073b");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}