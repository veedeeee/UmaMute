using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace UmaMute.Model {
  class UMUmamusumeAppStateManager {
    private static UMUmamusumeAppStateManager _singletonInstance = new UMUmamusumeAppStateManager();
    public static UMUmamusumeAppStateManager SharedManager() {
      return _singletonInstance;
    }

    private string TargetAppName = "umamusume";
    private Process TargetAppProcess {
      get {
        foreach(Process p in Process.GetProcesses()) {
          if(p.MainWindowTitle == TargetAppName) { return p; }
        }
        return null;
      }
    }

    NAudio.CoreAudioApi.MMDevice device;

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    private UMUmamusumeAppStateManager() {
      NAudio.CoreAudioApi.MMDeviceEnumerator deviceEnumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
      this.device = deviceEnumerator.GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);
    }

    /// <summary>
    /// The umamusume.exe is launching or not
    /// </summary>
    /// <returns>True returned when the umamusume.exe is launching</returns>
    public bool IsUmamusumeLaunchingNow {
      get {
        return this.TargetAppProcess != null;
      }
    }

    /// <summary>
    /// The umamusume.exe is active or not
    /// </summary>
    /// <returns>True returned when the umamusume.exe is active</returns>
    public bool IsUmamusumeActiveNow {
      get {
        const int nChars = 256;
        IntPtr handle = IntPtr.Zero;
        StringBuilder Buff = new StringBuilder(nChars);
        handle = GetForegroundWindow();

        if(GetWindowText(handle, Buff, nChars) > 0) {
          return Buff.ToString() == TargetAppName;
        }
        return false;
      }
    }

    public short AppVolume {
      get {
        if(!this.IsUmamusumeLaunchingNow) { return 0; }
        NAudio.CoreAudioApi.SessionCollection sessions = this.device.AudioSessionManager.Sessions;
        for(short i=0; i<sessions.Count; i++) {
          NAudio.CoreAudioApi.AudioSessionControl session = sessions[i];
          if((int)session.GetProcessID == this.TargetAppProcess.Id) {
            float appVolume = session.SimpleAudioVolume.Volume;
            return (short)(session.SimpleAudioVolume.Volume * 100);
          }
        }
        return 0;
      }
      set {
        float newVolume = value / 100.0f;

        NAudio.CoreAudioApi.SessionCollection sessions = this.device.AudioSessionManager.Sessions;
        for(short i = 0; i < sessions.Count; i++) {
          NAudio.CoreAudioApi.AudioSessionControl session = sessions[i];
          if((int)session.GetProcessID == this.TargetAppProcess.Id) {
            session.SimpleAudioVolume.Volume = newVolume;
            return;
          }
        }
      }
    }

  }
}
