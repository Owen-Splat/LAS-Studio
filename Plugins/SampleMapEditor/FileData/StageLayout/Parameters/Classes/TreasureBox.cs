namespace SampleMapEditor
{
    public class TreasureBoxParameters : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter("Actor Switch", "Switch0");
        new public Parameter Parameter2 { get; set; } = new Parameter("Item", "Seashell");
        new public Parameter Parameter3 { get; set; } = new Parameter("Index", 0);
    }
}