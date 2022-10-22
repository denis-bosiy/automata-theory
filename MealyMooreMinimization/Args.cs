namespace MealyMooreMinimization
{
    internal class Args
    {
        public Args(string ConversionType, string SourceFilePath, string DestinationFilePath)
        {
            if (!AvailableConversionTypes.Contains(ConversionType.ToLower()))
            {
                throw new ArgumentException("Incorrect conversion type");
            }

            m_ConversionType = ConversionType;
            m_SourceFilePath = SourceFilePath;
            m_DestinationFilePath = DestinationFilePath;
        }

        public string ConversionType { get { return m_ConversionType; } }
        public string SourceFilePath { get { return m_SourceFilePath; } }
        public string DestinationFilePath { get { return m_DestinationFilePath; } }

        internal static Args Parse(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("Incorrect arguments count");
            }

            return new Args(args[0], args[1], args[2]);
        }

        private ISet<string> AvailableConversionTypes = new HashSet<string> { Program.MEALY_MINIMIZATION_TYPE, Program.MOORE_MINIMIZATION_TYPE };

        private string m_ConversionType;
        private string m_SourceFilePath;
        private string m_DestinationFilePath;
    }
}
