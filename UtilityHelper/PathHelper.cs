using System;
using System.IO;

namespace Utility
{
    public class PathHelper
    {
        public static bool IsValidPath(string path, bool allowRelativePaths = false)
        {
            try
            {
                return allowRelativePaths ?
                    Path.IsPathRooted(path) :
                    string.IsNullOrEmpty(Path.GetPathRoot(path)?.Trim('\\', '/')) == false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
