using Executor;

namespace Reservations; 

public class Exporter {
    private string _dataRepository;
    private string _reservationsFile;

    public Exporter(string dataRepository, string reservationsFile) {
        _dataRepository = dataRepository;
        _reservationsFile = reservationsFile;
    }

    public void ExportReservations() {
        Git.Execute("add -A", true);
        Git.Execute("commit -m \"Update reservations\"", true);
        Git.Execute($"push {_dataRepository} --all", true);
    }

    public void SetReservations(Dictionary<string, List<int>> reservations) {
        var reservationsText = ConvertReservations(reservations);
        File.WriteAllLines(_reservationsFile, reservationsText);
    }
    
    public void SetReservations(Dictionary<string, List<int>> reservations, Dictionary<string, List<int>> inversions) {
        SetReservations(InvertReservations(reservations, inversions));
    }

    private Dictionary<string, List<int>> InvertReservations(Dictionary<string, List<int>> reservations, Dictionary<string, List<int>> inversions) {
        foreach (var inversionKey in inversions.Keys) {
            var inversionDays = inversions[inversionKey];
            var reservedDays = reservations.ContainsKey(inversionKey) ? reservations[inversionKey] : new List<int>();
            foreach (var inversionDay in inversionDays) {
                if (reservedDays.Contains(inversionDay)) {
                    reservedDays.Remove(inversionDay);
                }
                else {
                    reservedDays.Add(inversionDay);
                }
            }

            reservations[inversionKey] = reservedDays;
        }

        return reservations;
    }

    private List<string> ConvertReservations(Dictionary<string, List<int>> reservations) {
        var reservationsString = new List<string>();

        foreach (var reservedKey in reservations.Keys) {
            var month = int.Parse(reservedKey.Split("-")[0]) + 1;
            var year = int.Parse(reservedKey.Split("-")[1]);
            var actualDate = DateTime.Now;

            if (((year > actualDate.Year) || (year == actualDate.Year && month >= actualDate.Month)) && reservations[reservedKey].Count > 0) {
                var reservationString = reservedKey + ":";
                foreach (var day in reservations[reservedKey]) {
                    reservationString += day + ";";
                }
                
                reservationsString.Add(reservationString);
            }
        }

        return reservationsString;
    }
    
}