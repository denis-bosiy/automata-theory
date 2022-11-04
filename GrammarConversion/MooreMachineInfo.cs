namespace GrammarConversion
{
    internal class MooreMachineInfo : IDeterminableMachineInfo
    {
        private const string LEFT_GRAMMAR_TYPE = "LEFT_GRAMMAR_TYPE";
        private const string RIGHT_GRAMMAR_TYPE = "RIGHT_GRAMMAR_TYPE";
        private const string EMPTY_STATE = "H";
        private const string FINISH_OUTPUT_SYMBOL = "F";

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

        public MooreMachineInfo(string grammarDirection, List<string> grammarInfo = null)
        {
            OutputAlphabet = new List<string>();
            States = new List<string>();
            InputAlphabet = new List<string>();
            TransitionFunctions = new List<List<string>>();

            switch (grammarDirection)
            {
                case MooreMachineInfo.LEFT_GRAMMAR_TYPE:
                    this.InitializeLeftGrammarType(grammarInfo);
                    break;
                case MooreMachineInfo.RIGHT_GRAMMAR_TYPE:
                    this.InitializeRightGrammarType(grammarInfo);
                    break;
                default:
                    throw new ArgumentException("Unavailable grammar type");
            }
        }

        public List<string> States;
        public List<List<string>> TransitionFunctions;
        public List<string> OutputAlphabet;
        public List<string> InputAlphabet;

        public string GetCsvData()
        {
            string csvData = "";

            csvData += ";";
            if (this.OutputAlphabet.Count != 0)
            {
                csvData += this.OutputAlphabet.Aggregate((total, part) => $"{total};{part}");
            }
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

        public void Determine()
        {
            List<string> determinedStates = States;
            List<string> determinedOutputAlphabet = OutputAlphabet;
            List<string> finishStates = new List<string>();
            for(int i = 0; i < OutputAlphabet.Count; i++)
            {
                if (OutputAlphabet[i] == FINISH_OUTPUT_SYMBOL)
                {
                    finishStates.Add(determinedStates[i]);
                }
            }
            List<string> newStates = new List<string>();
            List<List<string>> determinedTransitionFunctions = TransitionFunctions;

            do
            {
                newStates = new List<string>();
                foreach (List<string> inputSymbolTransitionFunction in determinedTransitionFunctions)
                {
                    foreach (string transitionFunction in inputSymbolTransitionFunction)
                    {
                        if (transitionFunction.Contains(","))
                        {
                            newStates.Add(transitionFunction);
                        }
                    }
                }

                foreach (string newState in newStates)
                {
                    // Сортировка символов по алфавиту
                    string determinedState = newState.Replace(",", "");
                    determinedState = new string(determinedState.OrderBy(ch => ch).ToArray());
                    if (determinedStates.Contains(determinedState))
                    {
                        break;
                    }
                    determinedStates.Add(determinedState);
                    determinedOutputAlphabet.Add("");
                    ISet<string> determinedStatesCharHashSet = newState.Split(",").ToHashSet<string>() ;
                    for(int i = 0; i < finishStates.Count; i++)
                    {
                        if (determinedStatesCharHashSet.Contains(finishStates[i]))
                        {
                            determinedOutputAlphabet[determinedOutputAlphabet.Count - 1] = FINISH_OUTPUT_SYMBOL;
                        }
                    }
                    List<string> states = newState.Split(",").ToList();
                    foreach (List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions)
                    {
                        inputSymbolTransitionFunctions.Add("");
                    }

                    foreach (string state in states)
                    {
                        int indexOfState = determinedStates.IndexOf(state);
                        foreach (List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions)
                        {
                            string transitionFunction = inputSymbolTransitionFunctions[indexOfState];
                            if (transitionFunction == "")
                            {
                                continue;
                            }
                            if (inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1] == "")
                            {
                                inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1] += transitionFunction;
                            }
                            else
                            {
                                inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1] += "," + transitionFunction;
                            }
                        }
                    }

                    foreach (List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions)
                    {
                        ISet<char> determinedStateCharHashSet = determinedState.ToHashSet<char>();
                        for (int indexOfTransitionFunction = 0; indexOfTransitionFunction < inputSymbolTransitionFunctions.Count; indexOfTransitionFunction++)
                        {
                            string transitionFunction = inputSymbolTransitionFunctions[indexOfTransitionFunction];

                            if (transitionFunction.Replace(",", "").ToHashSet<char>().SetEquals(determinedStateCharHashSet))
                            {
                                inputSymbolTransitionFunctions[indexOfTransitionFunction] = determinedState;
                            }
                        }
                    }
                }
            } while (newStates.Count != 0);

            Dictionary<string, string> newStatesToDeterminedStates = new Dictionary<string, string>();
            for(int i = 0; i < determinedStates.Count; i++)
            {
                newStatesToDeterminedStates.Add(determinedStates[i], "S" + i);
            }
            List<List<string>> newTransitionFunctions = determinedTransitionFunctions;
            foreach(List<string> inputSymbolTransitionFunctions in newTransitionFunctions)
            {
                for(int i = 0; i < inputSymbolTransitionFunctions.Count; i++)
                {
                    if (inputSymbolTransitionFunctions[i] != "")
                    {
                        inputSymbolTransitionFunctions[i] = newStatesToDeterminedStates[inputSymbolTransitionFunctions[i]];
                    }
                }
            }
            States = newStatesToDeterminedStates.Values.ToList();
            TransitionFunctions = newTransitionFunctions;
            OutputAlphabet = determinedOutputAlphabet;
        }

        private void InitializeRightGrammarType(List<string> grammarInfo = null)
        {
            this.States.Add(EMPTY_STATE);
            this.OutputAlphabet.Add(FINISH_OUTPUT_SYMBOL);
            foreach (string grammarRule in grammarInfo)
            {
                string state = grammarRule.Split(" -> ")[0];
                this.States.Add(state);
                this.OutputAlphabet.Add("");
            }

            foreach (string grammarRule in grammarInfo)
            {
                string currentState = grammarRule.Split(" -> ")[0];
                List<string> grammarTransitions = grammarRule.Split(" -> ")[1].Split(" | ").ToList();
                int indexOfCurrentState = States.IndexOf(currentState);

                foreach (string grammarTransition in grammarTransitions)
                {
                    bool isInputSymbol = true;
                    string state = "";
                    string inputSymbol = "";

                    foreach (char ch in grammarTransition)
                    {
                        if (isInputSymbol && !States.Contains(ch.ToString()))
                        {
                            inputSymbol += ch;
                        }
                        else
                        {
                            isInputSymbol = false;
                            state += ch;
                        }
                    }
                    if (state == "")
                    {
                        state = EMPTY_STATE;
                    }

                    if (!InputAlphabet.Contains(inputSymbol))
                    {
                        InputAlphabet.Add(inputSymbol);
                    }

                    int indexOfInputSymbol = InputAlphabet.IndexOf(inputSymbol);

                    if (indexOfInputSymbol >= TransitionFunctions.Count)
                    {
                        List<string> newList = new List<string>();
                        foreach (string st in States)
                        {
                            newList.Add("");
                        }
                        TransitionFunctions.Add(newList);
                    }

                    if (TransitionFunctions[indexOfInputSymbol][indexOfCurrentState] == "")
                    {
                        TransitionFunctions[indexOfInputSymbol][indexOfCurrentState] += state;
                    }
                    else
                    {
                        TransitionFunctions[indexOfInputSymbol][indexOfCurrentState] += "," + state;
                    }
                }
            }
        }

        private void InitializeLeftGrammarType(List<string> grammarInfo = null)
        {
            this.States.Add(EMPTY_STATE);
            this.OutputAlphabet.Add("");
            foreach (string grammarRule in grammarInfo)
            {
                string state = grammarRule.Split(" -> ")[0];
                this.States.Add(state);
                this.OutputAlphabet.Add("");
            }

            foreach (string grammarRule in grammarInfo)
            {
                string currentState = grammarRule.Split(" -> ")[0];
                List<string> grammarTransitions = grammarRule.Split(" -> ")[1].Split(" | ").ToList();
                int indexOfCurrentState = States.IndexOf(currentState);

                foreach (string grammarTransition in grammarTransitions)
                {
                    bool isState = true;
                    string state = "";
                    string inputSymbol = "";

                    foreach (char ch in grammarTransition)
                    {
                        if(isState && States.Contains(ch.ToString()))
                        {
                            state += ch;
                        } else
                        {
                            isState = false;
                            inputSymbol += ch;
                        }
                    }
                    if (state == "")
                    {
                        state = EMPTY_STATE;
                        OutputAlphabet[indexOfCurrentState] = FINISH_OUTPUT_SYMBOL;
                    }

                    if (!InputAlphabet.Contains(inputSymbol))
                    {
                        InputAlphabet.Add(inputSymbol);
                    }

                    int indexOfInputSymbol = InputAlphabet.IndexOf(inputSymbol);

                    if (indexOfInputSymbol >= TransitionFunctions.Count)
                    {
                        List<string> newList = new List<string>();
                        foreach(string st in States)
                        {
                            newList.Add("");
                        }
                        TransitionFunctions.Add(newList);
                    }

                    int indexOfState = States.IndexOf(state);
                    if (TransitionFunctions[indexOfInputSymbol][indexOfState] == "")
                    {
                        TransitionFunctions[indexOfInputSymbol][indexOfState] += currentState;
                    }
                    else
                    {
                        TransitionFunctions[indexOfInputSymbol][indexOfState] += "," + currentState;
                    }
                }
            }
        }
    }
}
