namespace Cube.Utility.ButterflyUid
{
    public class Buid
    {
        readonly static ButterflyUid butterflyUid = new ButterflyUid(0);


        private Buid()
        {
        }

        public static long Next()
        {
            return butterflyUid.Next();
        }


    }
}
