using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;

namespace MandatoryAssignmentSurface
{
    public class APicture : System.Windows.Controls.Image
    {
        public string owner;
        public string name;
        public string path;

        public APicture(string name,string path,string owner)
        {
            this.name = name;
            this.path = path;
            this.owner = owner;
        }

        override
        public string ToString()
        {
            return path;
        }
    }
}
