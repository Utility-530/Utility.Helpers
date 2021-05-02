using System;
using System.IO;
using System.Linq;

namespace UtilityHelper
{
    public static class FileInfoHelper
    {
        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        public static Uri CreateUri(string relativePath, string assemblyName)
        {
            var uri = new Uri($"pack://application:,,,{PrependForwardSlash(assemblyName)};component{PrependForwardSlash(relativePath)}");
            return uri;

            string PrependForwardSlash(string path) => path.First() == '/' ? path : "/" + path;
        }

        public static FileInfo AppendToFileName(FileInfo fileInfo, string appendage)
        {
            string file = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            string path = fileInfo.FullName.Replace(file, file + appendage);
            return new FileInfo(path);
        }

        public static FileInfo Combine(this DirectoryInfo directoryInfo, FileInfo fileInfo)
        {
            return new FileInfo(Path.Combine(directoryInfo.FullName, fileInfo.Name));
        }

        public static FileInfo Combine(this DirectoryInfo directoryInfo, string fileName)
        {
            return new FileInfo(Path.Combine(directoryInfo.FullName, fileName));
        }

        public static FileInfo FindNextAvailable(this DirectoryInfo directoryInfo, string fileName)
        {
            int length = Path.GetFileNameWithoutExtension(fileName).Length, i = 0;

            while (new FileInfo(Path.Combine(directoryInfo.FullName, fileName)).Exists)
            {
                fileName = fileName.Remove(length) + i++ + Path.GetExtension(fileName);
            }

            return new FileInfo(Path.Combine(directoryInfo.FullName, fileName));
        }

        public static void DeleteDirectories(DirectoryInfo directory)
        {
            foreach (var directoryInfo in directory.GetDirectories())
            {
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    fileInfo.Delete();
                }

                directoryInfo.Delete();
            }
        }
    }
}