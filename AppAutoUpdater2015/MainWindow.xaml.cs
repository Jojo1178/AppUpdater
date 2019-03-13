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
            string downloadToPath = ConfigurationManager.AppSettings["localAppURL"];
            string localVersion =
              Versions.LocalVersion(Path.Combine(downloadToPath, ConfigurationManager.AppSettings["localVersionName"]));
            //string remoteURL = "http://localhost/AutoUpdate/";
            string remoteURL = ConfigurationManager.AppSettings["remoteAppURL"];
            if (!remoteURL.EndsWith("/"))
                remoteURL += "/";
            string remoteVersion = Versions.RemoteVersion(remoteURL + ConfigurationManager.AppSettings["remoteVersionName"]);
            string remoteFile = remoteURL + remoteVersion + ".zip";

            LocalVersion.Text = localVersion;
            RemoteVersion.Text = remoteVersion;
            if (localVersion != remoteVersion)
            {
                BeginDownload(remoteFile, downloadToPath, remoteVersion, "update.txt");
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
            string exePath = downloadToPath + currentVersion + "\\" + executeTarget;
            FileInfo zipInfo = new FileInfo(zipName);
            if (zipInfo.Exists)
            {


                ZipFile.ExtractToDirectory(zipName, downloadToPath + currentVersion);
                if (new FileInfo(exePath).Exists)
                {
                    Versions.CreateLocalVersionFile(
                              downloadToPath, "version.txt", currentVersion);
                    System.Diagnostics.Process proc =
                              System.Diagnostics.Process.Start(exePath);
                
                }
                else
                {
                    MessageBox.Show("Problem with download. File does not exist.");
                }
                zipInfo.Delete();
            }
            else
            {
                MessageBox.Show("Problem with download. File does not exist.");
            }
        }
    }
}
