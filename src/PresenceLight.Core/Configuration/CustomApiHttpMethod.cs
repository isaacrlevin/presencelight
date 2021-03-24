using System.Collections.Generic;

namespace PresenceLight.Core
{
    public static class CustomApiHttpMethod
    {
        public const string None = "";
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Delete = "DELETE";

        public static IReadOnlyList<string> AllMethods { get; } = new List<string>
            {
                None,
                Get,
                Post,
                Delete,
            };
    }
}
