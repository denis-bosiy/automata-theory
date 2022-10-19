namespace MealyMooreConversion
{
    internal class MooreMachineInfo : IMachineInfo
    {
        public MooreMachineInfo(List<string> info = null)
        {
            OutputAlphabet = new List<string>();
            States = new List<string>();
            InputAlphabet = new List<string>();
            TransitionFunctions = new List<List<string>>();

            if (info == null || info.Count == 0)
            {
                return;
            }

            string[] dirtyOutputAlphabet = info[0].Split(";");
            OutputAlphabet = new ArraySegment<string>(dirtyOutputAlphabet, 1, dirtyOutputAlphabet.Length - 1).ToList();
            string[] dirtyStates = info[1].Split(";");
            States = new ArraySegment<string>(dirtyStates, 1, dirtyStates.Length - 1).ToList();
            for (int i = 2; i < info.Count; i++)
            {
                string[] values = info[i].Split(";");
                InputAlphabet.Add(values[0]);
                TransitionFunctions.Add(new ArraySegment<string>(values, 1, values.Length - 1).ToList());
            }

            if (States.Count <= 1)
            {
                throw new ArgumentException("Incorrect input machine. Number of states can not be less than 2");
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
