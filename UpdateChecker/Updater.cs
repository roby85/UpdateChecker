﻿/* Updater.cs
 * UpdateChecker <https://github.com/ippavlin/UpdateChecker>
 * 
 * Copyright (c)2013 ippavlin. All rights reserved.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace UpdateChecker
{
    public partial class Updater : Form
    {
        public Updater()
        {
            InitializeComponent();
        }

        private void Updater_Load(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader(Properties.Settings.Default.LocalVersionFile))
                {
                    String line = sr.ReadToEnd();
                    this.lblCurrent.Text = "Current version: " + line;
                }
            }
            catch (Exception ex)
            {
                this.lblCurrent.Text = "Version file missing";
                Debug.Print(ex.Message);
            }
        }

        private void cmdCheck_Click(object sender, EventArgs e)
        {
            bool downloadUpdate = false;

            // Download the file from the remote server based on settings file configuration of URLs and file names.
            String remoteVer = Properties.Settings.Default.BaseURL + Properties.Settings.Default.VersionFile;
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(remoteVer, @Properties.Settings.Default.VersionFile + ".tmp");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Debug.Print(ex.Message + " (URL: " + remoteVer + ")");
            }

            // Read the values from the current and downloaded version files.
            try
            {
                int currentVersion, latestVersion;
                using (StreamReader sr = new StreamReader(Properties.Settings.Default.LocalVersionFile))
                {
                    String current = sr.ReadToEnd();
                    currentVersion = Convert.ToInt32(current);
                    Debug.Print("Current: " + currentVersion);
                }
                using (StreamReader sr = new StreamReader(Properties.Settings.Default.VersionFile + ".tmp"))
                {
                    String latest = sr.ReadToEnd();
                    latestVersion = Convert.ToInt32(latest);
                    Debug.Print("Latest: " + latestVersion);
                }
                // Compare version information
                if (currentVersion < latestVersion)
                {
                    if (((MessageBox.Show("A new version is available. Would you like to download it?", 
                        "Update available", MessageBoxButtons.YesNo) == DialogResult.Yes)))
                    {
                        downloadUpdate = true;
                    }
                }
                else if (currentVersion == latestVersion)
                {
                    if (((MessageBox.Show("You already have the latest version. Update anyways?", 
                        "No updates available", MessageBoxButtons.YesNo) == DialogResult.Yes)))
                    {
                        downloadUpdate = true;
                    }
                }
                else if (currentVersion > latestVersion)
                {
                    if (((MessageBox.Show("It appears you have a newer version than the latest. " + 
                        "Update anyways (this will probably result in a downgrade to an older version)?",
                        "No updates available", MessageBoxButtons.YesNo) == DialogResult.Yes)))
                    {
                        downloadUpdate = true;
                    }
                }

                // Download update if it was selected
                if (downloadUpdate)
                {
                    String updateFile = Properties.Settings.Default.BaseURL + Properties.Settings.Default.UpdateFile;
                    WebClient webClient = new WebClient();
                    
                    // Update version file
                    File.Delete(Properties.Settings.Default.VersionFile);
                    File.Copy(Properties.Settings.Default.VersionFile + ".tmp", Properties.Settings.Default.VersionFile);
                    webClient.DownloadFile(remoteVer, @Properties.Settings.Default.VersionFile);
                    
                    // Download update file (TODO: unzip or find alternative to downloading)
                    webClient.DownloadFile(updateFile, @Properties.Settings.Default.UpdateFile);

                    MessageBox.Show("Update successful.", "");
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }
    }
}