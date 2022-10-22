namespace MealyMooreMinimization
{
    internal class MealyMachineInfo : IMinimizableMachineInfo
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

        public void Minimize()
        {
            DeleteUnreachableStates();
            Dictionary<string, HashSet<string>> previousMatchingMinimizedStatesToStates = new Dictionary<string, HashSet<string>>();
            Dictionary<string, HashSet<string>> matchingMinimizedStatesToStates = new Dictionary<string, HashSet<string>>();
            foreach (string state in States)
            {
                matchingMinimizedStatesToStates.Add(state, States.ToHashSet());
            }
            List<List<string>> newTransitionFunctions = TransitionFunctions;

            do
            {
                previousMatchingMinimizedStatesToStates = matchingMinimizedStatesToStates;
                matchingMinimizedStatesToStates = GetMatchingMinimizedStatesToStates(previousMatchingMinimizedStatesToStates, newTransitionFunctions);
                newTransitionFunctions = GetNewTransitionFunctions(matchingMinimizedStatesToStates, TransitionFunctions);
            } while (matchingMinimizedStatesToStates.Count != previousMatchingMinimizedStatesToStates.Count && States.Count != matchingMinimizedStatesToStates.Count);

            List<string> minimizedStates = matchingMinimizedStatesToStates.Keys.ToList();
            List<List<string>> minimizedTransitionFunctions = GetMinimizedTransitionFunctions(matchingMinimizedStatesToStates);
            States = minimizedStates;
            TransitionFunctions = minimizedTransitionFunctions;
        }

        private void DeleteUnreachableStates()
        {
            HashSet<string> reachableStates = new HashSet<string>();

            foreach (List<string> innerStateTransitionFunctions in TransitionFunctions)
            {
                foreach (string transitionFunction in innerStateTransitionFunctions)
                {
                    if (transitionFunction.Split("/")[0] != States[innerStateTransitionFunctions.IndexOf(transitionFunction)] || innerStateTransitionFunctions.IndexOf(transitionFunction) == 0)
                    {
                        reachableStates.Add(transitionFunction.Split("/")[0]);
                    }
                }
            }

            List<string> removedStates = new List<string>();
            if (reachableStates.Count != States.Count)
            {
                foreach (string state in States)
                {
                    if (!reachableStates.Contains(state))
                    {
                        removedStates.Add(state);
                    }
                }
            }
            foreach (string state in removedStates)
            {
                int indexOfRemovedState = States.IndexOf(state);
                States.Remove(state);
                foreach(List<string> innerStateTransitionFunctions in TransitionFunctions)
                {
                    innerStateTransitionFunctions.RemoveAt(indexOfRemovedState);
                }
            }
        }

        private Dictionary<string, HashSet<string>> GetMatchingMinimizedStatesToStates(Dictionary<string, HashSet<string>> matchingEquivalenceClassesToStates, List<List<string>> transitionFunctions)
        {
            Dictionary<string, HashSet<string>> matchingNewStatesToPreviousStates = new Dictionary<string, HashSet<string>>();
            Dictionary<string, List<string>> matchingNewStatesToTransitionFunctions = new Dictionary<string, List<string>>();

            for (int i = 0; i < States.Count; i++)
            {
                List<string> transitionsSequence = new List<string>();
                foreach (List<string> innerStateTransitionFunctions in transitionFunctions)
                {
                    if (innerStateTransitionFunctions[i].Contains("/"))
                    {
                        transitionsSequence.Add(innerStateTransitionFunctions[i].Split("/")[1]);
                    }
                    else
                    {
                        transitionsSequence.Add(innerStateTransitionFunctions[i]);
                    }
                }
                bool isExistMinimizedState = false;
                foreach (KeyValuePair<string, List<string>> matchingNewStateToTransitionFunctions in matchingNewStatesToTransitionFunctions)
                {
                    if (matchingNewStateToTransitionFunctions.Value.SequenceEqual(transitionsSequence))
                    {
                        string firstElementOfEquivalenceClass = matchingNewStatesToPreviousStates[matchingNewStateToTransitionFunctions.Key].First();
                        string firstEquivalenceClass = "";
                        string secondEquivalenceClass = "";
                        foreach (KeyValuePair<string, HashSet<string>> matchingEquivalenceClassToStates in matchingEquivalenceClassesToStates)
                        {
                            if (matchingEquivalenceClassToStates.Value.Contains(firstElementOfEquivalenceClass))
                            {
                                firstEquivalenceClass = matchingEquivalenceClassToStates.Key;
                            }
                            if (matchingEquivalenceClassToStates.Value.Contains(States[i]))
                            {
                                secondEquivalenceClass = matchingEquivalenceClassToStates.Key;
                            }
                        }
                        if (firstEquivalenceClass == secondEquivalenceClass)
                        {
                            matchingNewStatesToPreviousStates[matchingNewStateToTransitionFunctions.Key].Add(States[i]);
                            isExistMinimizedState = true;
                            break;
                        }
                    }
                }
                if (!isExistMinimizedState)
                {
                    string newState = "q" + matchingNewStatesToPreviousStates.Count.ToString();
                    matchingNewStatesToPreviousStates.Add(newState, new HashSet<string>() { States[i] });
                    matchingNewStatesToTransitionFunctions.Add(newState, transitionsSequence);
                }
            }

            return matchingNewStatesToPreviousStates;
        }

        private List<List<string>> GetNewTransitionFunctions(Dictionary<string, HashSet<string>> matchingNewStatesToStates, List<List<string>> oldTransitionFunctions)
        {
            List<List<string>> newTransitionFunctions = new List<List<string>>();

            foreach (List<string> innerStateTransitionFunctions in oldTransitionFunctions)
            {
                List<string> newInnerStateTransitionFunctions = new List<string>();
                foreach (string oldTransitionFunction in innerStateTransitionFunctions)
                {
                    string oldState = oldTransitionFunction.Contains("/") ? oldTransitionFunction.Split("/")[0] : oldTransitionFunction;
                    foreach (KeyValuePair<string, HashSet<string>> matchingNewStateToStates in matchingNewStatesToStates)
                    {
                        if (matchingNewStateToStates.Value.Contains(oldState))
                        {
                            newInnerStateTransitionFunctions.Add(matchingNewStateToStates.Key);
                        }
                    }
                }
                newTransitionFunctions.Add(newInnerStateTransitionFunctions);
            }

            return newTransitionFunctions;
        }

        private List<List<string>> GetMinimizedTransitionFunctions(Dictionary<string, HashSet<string>> matchingMinimizedStatesToStates)
        {
            List<List<string>> minimizedTransitionFunctions = new List<List<string>>();

            foreach (KeyValuePair<string, HashSet<string>> matchingMinimizedStateToStates in matchingMinimizedStatesToStates)
            {
                List<string> innerStateMinimizedTransitionFunctions = new List<string>();
                foreach (List<string> innerStateTransitionFunctions in TransitionFunctions)
                {
                    foreach (KeyValuePair<string, HashSet<string>> localMatchingMinimizedStateToStates in matchingMinimizedStatesToStates)
                    {
                        if (localMatchingMinimizedStateToStates.Value.Contains(innerStateTransitionFunctions[States.IndexOf(matchingMinimizedStateToStates.Value.First())].Split("/")[0]))
                        {
                            innerStateMinimizedTransitionFunctions.Add(localMatchingMinimizedStateToStates.Key + "/" + (innerStateTransitionFunctions[States.IndexOf(matchingMinimizedStateToStates.Value.First())].Split("/")[1]));
                        }
                    }
                }
                if (minimizedTransitionFunctions.Count != innerStateMinimizedTransitionFunctions.Count)
                {
                    foreach (string transitionFunction in innerStateMinimizedTransitionFunctions)
                    {
                        minimizedTransitionFunctions.Add(new List<string>() { transitionFunction });
                    }
                }
                else
                {
                    for (int i = 0; i < minimizedTransitionFunctions.Count; i++)
                    {
                        minimizedTransitionFunctions[i].Add(innerStateMinimizedTransitionFunctions[i]);
                    }
                }
            }

            return minimizedTransitionFunctions;
        }
    }
}