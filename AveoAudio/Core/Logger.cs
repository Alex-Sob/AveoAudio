using System.Diagnostics;

namespace AveoAudio;

public static class Logger
{
    public static void LogError(Exception e) => Trace.WriteLine($"[{DateTime.Now:g}] {e}");

    public static void LogInfo(string message) => Trace.WriteLine($"[{DateTime.Now:g}] {message}");
}
