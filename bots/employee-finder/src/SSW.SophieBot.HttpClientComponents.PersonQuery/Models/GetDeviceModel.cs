namespace SSW.SophieBot.HttpClientAction.Models
{
    public class GetDeviceModel
    {
        public string DeviceType { get; set; }
        public string MacAddress { get; set; }
        public string OS { get; set; }

        public string DeviceBrand { get; set; }
        public string DeviceName { get; set; }
        public string DeviceNote { get; set; }
    }
}
