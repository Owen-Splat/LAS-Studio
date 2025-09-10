namespace SampleMapEditor
{
    public class PushableObjectParams : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter() { Name = "East Push", Value = false };
        new public Parameter Parameter2 { get; set; } = new Parameter() { Name = "West Push", Value = false };
        new public Parameter Parameter3 { get; set; } = new Parameter() { Name = "South Push", Value = false };
        new public Parameter Parameter4 { get; set; } = new Parameter() { Name = "North Push", Value = false };
    }
}