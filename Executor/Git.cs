using System.Diagnostics;

namespace Executor;

public class Git {
    
    public static void Execute(string command, bool inRepository) {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        if (inRepository) {
            startInfo.WorkingDirectory = "data";
        }
        Base.Execute("git.exe", command, startInfo);
    }

}