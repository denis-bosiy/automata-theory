namespace NDSMToDSM
{
    internal class Args
    {
        public Args(string SourceFilePath, string DestinationFilePath)
        {
            m_SourceFilePath = SourceFilePath;
            m_DestinationFilePath = DestinationFilePath;
        }

        public string SourceFilePath { get { return m_SourceFilePath; } }
        public string DestinationFilePath { get { return m_DestinationFilePath; } }

        internal static Args Parse(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Incorrect arguments count");
            }

            return new Args(args[0], args[1]);
        }

        private string m_SourceFilePath;
        private string m_DestinationFilePath;
    }
}