using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UtilityHelper
{
    public class DirectoryHelper
    {
        public static string GetProjectPath()
        {
            return System.IO.Directory.GetParent(
                     System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
        }

        public static string GetSolutionPath()
        {
            return System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        }

        public static string GetCurrentExecutingDirectory()
        {
            string filePath = new System.Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
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
                    if (!System.IO.Directory.Exists(pathParts[i]))
                    {
                        System.IO.Directory.CreateDirectory(pathParts[i]);
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
        //System.IO.Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories)
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
                    tmp = System.IO.Directory.GetFiles(rootFolderPath, fileSearchPattern);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                tmp = System.IO.Directory.GetDirectories(rootFolderPath);
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

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            System.IO.DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                try
                {
                    file.CopyTo(temppath, true);
                }
                catch (Exception)
                {

                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (System.IO.DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static IEnumerable<string> SelectRemovables()
        {
            return from d in DriveInfo.GetDrives()
                   where d.DriveType == DriveType.Removable
                   select FilterUpperLetters(d.Name).Single().ToString();

            static string FilterUpperLetters(string myString)
                => new string(myString.Where(c => char.IsLetter(c) && char.IsUpper(c)).ToArray());
        }

        public static bool IsDirectoryFile(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                var fileAttr = File.GetAttributes(path);
                return fileAttr.HasFlag(FileAttributes.Directory);
            }

            throw new IOException("File or directory does not exist");
        }

        /// <summary>
        /// Gets the path of the directory containing the build files (e.g .dll) for a netcore application.
        /// </summary>
        /// <param name="projectFilePath"></param>
        /// <returns></returns>
        public static string GetSourceDirectory(string projectFilePath)
        {
            var directory = GetBuildFolderDirectoryInfo(projectFilePath);

            return directory.GetDirectories("netcoreapp*") is { } subDirs && subDirs.Any() ?
                subDirs.OrderBy(a => a).Last().FullName :
                directory.FullName;
        }

        public static System.IO.DirectoryInfo GetBuildFolderDirectoryInfo(string projectFilePath)
        {
            return GetDirectories("Debug").Select(dir => (order: 0, dir))
                 .Concat(GetDirectories("Release").Select(dir => (order: 1, dir)))
                 .OrderBy(a => a.order)
                 .Last()
                 .dir;

            IEnumerable<System.IO.DirectoryInfo> GetDirectories(string folderName) =>
                Directory
                    .GetParent(projectFilePath)
                    .GetDirectories("bin\\" + folderName, SearchOption.AllDirectories);
        }
    }
}