namespace SampleMapEditor
{
    public class TreasureBoxParameters : BaseParameters
    {
        new public Parameter Parameter1 { get; set; } = new Parameter("Actor Switch", "Switch0");
        new public Parameter Parameter2 { get; set; } = new Parameter("Item", "Seashell");
        new public Parameter Parameter3 { get; set; } = new Parameter("Index", 0);

        new public string[] GetParametersAsStrings()
        {
            return new string[8]
            {
                (string)Parameter1.Value,
                (string)Parameter2.Value,
                ((int)Parameter3.Value).ToString(),
                (string)Parameter4.Value,
                (string)Parameter5.Value,
                (string)Parameter6.Value,
                (string)Parameter7.Value,
                (string)Parameter8.Value,
            };
        }
    }
}