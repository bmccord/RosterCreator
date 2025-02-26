public class ServiceDuty
{
    public Duty Duty { get; set; }
    public int PrintOrder { get; set; }

    public ServiceDuty(Duty duty, int printOrder)
    {
        Duty = duty;
        PrintOrder = printOrder;
    }
}