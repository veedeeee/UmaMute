using System;
using System.Reflection;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// https://stackoverflow.com/questions/4372055/detect-active-window-changed-using-c-sharp-without-polling

namespace UmaMute
{
  public partial class Form1 : Form
  {
    public NotifyIcon notifyIcon;
    WinEventDelegate dele = null;
    bool wasUmamusumeActive = false;

    public Form1()
    {
      this.ShowInTaskbar = false;
      this.setComponents();

      dele = new WinEventDelegate(WinEventProc);
      IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
    }

    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    private const uint WINEVENT_OUTOFCONTEXT = 0;
    private const uint EVENT_SYSTEM_FOREGROUND = 3;

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    private void setComponents()
    {
      notifyIcon = new NotifyIcon();
      notifyIcon.Icon = Properties.Resources.AppIcon;
      notifyIcon.Visible = true;
      notifyIcon.Text = "UmaMute";
      notifyIcon.Click += notifyIconLeftClicked;

      ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
      ToolStripMenuItem terminateItem = new ToolStripMenuItem();
      terminateItem.Text = "Exit UmaMute";
      terminateItem.Click += terminateClicked;
      contextMenuStrip.Items.Add(terminateItem);
      notifyIcon.ContextMenuStrip = contextMenuStrip;
    }

    private void notifyIconLeftClicked(object sender, EventArgs e)
    {
      MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
      method.Invoke(notifyIcon, null);
    }

    private void terminateClicked(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private bool isUmamusumeActiveNow()
    {

      const int nChars = 256;
      IntPtr handle = IntPtr.Zero;
      StringBuilder Buff = new StringBuilder(nChars);
      handle = GetForegroundWindow();

      if (GetWindowText(handle, Buff, nChars) > 0)
      {
        return Buff.ToString() == "umamusume";
      }
      return false;
    }

    public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
      bool _isUmamusumeActiveNow = isUmamusumeActiveNow();
      if (_isUmamusumeActiveNow && !wasUmamusumeActive)
      {
        notifyIcon.BalloonTipText = "umamusume became foreground";
        notifyIcon.ShowBalloonTip(1000);
      }
      if (!_isUmamusumeActiveNow && wasUmamusumeActive)
      {
        notifyIcon.BalloonTipText = "umamusume into background";
        notifyIcon.ShowBalloonTip(1000);
      }
      wasUmamusumeActive = _isUmamusumeActiveNow;
    }
  }
}
