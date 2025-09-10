namespace SampleMapEditor
{
    public class TreasureBoxParameters : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter() { Name = "Actor Switch", Value = "Switch0" };
        new public Parameter Parameter2 { get; set; } = new Parameter() { Name = "Item", Value = "Seashell" };
        new public Parameter Parameter3 { get; set; } = new Parameter() { Name = "Index", Value = 0 };
    }
}