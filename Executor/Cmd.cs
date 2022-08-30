namespace Executor; 

public class Cmd {
    
    public static void Execute(string command) {
        Base.Execute("cmd.exe", "/c" + command);
    }
}