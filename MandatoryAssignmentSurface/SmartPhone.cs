using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MandatoryAssignmentSurface
{
    

    class SmartPhone
    {
        public List<object> images { get; private set; }
        private String name = "";

        public SmartPhone(String name)
        {
            this.name = name;
        }

    }
}