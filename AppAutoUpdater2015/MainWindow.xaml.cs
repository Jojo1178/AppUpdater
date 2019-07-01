using System;
using System.Windows;
using AppAutoUpdate;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Configuration;

namespace AppAutoUpdater2015
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CompareVersions();
        }

        private void CompareVersions()
        {
            string downloadToPath = AppDomain.CurrentDomain.BaseDirectory;
            //string downloadToPath = ConfigurationManager.AppSettings["localAppURL"];
            string localVersionFileName = ConfigurationManager.AppSettings["localVersionName"];
            string localVersion =
              Versions.LocalVersion(Path.Combine(downloadToPath, localVersionFileName));
            if(localVersion == null)
            {
                Versions.CreateLocalVersionFile(downloadToPath, localVersionFileName, "0.0.0");
            }
            //string remoteURL = "http://localhost/AutoUpdate/";
            string remoteURL = ConfigurationManager.AppSettings["remoteAppURL"];
            if (!remoteURL.EndsWith("/"))
                remoteURL += "/";
            string remoteVersion = Versions.RemoteVersion(remoteURL + ConfigurationManager.AppSettings["remoteVersionName"]);
            string remoteFile = remoteURL + ConfigurationManager.AppSettings["remoteZipName"];

            LocalVersion.Text = localVersion;
            RemoteVersion.Text = remoteVersion;
            if (localVersion != remoteVersion)
            {
                BeginDownload(remoteFile, downloadToPath, remoteVersion, localVersionFileName);
            }
        }

        private void BeginDownload(string remoteURL, string downloadToPath, string version, string executeTarget)
        {
            string filePath = Versions.CreateTargetLocation(downloadToPath, version);

            Uri remoteURI = new Uri(remoteURL);
            WebClient downloader = new WebClient();

            downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);

            downloader.DownloadFileAsync(remoteURI, filePath + ".zip",
                new string[] { version, downloadToPath, executeTarget });
        }

        void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string[] us = (string[])e.UserState;
            string currentVersion = us[0];
            string downloadToPath = us[1];
            string executeTarget = us[2];

            if (!downloadToPath.EndsWith("\\"))
                // Give a trailing \ if there isn't one
                downloadToPath += "\\";

            // Download folder + zip file
            string zipName = downloadToPath + currentVersion + ".zip";
            // Download folder\version\ + executable
            string exePath = downloadToPath + executeTarget;
            FileInfo zipInfo = new FileInfo(zipName);
            if (zipInfo.Exists)
            {

                ClearDownloadFolder(downloadToPath, zipInfo);
                ZipFile.ExtractToDirectory(zipName, downloadToPath);
                zipInfo.Delete();
                if (new FileInfo(exePath).Exists)
                {
                    Versions.CreateLocalVersionFile(
                              downloadToPath, ConfigurationManager.AppSettings["localVersionName"], currentVersion);
                    //System.Diagnostics.Process proc = System.Diagnostics.Process.Start(exePath); // Debug
                
                }
                else
                {
                    MessageBox.Show("Problem with download. Version file does not exist.");
                    Environment.Exit(1);
                }
            }
            else
            {
                MessageBox.Show("Problem with download. Zip file does not exist.");
                Environment.Exit(1);
            }
            Environment.Exit(0);
        }

        private void ClearDownloadFolder(string folderPath, FileInfo downloadedZipInfo)
        {
            string[] dirs = Directory.GetDirectories(folderPath);
            for(int i = 0; i < dirs.Length; i++)
            {
                Directory.Delete(dirs[i], true);
            }
            foreach (string filePath in 
                Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                string fileName = filePath.Substring(filePath.LastIndexOf('\\')+1);
                if(
                    !fileName.Contains(downloadedZipInfo.Name) 
                    && !fileName.ToLower().Contains("update")
                    && !fileName.ToLower().Contains("config.exe")
                    && !fileName.Contains(ConfigurationManager.AppSettings["localVersionName"]))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Deleted :" +filePath);
                }
            }
            Console.WriteLine("---------------");
        }

    }
}
