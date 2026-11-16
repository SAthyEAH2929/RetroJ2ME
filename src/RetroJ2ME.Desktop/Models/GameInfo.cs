namespace RetroJ2ME.Desktop.Models;

public class GameInfo
{
    public string Name { get; set; } = "Unknown Game";
    public string Vendor { get; set; } = "Unknown";
    public string Version { get; set; } = "Unknown";
    public string JarPath { get; set; } = "";
    public long FileSize { get; set; }
    public bool HasBeenPlayed { get; set; }
}