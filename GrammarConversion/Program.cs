namespace GrammarConversion
{
    class Program
    {
        public const string LEFT_GRAMMAR_TYPE = "left";
        public const string RIGHT_GRAMMAR_TYPE = "right";

        static public List<string> GetInfoFromFile(string filePath)
        {
            List<string> info = new List<string>();

            using (StreamReader reader = new StreamReader(@filePath))
            {
                while (!reader.EndOfStream)
                {
                    info.Add(reader.ReadLine());
                }
            }

            return info;
        }

        static public void PrintDataToFile(string data, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(data);
            }
        }

        static public IMachineInfo ProcessData(List<string> infoFromFile, string grammarType)
        {
            IDeterminableMachineInfo machineInfo;

            switch (grammarType)
            {
                case LEFT_GRAMMAR_TYPE:
                    machineInfo = new MooreMachineInfo("LEFT_GRAMMAR_TYPE", infoFromFile);
                    break;
                case RIGHT_GRAMMAR_TYPE:
                    machineInfo = new MooreMachineInfo("RIGHT_GRAMMAR_TYPE", infoFromFile);
                    break;
                default:
                    throw new ArgumentException("Unavailable conversion type");
            }
            machineInfo.Determine();

            return machineInfo;
        }

        static void Main(string[] args)
        {
            try
            {
                Args parsedArgs = Args.Parse(args);
                List<string> infoFromFile = GetInfoFromFile(parsedArgs.SourceFilePath);

                IMachineInfo machineInfo = ProcessData(infoFromFile, parsedArgs.GrammarType);

                PrintDataToFile(machineInfo.GetCsvData(), parsedArgs.DestinationFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}