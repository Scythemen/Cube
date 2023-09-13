using System;

namespace Cube.Utility.SequentialGuid
{
    public class SequentialGuid
    {
        private static IGenerator generator = new SequentialGuidGenerator(new SequentialGuidOptions() { GuidType = SequentialGuidType.SequentialAsString });

        public static Guid NewGuid()
        {
            return generator.NewGuid();
        }

    }
}
