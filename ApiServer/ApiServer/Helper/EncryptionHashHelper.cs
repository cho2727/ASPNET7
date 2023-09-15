using System.Security.Cryptography;
using System.Text;

namespace ApiServer.Helper;

public class EncryptionHashHelper
{
    public static string EncryptPassword(string text, bool isUppercase = false)
    {
        string result = string.Empty;
        using (var algo = System.Security.Cryptography.SHA256.Create())
        {
            result = GenerateHashString(algo, text, isUppercase);
        }

        return result;
    }

    private static string GenerateHashString(HashAlgorithm algo, dynamic input, bool isUppercase)
    {
        if (input is string)
            input = Encoding.UTF8.GetBytes(input);

        if (!(input is Stream) && !(input is byte[]))
            throw new Exception($"can not GenerateHashString type  = {input}");

        // Compute hash from text parameter
        algo.ComputeHash(input);

        // Get has value in array of bytes
        var result = algo.Hash;

        if (result is null) throw new Exception($"can not ComputeHash");

        // Return as hexadecimal string
        return string.Join(
            string.Empty,
            result.Select(x => x.ToString(isUppercase ? "X2" : "x2")));
    }
}