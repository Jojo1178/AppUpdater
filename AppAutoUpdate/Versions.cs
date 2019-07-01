using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AppAutoUpdate
{
    public static class Versions
    {

        private static bool IsValidPath(string path, bool allowRelativePaths = false)
        {
            bool isValid = true;

            try
            {
                string fullPath = Path.GetFullPath(path);

                if (allowRelativePaths)
                {
                    isValid = Path.IsPathRooted(path);
                }
                else
                {
                    string root = Path.GetPathRoot(path);
                    isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                }
            }
            catch (Exception ex)
            {
                isValid = false;
            }

            return isValid;
        }

        #region LocalVesion

        public static string LocalVersion(string path)
        {
            string lv = null;
            if (IsValidPath(path) && File.Exists(path))
            {
                string s = File.ReadAllText(path);
                if (ValidateFile(s))
                    lv = s;
                else
                    lv = null;
            }
            return lv;
        }

        public static bool ValidateFile(string contents)
        {
            bool val = false;
            if (!string.IsNullOrEmpty(contents))
            {
                string pattern = "^([0-9]*\\.){2}[0-9]*$";
                Regex re = new Regex(pattern);
                val = re.IsMatch(contents);
            }
            return val;
        }

        public static string CreateTargetLocation(string downloadToPath, string version)
        {
            if (!downloadToPath.EndsWith("\\")) // Give a trailing \ if there isn't one
                downloadToPath += "\\";

            string filePath = downloadToPath + version;

            DirectoryInfo newFolder = new DirectoryInfo(filePath);
            if (!newFolder.Exists)
                newFolder.Create();
            return filePath;
        }

        public static string CreateLocalVersionFile(string folderPath,
                     string fileName, string version)
        {
            if (!new DirectoryInfo(folderPath).Exists)
            {
                Directory.CreateDirectory(folderPath);
            }

            string path = folderPath + "\\" + fileName;

            FileInfo fi = new FileInfo(path);

            if (fi.Exists)
            {
                fi.Delete();
            }
            File.WriteAllText(path, version);
            return path;
        }


        #endregion

        #region RemoteVersion

        public static string RemoteVersion(string url)
        {
            string rv = "";

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string s = readStream.ReadToEnd()
                    .Replace("\n", String.Empty)
                    .Replace("\r", String.Empty)
                    .Replace("\t", String.Empty);
                response.Close();
                if (ValidateFile(s))
                {
                    rv = s;
                }
            }
            catch (Exception)
            {
                // Anything could have happened here but 
                // we don't want to stop the user
                // from using the application.
                rv = null;
            }
            return rv;
        }

        #endregion
    }
}
