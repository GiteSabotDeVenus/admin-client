using Executor;

namespace Reservations;

public class Importer {
    private string _dataRepository;
    private string _reservationsFile;
    private string _repositoryFolder;

    public Importer(string dataRepository, string reservationsFile, string repositoryFolder) {
        _dataRepository = dataRepository;
        _reservationsFile = reservationsFile;
        _repositoryFolder = repositoryFolder;
    }
    
    public void ImportReservations() {
        Cmd.Execute($"rmdir /s /q {_repositoryFolder}");
        Git.Execute($"clone {_dataRepository}", false);
    }

    public Dictionary<string, List<int>> GetReservations() {
        var reservations = new Dictionary<string, List<int>>();
        var content = File.ReadAllLines(_reservationsFile);
        foreach (var line in content) {
            var data = line.Split(":");
            var date = data[0];
            var days = new List<int>();
            foreach (var day in data[1].Split(";"))
                try {
                    days.Add(int.Parse(day));
                }
                catch (FormatException) {
                    // Just ignore it
                }

            reservations[date] = days;
        }

        return reservations;
    }
}