using System.IO;

namespace Utility.Helpers
{
    public static class FileInfoHelper
    {
        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream? stream = null;

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

        public static FileInfo AppendToFileName(this FileInfo fileInfo, string appendage)
        {
            string file = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            string path = fileInfo.FullName.Replace(file, file + appendage);
            return new FileInfo(path);
        }
    }
}