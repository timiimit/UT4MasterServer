using System.Text;
using System.Xml;

namespace UT4MasterServer.Xmpp.Stanzas;

public class StanzaError
{
    public enum ConditionValues
    {
        BadRequest,
        Conflict,
        FeatureNotImplemented,
        Forbidden,
        Gone,
        InternalServerError,
        ItemNotFound,
        JidMalformed,
        NotAcceptable,
        NotAllowed,
        NotAuthorized,
        PolicyViolation,
        RecipientUnavailable,
        Redirect,
        RegistrationRequired,
        RemoteServerNotFound,
        RemoteServerTimeout,
        ResourceConstraint,
        ServiceUnavailable,
        SubscriptionRequired,
        UndefinedCondition,
        UnexpectedRequest
    }

    private readonly ConditionValues condition;

    public ConditionValues Condition { get => condition; }

    public StanzaError(ConditionValues condition)
    {
        this.condition = condition;
    }

    public async Task WriteAsync(XmppWriter writer, CancellationToken cancellationToken)
    {
        await Task.Yield();

        string type;
        switch (Condition)
        {
            case ConditionValues.BadRequest:
            case ConditionValues.JidMalformed:
            case ConditionValues.NotAcceptable:
            case ConditionValues.PolicyViolation: // or "wait". optionally provide additional <text> tag in <error/> tag after <policy-violation/> -> <text>POLICY DESCRIPTION HERE</text>
            case ConditionValues.Redirect: // include new address in "redirect" tag
                type = "modify";
                break;
            case ConditionValues.Conflict:
            case ConditionValues.FeatureNotImplemented: // or "modify"
            case ConditionValues.Gone: // include new address in "gone" tag
            case ConditionValues.InternalServerError:
            case ConditionValues.ItemNotFound:
            case ConditionValues.NotAllowed:
            case ConditionValues.RemoteServerNotFound:
            case ConditionValues.ServiceUnavailable:
                type = "cancel";
                break;
            case ConditionValues.Forbidden:
            case ConditionValues.NotAuthorized:
            case ConditionValues.RegistrationRequired:
            case ConditionValues.SubscriptionRequired:
                type = "auth";
                break;
            case ConditionValues.RecipientUnavailable:
            case ConditionValues.RemoteServerTimeout: // generally "wait", but can be something else if more applicable
            case ConditionValues.ResourceConstraint:
            case ConditionValues.UnexpectedRequest: // or "modify"
                type = "wait";
                break;
            case ConditionValues.UndefinedCondition: // any type. should include app specific error as well.
            default:
                type = "cancel";
                break;
        }

        // transform enum name to xml element name
        string errorName = Condition.ToString();
        StringBuilder sb = new StringBuilder();
        sb.Append(char.ToLower(errorName[0]));
        for (int i = 1; i < errorName.Length; i++)
        {
            if (char.IsUpper(errorName[i]))
            {
                sb.Append('-');
                sb.Append(char.ToLower(errorName[i]));
            }
            else
            {
                sb.Append(errorName[i]);
            }
        }
        errorName = sb.ToString();

        writer.OpenTag("error");
        writer.Attribute("type", type);
        {
            writer.OpenTagNS(errorName, "urn:ietf:params:xml:ns:xmpp-stanzas");
            writer.CloseTag();
        }
        writer.CloseTag();
    }

    public static async Task<StanzaError?> ReadAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        await Task.Yield();

        try
        {
            if (reader.Name != "error" || reader.NodeType != XmlNodeType.Element)
                return null;

            var type = reader.GetAttribute("type");
            if (type == null)
                return null;

            // TODO: parse type

            reader.Read();

            var condition = reader.Name;
            // TODO: parse condition

            // skip any unhandled or application-specific errors
            while (reader.Depth > 3)
            {
                reader.Read();
            }

            if (reader.Name != "error" || reader.NodeType != XmlNodeType.EndElement)
                return null;

            return new StanzaError(ConditionValues.BadRequest);
        }
        finally
        {
            while (reader.Depth > 2)
            {
                reader.Read();
            }
        }
    }
}
