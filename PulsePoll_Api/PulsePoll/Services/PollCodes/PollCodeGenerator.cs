using System.Security.Cryptography;
using PulsePoll.Services.Caching;

namespace PulsePoll.PollCodes;

// 5 char alphanumeric, excludes 0/O and 1/I/l; collision + reuse-delay aware
public class PollCodeGenerator
{
    private const string Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const int MaxAttempts = 50;
    private const int CodeLength = 5;

    private readonly IPollCacheStore _pollCacheStore;

    public PollCodeGenerator(IPollCacheStore pollCacheStore)
    {
        _pollCacheStore = pollCacheStore;
    }

    public string Generate()
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var code = GenerateCode(CodeLength);
            // check whether cachestore has pollcode already or not 
            if (_pollCacheStore.Get(code) is null)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique poll code after multiple attempts.");
    }

    private static string GenerateCode(int length)
    {
        Span<char> chars = stackalloc char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(0, Alphabet.Length)];
        }
        return new string(chars);
    }
}
