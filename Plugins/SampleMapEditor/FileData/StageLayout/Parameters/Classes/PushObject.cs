namespace SampleMapEditor
{
    public class PushableObjectParams : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter("East Push", false);
        new public Parameter Parameter2 { get; set; } = new Parameter("West Push", false);
        new public Parameter Parameter3 { get; set; } = new Parameter("South Push", false);
        new public Parameter Parameter4 { get; set; } = new Parameter("North Push", false);
    }
}