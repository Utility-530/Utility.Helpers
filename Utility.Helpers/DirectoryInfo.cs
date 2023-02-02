using System.IO;

namespace Utility.Helpers
{
    public static class DirectoryInfo
    {
        public static FileInfo Combine(this System.IO.DirectoryInfo directoryInfo, FileInfo fileInfo)
        {
            return new FileInfo(Path.Combine(directoryInfo.FullName, fileInfo.Name));
        }

        public static FileInfo Combine(this System.IO.DirectoryInfo directoryInfo, string fileName)
        {
            return new FileInfo(Path.Combine(directoryInfo.FullName, fileName));
        }

        public static FileInfo FindNextAvailable(this System.IO.DirectoryInfo directoryInfo, string fileName)
        {
            int length = Path.GetFileNameWithoutExtension(fileName).Length, i = 0;

            while (new FileInfo(Path.Combine(directoryInfo.FullName, fileName)).Exists)
            {
                fileName = fileName.Remove(length) + i++ + Path.GetExtension(fileName);
            }

            return new FileInfo(Path.Combine(directoryInfo.FullName, fileName));
        }

        public static void DeleteDirectories(this System.IO.DirectoryInfo directory)
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