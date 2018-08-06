using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper
{
    public  class DirectoryNetFrameworkHelper
    {


        public static string GetProjectPath()
        {

            return Directory.GetParent(
                     Directory.GetCurrentDirectory()).Parent.FullName;
        }


        public static string GetSolutionPath()
        {
            return Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
      
        }

        public static string GetCurrentExecutingDirectory()
        {
            string filePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            return Path.GetDirectoryName(filePath);
        }


        //StackOverflow answered Jun 17 '16 at 23:03 williambq
        public static bool CreateDirectoryRecursively(string path)
        {
            try
            {
                string[] pathParts = path.Split('\\');
                for (var i = 0; i < pathParts.Length; i++)
                {
                    // Correct part for drive letters
                    if (i == 0 && pathParts[i].Contains(":"))
                    {
                        pathParts[i] = pathParts[i] + "\\";
                    } // Do not try to create last part if it has a period (is probably the file name)
                    else if (i == pathParts.Length - 1 && pathParts[i].Contains("."))
                    {
                        return true;
                    }
                    if (i > 0)
                    {
                        pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);
                    }
                    if (!Directory.Exists(pathParts[i]))
                    {
                        Directory.CreateDirectory(pathParts[i]);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // Get All files (in subdirectories)
        //Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories)
        //or 
        //https://stackoverflow.com/questions/2106877/is-there-a-faster-way-than-this-to-find-all-the-files-in-a-directory-and-all-sub/2107294#2107294
        public static IEnumerable<string> GetFileList(string fileSearchPattern, string rootFolderPath)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] tmp;
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                try
                {
                    tmp = Directory.GetFiles(rootFolderPath, fileSearchPattern);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                tmp = Directory.GetDirectories(rootFolderPath);
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }
            }
        }




        //public static string GetDirectoryOfAssembly<T>()
        //{
        //    string filePath = new Uri(typeof(T).Assembly.CodeBase).LocalPath;
        //    return Path.GetDirectoryName(filePath);
        //}

        //public static string GetDirectoryOfAssembly(Type t)
        //{
        //    string filePath = new Uri(Assembly.GetAssembly(t).CodeBase).LocalPath;
        //    return System.IO.Path.GetDirectoryName(filePath);
        //}
    }
}
