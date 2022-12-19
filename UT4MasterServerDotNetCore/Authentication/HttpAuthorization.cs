using System.Net;

namespace UT4MasterServer.Authorization;

public class HttpAuthorization
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public bool IsBearer { get => Type == "bearer"; }
        public bool IsBasic { get => Type == "basic"; }

        public HttpAuthorization(HttpListenerRequest request) : this(request.Headers["Authorization"])
        {
        }

        public HttpAuthorization(string? authorizationHeader)
        {
            if (authorizationHeader == null)
                return;

            int space = authorizationHeader.IndexOf(' ');
            if (space == -1)
            {
                Value = authorizationHeader;
                return;
            }

            Type = authorizationHeader.Substring(0, space);
            Value = authorizationHeader.Substring(space + 1);
        }
    }
