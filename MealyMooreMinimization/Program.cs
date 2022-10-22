namespace MealyMooreMinimization
{
    class Program
    {
        public const string MEALY_MINIMIZATION_TYPE = "mealy";
        public const string MOORE_MINIMIZATION_TYPE = "moore";

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

        static public IMachineInfo ProcessData(List<string> infoFromFile, string conversionType)
        {
            IMachineInfo machineInfo;

            switch (conversionType.ToLower())
            {
                case MEALY_MINIMIZATION_TYPE:
                    MealyMachineInfo mealyMachineInfo = new MealyMachineInfo(infoFromFile);
                    mealyMachineInfo.Minimize();
                    machineInfo = mealyMachineInfo;
                    break;
                case MOORE_MINIMIZATION_TYPE:
                    MooreMachineInfo mooreMachineInfo = new MooreMachineInfo(infoFromFile);
                    mooreMachineInfo.Minimize();
                    machineInfo = mooreMachineInfo;
                    break;
                default:
                    throw new ArgumentException("Unavailable conversion type");
            }

            return machineInfo;
        }

        static public void Main(string[] args)
        {
            try
            {
                Args parsedArgs = Args.Parse(args);
                List<string> infoFromFile = GetInfoFromFile(parsedArgs.SourceFilePath);

                IMachineInfo machineInfo = ProcessData(infoFromFile, parsedArgs.ConversionType);

                PrintDataToFile(machineInfo.GetCsvData(), parsedArgs.DestinationFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}