using System.Collections.Generic;
using NAudio.Wave;

namespace VoiceQueen
{
    public class AudioDeviceInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AudioDeviceManager
    {
        public IList<AudioDeviceInfo> GetInputDevices()
        {
            var devices = new List<AudioDeviceInfo>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var info = WaveIn.GetCapabilities(i);
                devices.Add(new AudioDeviceInfo { Id = i, Name = info.ProductName });
            }

            return devices;
        }

        public IList<AudioDeviceInfo> GetOutputDevices()
        {
            var devices = new List<AudioDeviceInfo>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var info = WaveOut.GetCapabilities(i);
                devices.Add(new AudioDeviceInfo { Id = i, Name = info.ProductName });
            }

            return devices;
        }
    }
}
