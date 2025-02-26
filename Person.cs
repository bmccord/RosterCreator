public class Person
{
    public string Name { get; set; }
    public List<Duty> Duties { get; set; }
    public bool SundayAM { get; set; } = true; // New flag
    public bool SundayPM { get; set; } = true; // New flag
    public bool WednesdayPM { get; set; } = true; // New flag

    public Person(string name, List<Duty> duties)
    {
        Name = name;
        Duties = duties;
    }
}