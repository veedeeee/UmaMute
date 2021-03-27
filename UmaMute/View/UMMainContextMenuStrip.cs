using System;
using System.Windows.Forms;

using UmaMute.Model;

namespace UmaMute.View {
  class UMMainContextMenuStrip: ContextMenuStrip {
    private static UMMainContextMenuStrip _singletonInstance = new UMMainContextMenuStrip();
    public static UMMainContextMenuStrip MenuStrip() {
      return _singletonInstance;
    }

    private readonly ToolStripMenuItem volumeForegroundItem = new ToolStripMenuItem {
      Text = Properties.Resources.MenuItem_VolumeForForeground_Title + "(" + Properties.Settings.Default.VolumeInForeground + "%)"
    };
    private readonly ToolStripMenuItem volumeBackgroundItem = new ToolStripMenuItem {
      Text = Properties.Resources.MenuItem_VolumeForBackground_Title + "(" + Properties.Settings.Default.VolumeInBackground + "%)"
    };

    private UMMainContextMenuStrip() {
      // Enable AutoMute
      ToolStripMenuItem isEnabledAutoMuteItem = new ToolStripMenuItem();
      isEnabledAutoMuteItem.Text = Properties.Resources.MenuItem_EnableThisApp_Title;
      isEnabledAutoMuteItem.Click += this.EnabledAutoMuteItemClicked;
      isEnabledAutoMuteItem.Checked = Properties.Settings.Default.IsAutoMuteEnabled;
      this.Items.Add(isEnabledAutoMuteItem);

      this.Items.Add(new ToolStripSeparator());

      // Volume Settings
      // Foreground volumes
      ToolStripMenuItem currentSettingForegroundItem = new ToolStripMenuItem {
        Text = Properties.Resources.MenuItem_Volume_CurrentValue + ": " + Properties.Settings.Default.VolumeInForeground.ToString() + "%",
        Enabled = false
      };
      ToolStripMenuItem volumeForegroundCurrentItem = new ToolStripMenuItem {
        Text = Properties.Resources.MenuItem_Volume_CurrentVolume_Title,
        ToolTipText = Properties.Resources.MenuItem_Volume_CurrentVolume_Description
      };
      volumeForegroundCurrentItem.Click += (sender, e) => {
        VolumeClicked(sender, new VolumeSettingsEventArgs {
          SettingOwner = SettingOwner.Foreground,
          Volume = -1
        });
      };
      volumeForegroundItem.DropDownItems.Add(currentSettingForegroundItem);
      volumeForegroundItem.DropDownItems.Add(new ToolStripSeparator());
      volumeForegroundItem.DropDownItems.Add(volumeForegroundCurrentItem);
      foreach(short val in new short[] { 100, 80, 60, 40, 20, 0 }) {
        ToolStripMenuItem item = new ToolStripMenuItem {
          Text = val.ToString() + "%"
        };
        VolumeSettingsEventArgs args = new VolumeSettingsEventArgs {
          Volume = val,
          SettingOwner = SettingOwner.Foreground
        };
        item.Click += (sender, e) => { this.VolumeClicked(sender, args); };
        volumeForegroundItem.DropDownItems.Add(item);
      }
      this.Items.Add(volumeForegroundItem);

      // Background Volumes
      ToolStripMenuItem currentSettingBackgroundItem = new ToolStripMenuItem {
        Text = Properties.Resources.MenuItem_Volume_CurrentValue + ": " + Properties.Settings.Default.VolumeInBackground.ToString() + "%",
        Enabled = false
      };
      ToolStripMenuItem volumeBackgroundCurrentItem = new ToolStripMenuItem {
        Text = Properties.Resources.MenuItem_Volume_CurrentVolume_Title,
        ToolTipText = Properties.Resources.MenuItem_Volume_CurrentVolume_Description
      };
      volumeBackgroundCurrentItem.Click += (sender, e) => {
        this.VolumeClicked(sender, new VolumeSettingsEventArgs {
          SettingOwner = SettingOwner.Background,
          Volume = -1
        });
      };
      volumeBackgroundItem.DropDownItems.Add(currentSettingBackgroundItem);
      volumeBackgroundItem.DropDownItems.Add(new ToolStripSeparator());
      volumeBackgroundItem.DropDownItems.Add(volumeBackgroundCurrentItem);
      foreach(short val in new short[] { 100, 80, 60, 40, 20, 0 }) {
        ToolStripMenuItem item = new ToolStripMenuItem {
          Text = val.ToString() + "%"
        };
        VolumeSettingsEventArgs args = new VolumeSettingsEventArgs {
          Volume = val,
          SettingOwner = SettingOwner.Background
        };
        item.Click += (sender, e) => { VolumeClicked(sender, args); };
        volumeBackgroundItem.DropDownItems.Add(item);
      }
      this.Items.Add(volumeBackgroundItem);

      this.Items.Add(new ToolStripSeparator());

      // Exit App
      ToolStripMenuItem terminateItem = new ToolStripMenuItem {
        Text = Properties.Resources.MenuItem_Terminate_Title
      };
      terminateItem.Click += TerminateClicked;
      this.Items.Add(terminateItem);
    }

    private void EnabledAutoMuteItemClicked(object sender, EventArgs e) {
      Properties.Settings.Default.IsAutoMuteEnabled = !Properties.Settings.Default.IsAutoMuteEnabled;
      ((ToolStripMenuItem)sender).Checked = Properties.Settings.Default.IsAutoMuteEnabled;
    }

    private void VolumeClicked(object sender, VolumeSettingsEventArgs e) {
      UNUmamusumeAppStateManager sppStateManager = UNUmamusumeAppStateManager.SharedManager();

      short volume = e.Volume;
      if(volume < 0) {
        if(!sppStateManager.IsUmamusumeLaunchingNow) { return; }
        volume = sppStateManager.AppVolume;
      }

      if(e.SettingOwner == SettingOwner.Foreground) {
        Properties.Settings.Default.VolumeInForeground = volume;
        volumeForegroundItem.Text = Properties.Resources.MenuItem_VolumeForForeground_Title + " (" + volume.ToString() + "%)";
        volumeForegroundItem.DropDownItems[0].Text = Properties.Resources.MenuItem_Volume_CurrentValue + ": " + volume.ToString() + "%";
      }
      if(e.SettingOwner == SettingOwner.Background) {
        Properties.Settings.Default.VolumeInBackground = volume;
        volumeBackgroundItem.Text = Properties.Resources.MenuItem_VolumeForBackground_Title + " (" + volume.ToString() + "%)";
        volumeBackgroundItem.DropDownItems[0].Text = Properties.Resources.MenuItem_Volume_CurrentValue + ": " + volume.ToString() + "%";
      }
    }

    private void TerminateClicked(object sender, EventArgs e) {
      Application.Exit();
    }
  }
}
