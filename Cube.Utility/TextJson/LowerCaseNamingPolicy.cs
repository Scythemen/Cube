using System.Text.Json;

namespace Cube.Utility.TextJson
{
    public class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name?.ToLowerInvariant();
        }
    }
}
