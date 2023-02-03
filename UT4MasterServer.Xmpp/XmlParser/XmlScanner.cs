using System.Text;

namespace UT4MasterServer.Xmpp.XmlParser;

internal enum XmlToken
{
    OpenProcInst,  // <?
    CloseProcInst, // ?>
    Open,       // <
    Close,      // >
    OpenEnd,    // </
    CloseEmpty, // />
    Equal,  // =
    Colon,  // :
    String, // "string" or 'string'
    Name,   // name
    Whitespace
}

internal class XmlScanner
{
    CharStateMachine stateMachine;
    Dictionary<int, XmlToken> stateToToken;
    StringBuilder lexem;
    int line;
    int column;

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
        stateMachine.AddGraph(0, 8, '>'); AddFinal(8, XmlToken.CloseProcInst);

        stateMachine.AddGraph(0, 9, '='); AddFinal(9, XmlToken.Equal);
        stateMachine.AddGraph(0, 10, ':'); AddFinal(10, XmlToken.Colon);

        stateMachine.AddGraph(0, 11, '\"');
        stateMachine.AddGraphAnyExcept(11, 12, '\"');
        stateMachine.AddGraphAnyExcept(12, 12, '\"'); AddFinal(12, XmlToken.String);

        stateMachine.AddGraph(0, 13, '\'');
        stateMachine.AddGraphAnyExcept(13, 14, '\'');
        stateMachine.AddGraphAnyExcept(14, 14, '\''); AddFinal(14, XmlToken.String);

        stateMachine.AddGraph(0, 15, ' ', '\t', '\n', '\r');
        stateMachine.AddGraph(15, 15, ' ', '\t', '\n', '\r'); AddFinal(15, XmlToken.Whitespace);

        // NAME

        // NOTE: NAME can supposedly also start with colon and a bunch of
        //       other character ranges. we don't handle these cases.

        stateMachine.AddGraphRanges(0, 16, ('A', 'Z'), ('a', 'z'));
        stateMachine.AddGraph(0, 16, '_'); AddFinal(16, XmlToken.Name);

        stateMachine.AddGraphRanges(16, 17, ('A', 'Z'), ('a', 'z'), ('0', '9'));
        stateMachine.AddGraph(16, 17, '_', '-'); AddFinal(17, XmlToken.Name);
    }

    private void AddFinal(int state, XmlToken token)
    {
        stateToToken.Add(state, token);
    }

    public XmlLexem? NextChar(char c)
    {
        if (stateMachine.NextChar(c))
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
            return null;
        }

        var finalState = stateMachine.State;

        // reset machine state
        stateMachine.State = 0;

        return new XmlLexem { Token = stateToToken[finalState], Value = lexem.ToString(), Line = line, Column = column };
    }

}
