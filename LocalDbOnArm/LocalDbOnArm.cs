using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IntelliTect.LocalDbOnArm;

public static class LocalDbOnArm
{
    /// <summary>
    /// This takes in a connection string and returns a new connection string that will work 
    /// with LocalDB on ARM processors. If it is not an ARM processor or using LocalDB no 
    /// changes will be made.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns>An updated connection string if running on LocalDb and ARM.</returns>
    /// <exception cref="Exception">Unable to start SQL LocalDB</exception>
    public static string UpdateConnectionString(string connectionString)
    {
            // See if we are even trying to connect to localDb on an ARM processor.
            if (connectionString.Contains("(localdb)",StringComparison.OrdinalIgnoreCase) &&
                    (RuntimeInformation.OSArchitecture == Architecture.Arm ||
                    RuntimeInformation.OSArchitecture == Architecture.Arm64)
                )
            {
                // Get the current status of LocalDb because it can stop unexpectedly.
                if (!IsRunning())
                {
                    if (!StartLocalDb())
                    {
                        throw new Exception("Could not start SQL LocalDB");
                    }
                }
                // It is running now get the named pipe
                var namedPipe = GetNamedPipe();
                // Insert this into the connection string
                var parts = connectionString.Split(';');
                StringBuilder newConnectionString = new();
                foreach (var part in parts)
                {
                    var partParts = part.Split("=");
                    if (partParts.Length > 1 && partParts[0].ToLower().Trim() == "server")
                    {
                        newConnectionString.Append(partParts[0] + "=" + namedPipe + ";");
                    }
                    else
                    {
                        newConnectionString.Append(part + ";");
                    }
                }
                return newConnectionString.ToString();
                //}
            }
            return connectionString;
        }

    private static bool IsRunning()
    {
        var output = GetInfo();
        return output.Contains("np:\\\\.\\pipe\\LOCALDB") && output.Contains("Running");
    }

    private static string GetNamedPipe()
    {
        var output = GetInfo();
        var parts = output.Split("Instance pipe name: ");
        return parts[1].Trim();
    }

    private static string GetInfo()
    {
        return RunProcess("SqlLocalDB.exe", "info MSSQLLocalDB");
    }

    private static bool StartLocalDb()
    {
        var output = RunProcess("SqlLocalDB.exe", "start");
        return output.Contains("LocalDB") && output.Contains("started.");
    }

    private static string RunProcess(string command, string args)
    {
        Process p = new();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = command;
        p.StartInfo.Arguments = args;
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return output;
    }
}
