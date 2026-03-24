namespace BlackBoxBuddy.Device;

public interface IDashcamDevice :
    IDeviceDiscovery,
    IDeviceConnection,
    IDeviceCommands,
    IDeviceFileSystem,
    IDeviceLiveStream
{
}
