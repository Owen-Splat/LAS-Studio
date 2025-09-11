namespace SampleMapEditor
{
    public class BaseParameters
    {
        public Parameter Parameter1 { get; set; } = new Parameter(0);
        public Parameter Parameter2 { get; set; } = new Parameter(1);
        public Parameter Parameter3 { get; set; } = new Parameter(2);
        public Parameter Parameter4 { get; set; } = new Parameter(3);
        public Parameter Parameter5 { get; set; } = new Parameter(4);
        public Parameter Parameter6 { get; set; } = new Parameter(5);
        public Parameter Parameter7 { get; set; } = new Parameter(6);
        public Parameter Parameter8 { get; set; } = new Parameter(7);

        public string[] GetParametersAsStrings()
        {
            return new string[8]
            {
                (string)Parameter1.Value,
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