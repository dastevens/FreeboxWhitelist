using GSF.Media;
using GSF.Media.Sound;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeboxWhitelist
{
    class Program
    {
        static void Main(string[] args)
        {
            var allowedNumbers = GetAllowedNumbers();

            var allowedPrefixes = allowedNumbers
                .Select(allowedNumber => allowedNumber.Substring(0, Math.Min(GetMaxFilterLength(), allowedNumber.Length)))
                .ToArray();

            var rejectedPrefixes = CalculatedRejectedPrefixes(allowedPrefixes).OrderBy(a => a).ToArray();

            foreach (var rejectedPrefix in rejectedPrefixes)
            {
                Console.WriteLine(rejectedPrefix);
            }
            Console.WriteLine($"{rejectedPrefixes.Length} prefixes");

            int errors = 0;
            foreach (var allowedNumber in allowedPrefixes)
            {
                if (StartsWithAny(rejectedPrefixes, allowedNumber))
                {
                    Console.Error.WriteLine($"Rejected allowed number {allowedNumber}");
                    errors++;
                }
            }

            if (errors == 0)
            {
                //ProgramFreebox(rejectedPrefixes);
            }

            var tested = 0;
            var failed = 0;
            while (true)
            {
                var randomNumber = GenerateNumber();
                tested++;
                if (!StartsWithAny(rejectedPrefixes, randomNumber))
                {
                    if (!StartsWithAny(allowedNumbers, randomNumber))
                    {
                        failed++;
                        var failureRate = ((double)failed) / ((double)tested);
                        Console.Error.WriteLine($"Failed to reject number {randomNumber} (failed for 1 in {(int)(1d / failureRate)} numbers)");
                    }
                }
            }
        }

        static readonly string[] AllNumbers = new[]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        };

        static string[] CalculatedRejectedPrefixes(string[] allowedNumbers)
        {
            var rejectedPrefixes = AllNumbers;

            foreach (var allowedNumber in allowedNumbers)
            {
                rejectedPrefixes = AllowNumber(rejectedPrefixes, allowedNumber)
                    .ToArray();
            }

            return rejectedPrefixes;
        }

        static string[] AllowNumber(string[] rejectedPrefixes, string allowedNumber)
        {
            var toChange = rejectedPrefixes.Where(rejectedPrefix => allowedNumber.StartsWith(rejectedPrefix)).ToArray();
            if (!toChange.Any())
            {
                return rejectedPrefixes;
            }

            var unchanged = rejectedPrefixes.Where(rejectedPrefix => !allowedNumber.StartsWith(rejectedPrefix)).ToArray();
            var extended = toChange.SelectMany(prefix => AllNumbers.Select(number => prefix + number))
                .Where(prefix => prefix.Length <= allowedNumber.Length && prefix != allowedNumber).ToArray();
            return AllowNumber(unchanged.Concat(extended).ToArray(), allowedNumber);
        }

        static Random random = new Random();
        static string GenerateNumber()
        {
            var rejectedNumber = new StringBuilder("0");
            for (var i = 0; i < 9; i++)
            {
                var nextNumber = random.Next(0, 10);
                rejectedNumber.Append(nextNumber.ToString());
            }
            return rejectedNumber.ToString();
        }

        static bool StartsWithAny(string[] prefixes, string number)
        {
            return prefixes.Any(rejectedPrefix => number.StartsWith(rejectedPrefix));
        }

        static void ProgramFreebox(string[] prefixes)
        {
            WriteToFreebox("*351#");    // Activate filter service
            WriteToFreebox("#351#");    // Clear list
            foreach (var prefix in prefixes)
            {
                WriteToFreebox("*351*" + prefix + "#");
            }
        }

        static void WriteToFreebox(string freeboxCode)
        {
            Console.WriteLine($"Enter to send {freeboxCode}...");
            Console.ReadLine();

            WaveFile waveFile = new WaveFile(SampleRate.Hz8000, BitsPerSample.Bits16, DataChannels.Mono);
            var touchTones = TouchTone.GetTouchTones(freeboxCode);
            DTMF.Generate(waveFile, touchTones, 1);

            Console.WriteLine($"Dialing {freeboxCode}...");
            waveFile.Play();
            Task.Delay(waveFile.AudioLength + TimeSpan.FromSeconds(1)).Wait();
        }

        static string[] GetAllowedNumbers()
        {
            return AllKeys
                .Where(key => key.StartsWith("Number:"))
                .Select(key => key.Substring("Number:".Length))
                .ToArray();
        }

        static int GetMaxFilterLength() => int.Parse(AppSetting("MaxFilterLength", "5"));

        static IEnumerable<string> AllKeys => ConfigurationManager.AppSettings.AllKeys;

        static string AppSetting(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

    }
}
