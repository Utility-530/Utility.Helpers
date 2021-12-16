using System;

namespace UtilityHelper
{
    public class PathHelper
    {
        public static bool IsValidPath(string path, bool allowRelativePaths = false)
        {
            try
            {
                return allowRelativePaths ?
                    System.IO.Path.IsPathRooted(path) :
                    string.IsNullOrEmpty(System.IO.Path.GetPathRoot(path)?.Trim('\\', '/')) == false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}