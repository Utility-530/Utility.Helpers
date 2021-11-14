using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utility
{
    public class DirectoryHelper
    {
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
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
                foreach (DirectoryInfo subdir in dirs)
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

            return directory.GetDirectories("netcoreapp*") is {} subDirs && subDirs.Any() ? 
                subDirs.OrderBy(a => a).Last().FullName : 
                directory.FullName;
        }

        public static DirectoryInfo GetBuildFolderDirectoryInfo(string projectFilePath)
        {
            return (GetDirectories("Debug") ?? throw new Exception("Unexpected null directory")).Select(dir => (order: 0, dir))
                 .Concat((GetDirectories("Release") ?? throw new Exception("Unexpected null directory")).Select(dir => (order: 1, dir)))
                 .OrderBy(a => a.order)
                 .Last()
                 .dir;

            IEnumerable<DirectoryInfo>? GetDirectories(string folderName) =>
                Directory
                    .GetParent(projectFilePath)
                    ?.GetDirectories("bin\\" + folderName, SearchOption.AllDirectories);                 
        }
    }
}
