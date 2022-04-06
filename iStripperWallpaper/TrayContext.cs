using DesktopWallpaper;
using iStripperWallpaper.BLL;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iStripperWallpaper
{
    public class TrayContext : ApplicationContext
    {
        RegistryMonitor monitor = new RegistryMonitor(RegistryHive.CurrentUser, @"Software\Totem\vghd\parameters");
	
        private NotifyIcon trayIcon;
        private TrackBarMenuItem menuBrightness;
        private ToolStripMenuItem wallpaperToolStripMenuItem;
        public TrayContext ()
        {
            this.ThreadExit += TrayContext_ThreadExit;
            // Initialize Tray Icon
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem menuShowText = new ToolStripMenuItem("Show Text");
            menuShowText.CheckOnClick = true;
            menuShowText.CheckedChanged += MenuShowText_CheckedChanged;
            menuShowText.Checked = Properties.Settings.Default.WallpaperDetails;
            contextMenu.Items.Add(menuShowText);

            wallpaperToolStripMenuItem = new ToolStripMenuItem("Monitors");
            //get number of monitors for wallpaper
            try
            {
                var wallpaper = (IDesktopWallpaper)(new DesktopWallpaperClass());
                string[] monitorsChecked = Properties.Settings.Default.WallpaperMonitors.Split(",", StringSplitOptions.TrimEntries);
                for (uint i = 0; i < wallpaper.GetMonitorDevicePathCount(); i++)
                {
                    ToolStripMenuItem newitem = new ToolStripMenuItem("Monitor " + (i+1).ToString());
                    newitem.CheckOnClick = true;                    
                    newitem.Tag = i;
                    if (monitorsChecked.Contains((i+1).ToString())) newitem.Checked = true;
                    wallpaperToolStripMenuItem.DropDownItems.Add(newitem);
                    newitem.CheckedChanged += WallpaperMonitor_CheckedChanged;
                }
            }
            catch { }
            contextMenu.Items.Add(wallpaperToolStripMenuItem);

            menuBrightness = new TrackBarMenuItem();
            menuBrightness.Has2Values = false;
            menuBrightness.BackColor = System.Drawing.Color.Transparent;
            menuBrightness.ClientSize = new System.Drawing.Size(200, 48);
            menuBrightness.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            menuBrightness.ForeColor = System.Drawing.Color.White;
            menuBrightness.Has2Values = false;
            menuBrightness.LargeChange = new decimal(new int[] {
            10,
            0,
            0,
            0});
            menuBrightness.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            menuBrightness.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            menuBrightness.Name = "menuBrightness";
            menuBrightness.ScaleDivisions = new decimal(new int[] {
            10,
            0,
            0,
            0});
            menuBrightness.Size = new System.Drawing.Size(200, 48);
            menuBrightness.SmallChange = new decimal(new int[] {
            5,
            0,
            0,
            0});
            menuBrightness.Text = "Brightness";
            menuBrightness.TickColor = System.Drawing.Color.White;
            menuBrightness.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            menuBrightness.TrackbarColor = System.Drawing.Color.Transparent;
            menuBrightness.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            menuBrightness.Value2 = new decimal(new int[] {
            60,
            0,
            0,
            0});
            menuBrightness.Value = Properties.Settings.Default.WallpaperBrightness;
            menuBrightness.ValueChanged += menuBrightness_ValueChanged;
            contextMenu.Items.Add(menuBrightness);
            //trackbarWallpaperBrightness.Value = Properties.Settings.Default.WallpaperBrightness;
            //automaticWallpaperToolStripMenuItem.Checked = Properties.Settings.Default.AutoWallpaper;
            //showTextToolStripMenuItem.Checked = Properties.Settings.Default.WallpaperDetails;
           
            ToolStripMenuItem menuExit = new ToolStripMenuItem("Exit");
            menuExit.Click += MenuExit_Click;
            contextMenu.Items.Add(menuExit);

            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.stripper,        
                ContextMenuStrip = contextMenu,
                Visible = true
            };
            StaticPropertiesLoader.loadXML();
            ChangeWallpaper(true);
            monitor.RegChanged += new EventHandler(OnRegChanged);
            monitor.Start();
        }

        private void OnRegChanged(object? sender, EventArgs e)
        {
            ChangeWallpaper(false);
        }

        private void MenuExit_Click(object? sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            Properties.Settings.Default.Save();
            monitor.Stop();
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Wallpaper.RestoreWallpaper();
            Application.Exit();
        }

        private void TrayContext_ThreadExit(object? sender, EventArgs e)
        {
            Wallpaper.RestoreWallpaper();
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }

        private void menuBrightness_ValueChanged(object? sender, EventArgs e)
        {
            menuBrightness.MouseUp += menuBrightness_MouseUp;
            menuBrightness.ValueChanged -= menuBrightness_ValueChanged;

        }

        private async void menuBrightness_MouseUp(object sender, EventArgs e)
        {
            menuBrightness.MouseUp -= menuBrightness_MouseUp;
            menuBrightness.ValueChanged += menuBrightness_ValueChanged;

            Properties.Settings.Default.WallpaperBrightness = menuBrightness.Value;
            string model, outfit, nowPlayingTagShort;
            GetModelOutfit(out model, out outfit, out nowPlayingTagShort);
            Wallpaper.ChangeBrightness(model, outfit);
        }

        private void WallpaperMonitor_CheckedChanged(object? sender, EventArgs e)
        {
            string m = "";
            foreach (var item in wallpaperToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                {
                    if (((ToolStripMenuItem)item).Checked && ((ToolStripMenuItem)item).Tag != null)
                    {
                        if (m == "")
                            m += ((uint)((ToolStripMenuItem)item).Tag+1).ToString();
                        else
                            m += "," + ((uint)((ToolStripMenuItem)item).Tag+1).ToString();                    
                    }
                }
            }
            Properties.Settings.Default.WallpaperMonitors = m;
            ChangeWallpaper(true);
        }

    
        public static string PascalCase(string word)
        {
            return string.Join(" " , word.Split('_')
                         .Select(w => w.Trim())
                         .Where(w => w.Length > 0)
                         .Select(w => w.Substring(0,1).ToUpper() + w.Substring(1).ToLower()));
        }

        private string lastWallpaperClip="";
        private HttpClient client = new HttpClient();
        private async Task ChangeWallpaper(bool NotFromCheck = true)
        {
            //get json for model and find details
            string model, outfit, nowPlayingTagShort;
            GetModelOutfit(out model, out outfit, out nowPlayingTagShort);
            foreach (var item in wallpaperToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem)
                    if (((ToolStripMenuItem)item).Checked && ((ToolStripMenuItem)item).Tag != null)
                    {
                        if ((NotFromCheck || Properties.Settings.Default.AutoWallpaper && nowPlayingTagShort != lastWallpaperClip))
                        {
                            CardPhotos photos = new CardPhotos();
                            await photos.LoadCardPhotos(client, nowPlayingTagShort);
                            Random r = new Random();
                            Wallpaper.ChangeWallpaper((uint)((ToolStripMenuItem)item).Tag, photos.getRandomWidescreenURL(), model, outfit);
                        }
                    }
                    else if (((ToolStripMenuItem)item).Tag != null)
                    {
                        Wallpaper.RestoreWallpaperByID((uint)((ToolStripMenuItem)item).Tag);
                    }

            }

            lastWallpaperClip = nowPlayingTagShort;
        }

        private void GetModelOutfit(out string model, out string outfit, out string nowPlayingTagShort)
        {
            string nowplaying = GetNowPlaying();
            model = "";
            outfit = "";
            nowPlayingTagShort = "";
            if (nowplaying != "")
            {
                nowPlayingTagShort = nowplaying.Split("\\")[0];
                var c = StaticPropertiesLoader.getCardByID(nowPlayingTagShort);
                outfit = c.name;
                var modelIDs = c.modelID.Split(",");
                string modelName = "";
                foreach(var cm in modelIDs)
                { 
                    //var m = StaticPropertiesLoader.getModelByID(cm);
                    var d = StaticPropertiesLoader.getBioByID(cm);
                    if (modelName != "")
                        modelName += " &_" + d.Name;
                    else
                        modelName = d.Name;
                }
                model = PascalCase(modelName);
            }
        }

        private string GetNowPlaying()
        {
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Totem\vghd\parameters", false);
            if (key != null)
            {
                var a = key.GetValue("CurrentAnim", "");
                if (a != null)
                { 
                    string nowp = a.ToString() ?? "";
                    key.Close();
                    return nowp;
                }
            }
            return "";
        }

        private void MenuShowText_CheckedChanged(object? sender, EventArgs e)
        {
            Properties.Settings.Default.WallpaperDetails = ((ToolStripMenuItem)sender).Checked;
            if (StaticPropertiesLoader.dnode != null) ChangeWallpaper(true);
        }

    }
}
