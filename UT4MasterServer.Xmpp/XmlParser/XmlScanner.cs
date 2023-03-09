using System.Text;

namespace UT4MasterServer.Xmpp.XmlParser;

public enum XmlToken
{
	OpenProcInst,  // <?
	CloseProcInst, // ?>
	Open,       // <
	Close,      // >
	OpenEnd,    // </
	CloseEmpty, // />
	Equal,  // =
	String, // "string" or 'string'
	Name,   // name
	Whitespace,
	Other
}

public class XmlScanner
{
	CharStateMachine stateMachine;
	Dictionary<int, XmlToken> stateToToken;
	StringBuilder lexem;
	int line;
	int column;

	public XmlLexem? LastLexem { get; private set; }

	public XmlScanner()
	{
		stateMachine = new CharStateMachine();
		stateToToken = new Dictionary<int, XmlToken>();
		lexem = new StringBuilder();

		DefineGrammar();
	}

	private void DefineGrammar()
	{
		stateMachine.AddGraph(0, 1, '<'); AddFinal(1, XmlToken.Open);
		stateMachine.AddGraph(1, 2, '/'); AddFinal(2, XmlToken.OpenEnd);
		stateMachine.AddGraph(1, 3, '?'); AddFinal(3, XmlToken.OpenProcInst);

		stateMachine.AddGraph(0, 4, '>'); AddFinal(4, XmlToken.Close);

		stateMachine.AddGraph(0, 5, '/');
		stateMachine.AddGraph(5, 6, '>'); AddFinal(6, XmlToken.CloseEmpty);

		stateMachine.AddGraph(0, 7, '?');
		stateMachine.AddGraph(7, 8, '>'); AddFinal(8, XmlToken.CloseProcInst);

		stateMachine.AddGraph(0, 9, '='); AddFinal(9, XmlToken.Equal);

		// STRING - should supposedly be "normalized" before parsed

		stateMachine.AddGraph(0, 11, '\"');
		stateMachine.AddGraphAnyExcept(11, 12, '\"');
		stateMachine.AddGraphAnyExcept(12, 12, '\"');
		stateMachine.AddGraph(11, 18, '\"');
		stateMachine.AddGraph(12, 18, '\"');
		AddFinal(18, XmlToken.String);

		stateMachine.AddGraph(0, 13, '\'');
		stateMachine.AddGraphAnyExcept(13, 14, '\'');
		stateMachine.AddGraphAnyExcept(14, 14, '\'');
		stateMachine.AddGraph(13, 19, '\'');
		stateMachine.AddGraph(14, 19, '\'');
		AddFinal(19, XmlToken.String);

		// WHITESPACE - should generally be converted into a single space

		stateMachine.AddGraph(0, 15, ' ', '\t', '\n', '\r');
		stateMachine.AddGraph(15, 15, ' ', '\t', '\n', '\r');
		AddFinal(15, XmlToken.Whitespace);

		// NAME - can supposedly also start with colon and a bunch of
		//        other character ranges. we don't handle these cases.

		stateMachine.AddGraphRanges(0, 16, ('A', 'Z'), ('a', 'z'));
		stateMachine.AddGraph(0, 16, '_', ':');
		AddFinal(16, XmlToken.Name);

		stateMachine.AddGraphRanges(16, 17, ('A', 'Z'), ('a', 'z'), ('0', '9'));
		stateMachine.AddGraph(16, 17, '_', '-');
		stateMachine.AddGraphRanges(17, 17, ('A', 'Z'), ('a', 'z'), ('0', '9'));
		stateMachine.AddGraph(17, 17, '_', '-');
		AddFinal(17, XmlToken.Name);

		// detect any other character
		stateMachine.AddGraphRanges(0, 20, (char.MinValue, char.MaxValue));
		AddFinal(20, XmlToken.Other);
	}

	private void AddFinal(int state, XmlToken token)
	{
		stateToToken.Add(state, token);
	}

	public int NextChar(int currentState, char c)
	{
		int nextState = stateMachine.NextChar(currentState, c);

		if (nextState >= 0)
		{
			lexem.Append(c);
			if (c == '\n')
			{
				column = 0;
				line++;
			}
			else
			{
				column++;
			}
			return nextState;
		}


		try
		{
			if (!stateToToken.ContainsKey(currentState))
				return -1;

			LastLexem = new XmlLexem { Token = stateToToken[currentState], Value = lexem.ToString(), Line = line, Column = column };
			return 0;
		}
		finally
		{
			lexem.Clear();
		}
	}
}
