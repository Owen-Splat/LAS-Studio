namespace SampleMapEditor
{
    public class ItemParameters : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter("Index", 0);

        new public string[] GetParametersAsStrings()
        {
            return new string[8]
            {
                ((int)Parameter1.Value).ToString(),
                (string)Parameter2.Value,
                (string)Parameter3.Value,
                (string)Parameter4.Value,
                (string)Parameter5.Value,
                (string)Parameter6.Value,
                (string)Parameter7.Value,
                (string)Parameter8.Value,
            };
        }
    }
}