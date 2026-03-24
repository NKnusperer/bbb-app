namespace BlackBoxBuddy.Models;

public class DeviceInfo
{
    public required string DeviceName { get; init; }
    public required string FirmwareVersion { get; init; }
    public bool IsProvisioned { get; set; }
    public string? IpAddress { get; init; }
}
