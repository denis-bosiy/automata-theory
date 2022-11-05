namespace NDSMToDSM
{
    class Program
    {
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

        static public IMachineInfo ProcessData(List<string> infoFromFile)
        {
            IDeterminableMachineInfo machineInfo;

            machineInfo = new MooreMachineInfo(infoFromFile);
            machineInfo.Determine();

            return machineInfo;
        }

        static void Main(string[] args)
        {
            try
            {
                Args parsedArgs = Args.Parse(args);
                List<string> infoFromFile = GetInfoFromFile(parsedArgs.SourceFilePath);

                IMachineInfo machineInfo = ProcessData(infoFromFile);

                PrintDataToFile(machineInfo.GetCsvData(), parsedArgs.DestinationFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}