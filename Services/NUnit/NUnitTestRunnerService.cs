using System;
using System.Diagnostics;
using System.IO;

namespace AttendanceSyncApp.Services.NUnit
{
    public class NUnitTestRunnerService
    {
        public string Run(string dllPath)
        {
            // Path to nunit3-console.exe (NuGet will install it)
            var consolePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "packages",
                "NUnit.ConsoleRunner.3.15.2",
                "tools",
                "nunit3-console.exe"
            );

            if (!File.Exists(consolePath))
            {
                throw new Exception("NUnit Console Runner not found.");
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = consolePath,
                    Arguments = $"\"{dllPath}\" --result=result.xml",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
    }
}
