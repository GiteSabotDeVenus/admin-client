using System.Diagnostics;

namespace Executor; 

public class Base {
    
    public static void Execute(string file, string arguments) {
        Execute(file, arguments, new ProcessStartInfo());
    }

    public static void Execute(string file, string arguments, ProcessStartInfo startInfo) {
        Process process = new Process();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.CreateNoWindow = true;
        startInfo.FileName = file;
        startInfo.Arguments = arguments;
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }
    
}