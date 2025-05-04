public class TemplateDuty
{
    public Duty Duty { get; set; }
    public int PrintOrder { get; set; }

    public TemplateDuty(Duty duty, int printOrder)
    {
        Duty = duty;
        PrintOrder = printOrder;
    }
}