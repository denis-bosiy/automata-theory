namespace MealyMooreConversion
{
    internal class MooreMachineInfo
    {
        public MooreMachineInfo(string csvData = "")
        {
            OutputAlphabet = new List<string>();
            States = new List<string>();
            InputAlphabet = new List<string>();
            TransitionFunctions = new List<List<string>>();

            if (csvData != "")
            {
                string[] dataLines = csvData.Split("\n");
                for (int i = 0; i < dataLines.Length - 1; i++)
                {
                    if (i == 0)
                    {
                        string[] dirtyOutputAlphabet = dataLines[i].Split(";");
                        OutputAlphabet = new ArraySegment<string>(dirtyOutputAlphabet, 1, dirtyOutputAlphabet.Length - 1).ToList();
                    }
                    else if (i == 1)
                    {
                        string[] dirtyStates = dataLines[i].Split(";");
                        States = new ArraySegment<string>(dirtyStates, 1, dirtyStates.Length - 1).ToList();
                    }
                    else
                    {
                        string[] values = dataLines[i].Split(";");
                        InputAlphabet.Add(values[0]);
                        TransitionFunctions.Add(new ArraySegment<string>(values, 1, values.Length - 1).ToList());
                    }
                }

                if (States.Count <= 1)
                {
                    throw new ArgumentException("Incorrect input machine. Number of states can not be less than 2");
                }
            }
        }

        public List<string> OutputAlphabet;
        public List<string> States;
        public List<string> InputAlphabet;
        public List<List<string>> TransitionFunctions;

        public string GetCsvData()
        {
            string csvData = "";

            csvData += ";";
            csvData += this.OutputAlphabet.Aggregate((total, part) => $"{total};{part}");
            csvData += "\n";

            csvData += ";";
            csvData += this.States.Aggregate((total, part) => $"{total};{part}");
            csvData += "\n";

            for (int i = 0; i < this.InputAlphabet.Count; i++)
            {
                csvData += this.InputAlphabet[i] + ";";
                csvData += this.TransitionFunctions[i].Aggregate((total, part) => $"{total};{part}");
                csvData += "\n";
            }

            return csvData;
        }

        public MealyMachineInfo GetConvertedToMealy()
        {
            MealyMachineInfo mealyMachineInfo = new MealyMachineInfo();

            mealyMachineInfo.States = this.States;
            mealyMachineInfo.InnerStates = this.InputAlphabet;

            for (int i = 0; i < this.TransitionFunctions.Count; i++)
            {
                mealyMachineInfo.TransitionFunctions.Add(new List<string>());
                for (int j = 0; j < this.TransitionFunctions[i].Count; j++)
                {
                    int indexOfState = this.States.IndexOf(this.TransitionFunctions[i][j]);
                    mealyMachineInfo.TransitionFunctions[i].Add(this.TransitionFunctions[i][j] + "/" + this.OutputAlphabet[indexOfState]);
                }
            }

            return mealyMachineInfo;
        }
    }
}
