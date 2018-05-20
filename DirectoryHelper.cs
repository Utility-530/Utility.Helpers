using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public  class DirectoryHelper
    {


        public static string GetParentPathName()
        {

            return Directory.GetParent(
                     Directory.GetCurrentDirectory()).Parent.FullName;
        }

    }
}
