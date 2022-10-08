namespace MealyMooreConversion
{
    class Program
    {
        public const string MEALY_TO_MOORE_CONVERSION_TYPE = "mealy-to-moore";
        public const string MOORE_TO_MEALY_CONVERSION_TYPE = "moore-to-mealy";

        static public string GetInfoFromFile(string filePath)
        {
            string mealyMachineInfoString = "";

            using (StreamReader reader = new StreamReader(@filePath))
            {
                while (!reader.EndOfStream)
                {
                    mealyMachineInfoString += reader.ReadLine();
                    mealyMachineInfoString += "\n";
                }
            }

            return mealyMachineInfoString;
        }

        static public void PrintDataToFile(string data, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(data);
            }
        }

        static public string GetOutData(string infoFromFile, string conversionType)
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

        static void Main(string[] args)
        {
            try
            {
                Args parsedArgs = Args.Parse(args);

                string infoFromFile = GetInfoFromFile(parsedArgs.SourceFilePath);
                string outData = GetOutData(infoFromFile, parsedArgs.ConversionType);

                PrintDataToFile(outData, parsedArgs.DestinationFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}