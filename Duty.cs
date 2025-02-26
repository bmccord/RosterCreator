public class Duty
{
    public string Name { get; set; }
    public int Importance { get; set; }
    public bool Weekly { get; set; } // Existing flag
    public bool Rotation { get; set; } // New flag

    public Duty(string name, int importance, bool weekly = false, bool rotation = false)
    {
        Name = name;
        Importance = importance;
        Weekly = weekly;
        Rotation = rotation;
    }
}