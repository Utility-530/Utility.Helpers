using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utility.Helpers
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

        public static string[] GetFilesInSubDirectories(IList<FileSystemInfo> value)
    => value
    .SelectMany(_ => Directory.GetFiles(_.FullName, "*.*", SearchOption.AllDirectories))
    .ToArray();

        public static string FileMap(string path)
            => DateTime
            .FromFileTime(Convert.ToInt64(Path.GetFileName(path).Replace(Path.GetExtension(path), "")))
            .ToShortDateString();

        public static string? FindPath(string endPath, int depth = 10)
        {
            string path = endPath.First() == '/' ? endPath.Remove(0, 1) : endPath;
            int i = 0;
            while (File.Exists(Path.GetFullPath(path)) == false && ++i < depth)
            {
                path = "../" + path;
            }
            if (i == depth)
                return null;
            return Path.GetFullPath(path);
        }

        public static Uri? FindUri(string assemblyName, string folder, string file)
        {
            if (FindPath(assemblyName + "/" + folder + "/" + file) is string uri)
                return new Uri(uri);
            return null;
        }

        public static string ChangeFilename(string filepath, string newFilename)
        {
            string dir = Path.GetDirectoryName(filepath);    // @"photo\myFolder"
            string ext = Path.GetExtension(filepath);        // @".jpg"
            return Path.Combine(dir, newFilename + ext); // @"photo\myFolder\image-resize.jpg"
        }
    }
}
