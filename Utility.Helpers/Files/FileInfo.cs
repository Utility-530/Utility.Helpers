using System;
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

        public static void OverWrite(this FileInfo fileInfo, string contents)
        {
            using FileStream fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using MemoryStream memoryStream = contents.ToMemoryStream();
            fileStream.SetLength(0);
            memoryStream.CopyTo(fileStream);
        }

        public static string Read(this FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                using FileStream fs = fileInfo.OpenRead();
                using StreamReader reader = new(fs);
                string fileContent = reader.ReadToEnd();
                return fileContent;
            }
            else
            {
                throw new Exception($"The file {fileInfo.FullName} does not exist.");
            }
        }
    }
}