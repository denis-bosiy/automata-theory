namespace GrammarConversion
{
    internal class Args
    {
        public Args(string GrammarType, string SourceFilePath, string DestinationFilePath)
        {
            if (!AvailableGrammarTypes.Contains(GrammarType))
            {
                throw new ArgumentException("Incorrect grammar type");
            }

            m_GrammarType = GrammarType;
            m_SourceFilePath = SourceFilePath;
            m_DestinationFilePath = DestinationFilePath;
        }

        public string GrammarType { get { return m_GrammarType; } }
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

        private ISet<string> AvailableGrammarTypes = new HashSet<string> { Program.LEFT_GRAMMAR_TYPE, Program.RIGHT_GRAMMAR_TYPE };

        private string m_GrammarType;
        private string m_SourceFilePath;
        private string m_DestinationFilePath;
    }
}
