public class Person
{
    public string Name { get; set; }
    public List<Duty> Duties { get; set; }
    public bool SundayAM { get; set; } = true; // New flag
    public bool SundayPM { get; set; } = true; // New flag
    public bool WednesdayPM { get; set; } = true; // New flag

    public Person(string name, List<Duty> duties, bool sundayAM = true, bool sundayPM = true, bool wednesdayPM = true)
    {
        Name = name;
        Duties = duties;
        SundayAM = sundayAM;
        SundayPM = sundayPM;
        WednesdayPM = wednesdayPM;
    }
}