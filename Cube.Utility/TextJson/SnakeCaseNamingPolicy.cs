using System.Text.Json;
using System.Text.RegularExpressions;

namespace Cube.Utility.TextJson
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {

        static Regex reg = new Regex(@"((?<=.)[A-Z][a-zA-Z]*)|((?<=[a-zA-Z])\d+)");

        public override string ConvertName(string name)
        {
            name = reg.Replace(name, @"_$1$2").ToLowerInvariant();
            return name;
        }

    }
}
