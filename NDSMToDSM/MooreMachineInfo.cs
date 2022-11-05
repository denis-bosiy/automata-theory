
namespace NDSMToDSM
{
    internal class MooreMachineInfo : IDeterminableMachineInfo
    {
        private const string FINISH_OUTPUT_SYMBOL = "F";
        private const string EMPTY_SYMBOL = "e";

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
            foreach(List<string> inputSymbolTransitionFunctions in TransitionFunctions)
            {
                for(int i = 0; i < inputSymbolTransitionFunctions.Count; i++)
                {
                    if (inputSymbolTransitionFunctions[i] == "-")
                    {
                        inputSymbolTransitionFunctions[i] = "";
                    }
                }
            }

            if (States.Count <= 1)
            {
                throw new ArgumentException("Incorrect input machine. Number of states can not be less than 2");
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
                csvData += this.TransitionFunctions[i].Aggregate((total, part) => $"{(total == "" ? "-" : total)};{(part == "" ? "-" : part)}");
                csvData += "\n";
            }

            return csvData;
        }

        public void Determine()
        {
            // Инициализируем новое состояние автомата
            List<string> determinedStates = new List<string>(States);
            List<string> determinedOutputAlphabet = new List<string>(OutputAlphabet);
            List<string> determinedInputAlphabet = new List<string>(InputAlphabet);
            Dictionary<string, string> eclosures = new Dictionary<string, string>();
            List<string> finishStates = new List<string>();
            for (int i = 0; i < OutputAlphabet.Count; i++)
            {
                if (OutputAlphabet[i] == FINISH_OUTPUT_SYMBOL)
                {
                    finishStates.Add(determinedStates[i]);
                }
            }
            List<string> newStates = new List<string>();
            List<List<string>> determinedTransitionFunctions = new List<List<string>>(TransitionFunctions);

            do
            {
                newStates = new List<string>();
                // Определение новых состояний в ДКА
                if (determinedInputAlphabet.Contains(EMPTY_SYMBOL))
                {
                    int indexOfEmptySymbol = determinedInputAlphabet.IndexOf(EMPTY_SYMBOL);
                    for (int i = 0; i < States.Count; i++)
                    {
                        if (determinedTransitionFunctions[indexOfEmptySymbol][i] != "")
                        {
                            int stateIndex = i;
                            ISet<string> statesSet = new HashSet<string>();
                            Queue<int> statesIndexQueue = new Queue<int>();

                            statesIndexQueue.Enqueue(stateIndex);
                            while (stateIndex != -1)
                            {
                                statesSet.Add(determinedStates[stateIndex]);
                                if (determinedTransitionFunctions[indexOfEmptySymbol][stateIndex] != "")
                                {
                                    if (determinedTransitionFunctions[indexOfEmptySymbol][stateIndex].Contains(","))
                                    {
                                        List<string> states = determinedTransitionFunctions[indexOfEmptySymbol][stateIndex].Split(",").ToList();

                                        foreach (string state in states)
                                        {
                                            int indexOfState = determinedStates.IndexOf(state);
                                            statesIndexQueue.Enqueue(indexOfState);
                                        }
                                    }
                                    else
                                    {
                                        int indexOfState = determinedStates.IndexOf(determinedTransitionFunctions[indexOfEmptySymbol][stateIndex]);
                                        statesIndexQueue.Enqueue(indexOfState);
                                    }
                                }
                                int newStateIndex = stateIndex;
                                while (newStateIndex == stateIndex && statesIndexQueue.Count > 0 && !statesSet.Contains(determinedTransitionFunctions[indexOfEmptySymbol][newStateIndex]))
                                {
                                    newStateIndex = statesIndexQueue.Dequeue();
                                };
                                if (newStateIndex == stateIndex)
                                {
                                    stateIndex = -1;
                                }
                                else
                                {
                                    stateIndex = newStateIndex;
                                }
                            }
                            eclosures.Add(States[i], new string(String.Join(",", statesSet)));
                        } else
                        {
                            eclosures.Add(States[i], States[i]);
                        }
                    }
                    determinedInputAlphabet.RemoveAt(indexOfEmptySymbol);
                    determinedTransitionFunctions.RemoveAt(indexOfEmptySymbol);
                    newStates = new List<string>() { eclosures[States[0]] };
                }
                else
                {
                    ISet<string> newSet = new HashSet<string>(determinedStates.GetRange(eclosures.Count, determinedStates.Count - eclosures.Count));
                    foreach (List<string> inputSymbolTransitionFunction in determinedTransitionFunctions)
                    {
                        if (eclosures.Count != 0)
                        {
                            for(int i = eclosures.Count; i < inputSymbolTransitionFunction.Count; i++)
                            {
                                if (!newSet.Contains(new string (inputSymbolTransitionFunction[i].OrderBy(ch => ch).ToArray())) && inputSymbolTransitionFunction[i] != "")
                                {
                                    newStates.Add(inputSymbolTransitionFunction[i]);
                                }
                            }
                        } else {
                            foreach (string transitionFunction in inputSymbolTransitionFunction)
                            {
                                if (transitionFunction.Contains(","))
                                {
                                    newStates.Add(transitionFunction);
                                }
                            }
                        }
                    }
                }

                foreach (string newState in newStates)
                {
                    string determinedState = newState.Replace(",", "");
                    if (eclosures.Count != 0)
                    {
                        string state = new string(determinedState.OrderBy(ch => ch).ToArray());
                        if (determinedStates.Contains(state) && eclosures.Count != 0 && determinedStates.IndexOf(state) >= States.Count)
                        {
                            // Убираем запятые в функциях перехода
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
                            continue;
                        }
                    }
                    if (determinedStates.Contains(determinedState) && eclosures.Count == 0)
                    {
                        // Убираем запятые в функциях перехода
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
                        continue;
                    }
                    // Сортировка символов по алфавиту
                    determinedState = new string(determinedState.OrderBy(ch => ch).ToArray());
                    determinedOutputAlphabet.Add("");
                    ISet<string> determinedStatesCharHashSet = newState.Split(",").ToHashSet<string>();
                    for (int i = 0; i < finishStates.Count; i++)
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

                    // Смотрим каждое состояние из newStates
                    // Добавляем функции перехода для нового состояния
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
                            if (eclosures.ContainsKey(transitionFunction))
                            {
                                transitionFunction = eclosures[transitionFunction];
                            } else
                            {
                                if (eclosures.Count != 0)
                                {
                                    List<string> transitionFunctionStates = transitionFunction.Split(",").ToList();
                                    transitionFunction = "";
                                    for (int i = 0; i < transitionFunctionStates.Count; i++)
                                    {
                                        if (transitionFunction == "")
                                        {
                                            transitionFunction += eclosures[transitionFunctionStates[i]];
                                        }
                                        else if (!transitionFunction.Contains(transitionFunctionStates[i]))
                                        {
                                            transitionFunction += "," + eclosures[transitionFunctionStates[i]];
                                        }
                                    }
                                }
                            }
                            if (inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1] == "")
                            {
                                inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1] += transitionFunction;
                            }
                            else
                            {
                                if (!inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1].Contains(transitionFunction))
                                {
                                    string[] transitions = transitionFunction.Split(",");
                                    foreach (string trans in transitions)
                                    {
                                        if (!inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1].Contains(trans))
                                        {
                                            inputSymbolTransitionFunctions[inputSymbolTransitionFunctions.Count - 1] += "," + trans;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    determinedStates.Add(determinedState);
                }
            } while (newStates.Count != 0);

            // Если были eps-замыкания, то удаляем ненужные состояния
            Dictionary<string, string> newStatesToDeterminedStates = new Dictionary<string, string>();
            for (int i = eclosures.Count; i < determinedStates.Count; i++)
            {
                newStatesToDeterminedStates.Add(determinedStates[i], "S" + (i - eclosures.Count));
            }
            List<List<string>> newTransitionFunctions = new List<List<string>>();
            foreach(List<string> inputSymbolTransitionFunctions in determinedTransitionFunctions)
            {
                newTransitionFunctions.Add(inputSymbolTransitionFunctions.GetRange(eclosures.Count, inputSymbolTransitionFunctions.Count - eclosures.Count));
            }
            determinedOutputAlphabet = determinedOutputAlphabet.GetRange(eclosures.Count, determinedOutputAlphabet.Count - eclosures.Count);

            // Обновляем функции переходов, добавляя новые названия для состояний
            foreach (List<string> inputSymbolTransitionFunctions in newTransitionFunctions)
            {
                for (int i = 0; i < inputSymbolTransitionFunctions.Count; i++)
                {
                    if (inputSymbolTransitionFunctions[i] != "")
                    {
                        inputSymbolTransitionFunctions[i] = newStatesToDeterminedStates[new string(inputSymbolTransitionFunctions[i].OrderBy(ch => ch).ToArray())];
                    }
                }
            }

            InputAlphabet = determinedInputAlphabet;
            States = newStatesToDeterminedStates.Values.ToList();
            TransitionFunctions = newTransitionFunctions;
            OutputAlphabet = determinedOutputAlphabet;
        }
    }
}