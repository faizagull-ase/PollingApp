using System.Security.Cryptography;

namespace PulsePoll.Services.PollCodes;

public interface IPollCodeGenerator
{
    string Generate();
}

public class PollCodeGenerator : IPollCodeGenerator
{
    // Excludes visually ambiguous characters (0/O, 1/I/L).
    private const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    private const int Length = 5;

    public string Generate()
    {
        Span<char> code = stackalloc char[Length];
        for (var i = 0; i < Length; i++)
        {
            code[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(code);
    }
}
