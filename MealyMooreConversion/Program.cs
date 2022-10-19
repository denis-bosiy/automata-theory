namespace MealyMooreConversion
{
    class Program
    {
        public const string MEALY_TO_MOORE_CONVERSION_TYPE = "mealy-to-moore";
        public const string MOORE_TO_MEALY_CONVERSION_TYPE = "moore-to-mealy";

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

        static public string GetOutData(List<string> infoFromFile, string conversionType)
        {
            string outData = "";

            try
            {
                if (conversionType == MEALY_TO_MOORE_CONVERSION_TYPE)
                {
                    MealyMachineInfo mealyMachineInfo = new MealyMachineInfo(infoFromFile);
                    MooreMachineInfo mooreMachineInfo = mealyMachineInfo.GetConvertedToMoore();
                    outData = mooreMachineInfo.GetCsvData();
                }
                if (conversionType == MOORE_TO_MEALY_CONVERSION_TYPE)
                {
                    MooreMachineInfo mooreMachineInfo = new MooreMachineInfo(infoFromFile);
                    MealyMachineInfo mealyMachineInfo = mooreMachineInfo.GetConvertedToMealy();
                    outData = mealyMachineInfo.GetCsvData();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return outData;
        }

        static public IMachineInfo ProcessData(List<string> infoFromFile, string conversionType)
        {
            IMachineInfo machineInfo;

            switch(conversionType) {
                case MEALY_TO_MOORE_CONVERSION_TYPE:
                    MealyMachineInfo mealyMachineInfo = new MealyMachineInfo(infoFromFile);
                    machineInfo = mealyMachineInfo.GetConvertedToMoore();
                    break;
                case MOORE_TO_MEALY_CONVERSION_TYPE:
                    MooreMachineInfo mooreMachineInfo = new MooreMachineInfo(infoFromFile);
                    machineInfo = mooreMachineInfo.GetConvertedToMealy();
                    break;
                default:
                    throw new ArgumentException("Unavailable conversion type");
            }

            return machineInfo;
        }

        static void Main(string[] args)
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