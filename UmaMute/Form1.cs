using System;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

using UmaMute.View;
using UmaMute.Model;

// https://stackoverflow.com/questions/4372055/detect-active-window-changed-using-c-sharp-without-polling

namespace UmaMute {
  public partial class Form1: Form {
    public NotifyIcon notifyIcon;
    private WinEventDelegate dele = null;
    private bool wasUmamusumeActive = false;

    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    private const uint WINEVENT_OUTOFCONTEXT = 0;
    private const uint EVENT_SYSTEM_FOREGROUND = 3;

    public Form1() {
      this.ShowInTaskbar = false;
      this.notifyIcon = new NotifyIcon {
        Icon = Properties.Resources.AppIcon,
        Visible = true,
        Text = "UmaMute"
      };
      this.notifyIcon.Click += NotifyIconLeftClicked;
      this.notifyIcon.ContextMenuStrip = UMMainContextMenuStrip.MenuStrip();

      dele = new WinEventDelegate(WinEventProc);
      IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

      // Launch umamusume when UmaMute has launched
      if(Properties.Settings.Default.IsLaunchUmamusumeWhenUmaMuteHasLaunched) {
        Process.Start(new ProcessStartInfo("dmmgameplayer://umamusume/cl/general/umamusume"));
      }

      // Update Registory for Auto-launch UmaMute
      UMRegistoryManager.SharedManager().RefreshRegistoryValueForAutoLaunch();
    }

    /// <summary>
    /// Hook the Left-Click on this app, then fire Right-Clicked event
    /// </summary>
    private void NotifyIconLeftClicked( object sender, EventArgs e ) {
      MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
      method.Invoke(notifyIcon, null);
    }

    public void WinEventProc( IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime ) {
      if(!Properties.Settings.Default.IsAutoMuteEnabled) { return; }

      UMUmamusumeAppStateManager appStateManager = UMUmamusumeAppStateManager.SharedManager();

      // umamusume.exe doesn't exist in launcing apps
      if(!appStateManager.IsUmamusumeLaunchingNow) { return; }

      // window state didn't change
      if(appStateManager.IsUmamusumeActiveNow == wasUmamusumeActive) { return; }

      if(appStateManager.IsUmamusumeActiveNow && !wasUmamusumeActive) {
        // became foreground
        appStateManager.AppVolume = Properties.Settings.Default.VolumeInForeground;
      } else {
        // into background
        appStateManager.AppVolume = Properties.Settings.Default.VolumeInBackground;
      }

      wasUmamusumeActive = appStateManager.IsUmamusumeActiveNow;
    }
  }
}
