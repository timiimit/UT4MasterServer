namespace UT4MasterServer.Xmpp.XmlParser;

public class StateMachine<T>
{
	protected abstract class StateCondition
	{
		public int NextState { get; set; }

		public StateCondition(int to)
		{
			NextState = to;
		}
	}

	protected class StateConditionExact : StateCondition
	{
		public List<T> PossibleChars { get; set; }

		public StateConditionExact(int to, params T[] values) : base(to)
		{
			PossibleChars = new List<T>(values);
		}
	}

	protected class StateConditionAnyExcept : StateConditionExact
	{
		public StateConditionAnyExcept(int to, params T[] values) : base(to, values)
		{
		}
	}

	protected Dictionary<int, List<StateCondition>> automata;

	public StateMachine()
	{
		automata = new Dictionary<int, List<StateCondition>>();
	}

	public void AddGraph(int fromState, int toState, params T[] chars)
	{
		EnsurekeyExists(fromState, toState);
		automata[fromState].Add(new StateConditionExact(toState, chars));
	}

	public void AddGraphAnyExcept(int fromState, int toState, params T[] exceptChars)
	{
		EnsurekeyExists(fromState, toState);
		automata[fromState].Add(new StateConditionAnyExcept(toState, exceptChars));
	}

	protected void EnsurekeyExists(int fromState, int toState)
	{
		if (fromState < 0 || toState < 0)
		{
			throw new ArgumentOutOfRangeException("parameters fromState and toState must both be positive.");
		}

		if (!automata.ContainsKey(fromState))
		{
			automata.Add(fromState, new List<StateCondition>());
		}
	}

	protected virtual int CheckCondition(int currentState, T value, StateCondition condition)
	{
		if (condition is StateConditionAnyExcept except)
		{
			if (!except.PossibleChars.Contains(value))
			{
				return except.NextState;
			}
		}
		else if (condition is StateConditionExact exact)
		{
			if (exact.PossibleChars.Contains(value))
			{
				return exact.NextState;
			}
		}
		return -1;
	}

	public int NextChar(int currentState, T value)
	{
		if (!automata.ContainsKey(currentState))
			return -1;

		foreach (var condition in automata[currentState])
		{
			var nextState = CheckCondition(currentState, value, condition);
			if (nextState >= 0)
			{
				return nextState;
			}
		}

		return -1;
	}
}
