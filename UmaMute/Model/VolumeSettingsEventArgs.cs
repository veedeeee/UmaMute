using System;

namespace UmaMute {
  enum SettingOwner {
    Foreground,
    Background
  }

  class VolumeSettingsEventArgs: EventArgs {
    public SettingOwner SettingOwner;

    /// <summary>
    /// -1 means Use Current volume
    /// </summary>
    public short Volume;
  }
}
