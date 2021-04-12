using System.Windows.Forms;

namespace UmaMute.Model {
  class UMRegistoryManager {
    private static UMRegistoryManager _singletonInstance = new UMRegistoryManager();
    public static UMRegistoryManager SharedManager() {
      return _singletonInstance;
    }

    public bool isEnableAutoLaunch {
      get {
        return Properties.Settings.Default.IsLaunchUmaMuteWhenWindowsHasBooted;
      }
      set {
        Properties.Settings.Default.IsLaunchUmaMuteWhenWindowsHasBooted = value;
        Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if(value) {
          regkey.SetValue(Application.ProductName, Application.ExecutablePath);
        } else {
          regkey.DeleteValue(Application.ProductName, false);
        }
        regkey.Close();
      }
    }

    /// <summary>
    /// Turn off the auto-launch once, then Turn it on again.
    /// This method will repair executable path if user has moved UmaMute to other directory.
    /// </summary>
    public void RefreshRegistoryValueForAutoLaunch() {
      if(!Properties.Settings.Default.IsLaunchUmaMuteWhenWindowsHasBooted) { return; }
      this.isEnableAutoLaunch = false;
      this.isEnableAutoLaunch = true;
    }
  }
}
