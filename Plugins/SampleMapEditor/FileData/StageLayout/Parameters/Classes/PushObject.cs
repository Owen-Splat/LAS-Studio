namespace SampleMapEditor
{
    public class PushableObjectParams : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter("East Push", false);
        new public Parameter Parameter2 { get; set; } = new Parameter("West Push", false);
        new public Parameter Parameter3 { get; set; } = new Parameter("South Push", false);
        new public Parameter Parameter4 { get; set; } = new Parameter("North Push", false);

        new public string[] GetParametersAsStrings()
        {
            return new string[8]
            {
                ((bool)Parameter1.Value).ToString(),
                ((bool)Parameter2.Value).ToString(),
                ((bool)Parameter3.Value).ToString(),
                ((bool)Parameter4.Value).ToString(),
                (string)Parameter5.Value,
                (string)Parameter6.Value,
                (string)Parameter7.Value,
                (string)Parameter8.Value,
            };
        }
    }
}