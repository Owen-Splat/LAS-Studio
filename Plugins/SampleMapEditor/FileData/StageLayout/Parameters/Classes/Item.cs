namespace SampleMapEditor
{
    public class ItemParameters : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter() { Name = "Index", Value = 0 };
    }
}