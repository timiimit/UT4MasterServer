namespace UT4MasterServer.Xmpp.XmlParser;

public class CharStateMachine : StateMachine<char>
{
	private class StateConditionRanges : StateCondition
	{
		public List<(char min, char max)> PossibleRanges { get; set; }

		public StateConditionRanges(int to, params (char min, char max)[] ranges) : base(to)
		{
			PossibleRanges = new List<(char min, char max)>(ranges);
		}
	}

	public CharStateMachine()
	{
	}

	public void AddGraphRanges(int fromState, int toState, params (char min, char max)[] ranges)
	{
		EnsurekeyExists(fromState, toState);
		automata[fromState].Add(new StateConditionRanges(toState, ranges));
	}


	protected override int CheckCondition(int currentState, char value, StateCondition condition)
	{
		if (condition is StateConditionRanges ranges)
		{
			foreach ((char min, char max) range in ranges.PossibleRanges)
			{
				if (value >= range.min && value <= range.max)
				{
					return ranges.NextState;
				}
			}
		}
		return base.CheckCondition(currentState, value, condition);
	}
}
