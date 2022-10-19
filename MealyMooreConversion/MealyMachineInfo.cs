namespace MealyMooreConversion
{
    internal class MealyMachineInfo : IMachineInfo
    {
        public MealyMachineInfo(List<string> info = null)
        {
            States = new List<string>();
            InnerStates = new List<string>();
            TransitionFunctions = new List<List<string>>();

            if (info == null || info.Count == 0)
            {
                return;
            }

            string[] dirtyStates = info[0].Split(";");
            States = new ArraySegment<string>(dirtyStates, 1, dirtyStates.Length - 1).ToList();
            for (int i = 1; i < info.Count; i++)
            {
                string[] values = info[i].Split(";");
                InnerStates.Add(values[0]);
                TransitionFunctions.Add(new ArraySegment<string>(values, 1, values.Length - 1).ToList());
            }

            if (States.Count <= 1)
            {
                throw new ArgumentException("Incorrect input machine. Number of states can not be less than 2");
            }
        }

        public List<string> States;
        public List<string> InnerStates;
        public List<List<string>> TransitionFunctions;

        public string GetCsvData()
        {
            string csvData = "";

            csvData += ";";
            csvData += this.States.Aggregate((total, part) => $"{total};{part}");
            csvData += "\n";

            for (int i = 0; i < this.InnerStates.Count; i++)
            {
                csvData += this.InnerStates[i] + ";";
                csvData += this.TransitionFunctions[i].Aggregate((total, part) => $"{total};{part}");
                csvData += "\n";
            }

            return csvData;
        }

        public MooreMachineInfo GetConvertedToMoore()
        {
            MooreMachineInfo mooreMachineInfo = new MooreMachineInfo();

            Dictionary<string, string> mooreStatesMap = new Dictionary<string, string>();
            Dictionary<string, string> viceVersaMooreStatesMap = new Dictionary<string, string>();
            foreach (List<string> transitionFunctions in this.TransitionFunctions)
            {
                foreach (string transitionFunction in transitionFunctions)
                {
                    if (!viceVersaMooreStatesMap.ContainsKey(transitionFunction))
                    {
                        string newState = "S" + mooreMachineInfo.States.Count;
                        mooreMachineInfo.States.Add(newState);
                        mooreStatesMap.Add(newState, transitionFunction);
                        viceVersaMooreStatesMap.Add(transitionFunction, newState);
                    }
                }
            }

            foreach (string state in mooreMachineInfo.States)
            {
                mooreMachineInfo.OutputAlphabet.Add(mooreStatesMap[state].Split("/")[1]);
            }

            mooreMachineInfo.InputAlphabet = this.InnerStates;

            foreach (string inputLetter in mooreMachineInfo.InputAlphabet)
            {
                mooreMachineInfo.TransitionFunctions.Add(new List<string>());
            }
            foreach (string state in mooreMachineInfo.States)
            {
                string prevState = mooreStatesMap[state].Split("/")[0];
                int mealyStatesIndex = this.States.IndexOf(prevState);

                for (int i = 0; i < mooreMachineInfo.InputAlphabet.Count; i++)
                {
                    mooreMachineInfo.TransitionFunctions[i].Add(viceVersaMooreStatesMap[this.TransitionFunctions[i][mealyStatesIndex]]);
                }
            }

            return mooreMachineInfo;
        }
    }
}
