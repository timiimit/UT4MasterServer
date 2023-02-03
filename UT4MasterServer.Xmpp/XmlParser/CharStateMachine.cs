namespace UT4MasterServer.Xmpp.XmlParser;

public class CharStateMachine
{
    private abstract class StateCondition
    {
        public int NextState { get; set; }

        public StateCondition(int to)
        {
            NextState = to;
        }
    }

    private class StateConditionExact : StateCondition
    {
        public List<char> PossibleChars { get; set; }

        public StateConditionExact(int to, params char[] chars) : base(to)
        {
            PossibleChars = new List<char>(chars);
        }
    }

    private class StateConditionRanges : StateCondition
    {
        public List<(char min, char max)> PossibleRanges { get; set; }

        public StateConditionRanges(int to, params (char min, char max)[] ranges) : base(to)
        {
            PossibleRanges = new List<(char min, char max)>(ranges);
        }
    }

    private class StateConditionAnyExcept : StateConditionExact
    {
        public StateConditionAnyExcept(int to, params char[] chars) : base(to)
        {
        }
    }

    Dictionary<int, List<StateCondition>> automata;
    public int State { get; set; }

    public CharStateMachine()
    {
        automata = new Dictionary<int, List<StateCondition>>();
        State = 0;
    }

    public void AddGraph(int fromState, int toState, params char[] chars)
    {
        EnsurekeyExists(fromState);
        automata[fromState].Add(new StateConditionExact(toState, chars));
    }

    public void AddGraphRanges(int fromState, int toState, params (char min, char max)[] ranges)
    {
        EnsurekeyExists(fromState);
        automata[fromState].Add(new StateConditionRanges(toState, ranges));
    }

    public void AddGraphAnyExcept(int fromState, int toState, params char[] exceptChars)
    {
        EnsurekeyExists(fromState);
        automata[fromState].Add(new StateConditionAnyExcept(toState, exceptChars));
    }

    private void EnsurekeyExists(int fromState)
    {
        if (!automata.ContainsKey(fromState))
        {
            automata.Add(fromState, new List<StateCondition>());
        }
    }

    public bool NextChar(char c)
    {
        if (!automata.ContainsKey(State))
            return false;

        foreach (var condition in automata[State])
        {
            if (condition is StateConditionExact exact)
            {
                if (exact.PossibleChars.Contains(c))
                {
                    State = exact.NextState;
                    return true;
                }
            }
            else if (condition is StateConditionRanges ranges)
            {
                foreach (var range in ranges.PossibleRanges)
                {
                    if (c >= range.min && c <= range.max)
                    {
                        State = ranges.NextState;
                        return true;
                    }
                }
            }
            else if (condition is StateConditionAnyExcept except)
            {
                if (!except.PossibleChars.Contains(c))
                {
                    State = except.NextState;
                    return true;
                }
            }
        }

        return false;
    }
}
