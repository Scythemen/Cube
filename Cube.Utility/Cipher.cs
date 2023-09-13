
namespace Cube.Utility
{
    public class Cipher
    {
        public static byte[] EncryptAes(byte[] original, byte[] key)
        {// note: original.Length % 16 ==0
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;
                aes.Key = key;
                var transform = aes.CreateEncryptor();
                var res = transform.TransformFinalBlock(original, 0, original.Length);
                return res;
            }
        }

        public static byte[] DecryptAes(byte[] cipher, byte[] key)
        {
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;
                aes.Key = key;
                var transform = aes.CreateDecryptor();
                return transform.TransformFinalBlock(cipher, 0, cipher.Length);
            }
        }


        private void Test()
        {
            //========================
            //== algorithm: AES / ECB / NoPadding
            //== key(16bit): 12345678901234567890123456789012
            //== content: 1234567890072017010203040506780007654343217E00000000000000000000
            //== result: 0083F48205C6B2C67C44648BAE0C78B24EE7691B704A25213CEC9F301635DE60
            //========================

            // string key = "12345678901234567890123456789012";// "A-16-Byte-String";
            // //string IV_STRING = "112233445566778899AABBCCDDEEFF00";// "A-16-Byte-String";
            //
            // Debug.WriteLine($"from java encrypted result :  0083F48205C6B2C67C44648BAE0C78B24EE7691B704A25213CEC9F301635DE60");
            //
            // //  note:
            // // because of algorithm: AES / ECB / NoPadding
            // // the length of plain content bytes must on a multiple of 16
            // // that means original string must be: original.Length % 32 ==0
            // string original = "1234567890072017010203040506780007654343217E00000000000000000000";
            //
            // Debug.WriteLine($"original: {original}");
            //
            // var en = EncryptAES(original.HexToBytes(), key.HexToBytes());
            // Debug.WriteLine($"encrypt: {en.ToHex()}");
            //
            // var de = DecryptAES(en, key.HexToBytes());
            //
            // Debug.WriteLine($"decrypt: {de.ToHex() }");

        }
    }
}
