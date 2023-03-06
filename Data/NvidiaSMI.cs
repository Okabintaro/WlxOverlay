using System.Diagnostics;
using System.Globalization;

namespace WlxOverlay.Data;

// TODO: Create Interface for AMD/Intel maybe
class NvidiaSMI : IDisposable
{
    public NvidiaSMI()
    {
        if (Instance != null)
            throw new InvalidOperationException("Can't have more than one Nvidia-SMI!");
        Instance = this;
    }

    public static NvidiaSMI? Instance;
    public Process? process;

    // TODO: Add more descriptive messages?, Dict?
    // TODO: Make configurable?
    private readonly String[] props = { "temperature.gpu", "memory.used", "memory.total", "power.draw", "power.limit" };

    // TODO: Could probably be typed enum as keys?
    public Dictionary<string, float>? Values;
    public delegate void StatsDictDelegate(Dictionary<string, float> newValue);
    public event StatsDictDelegate? StatsUpdated;

    private void ParseInput(string? input)
    {
        if (input is not null)
        {
            var splitInput = input.Split(",", StringSplitOptions.TrimEntries).Select(value => float.Parse(value, CultureInfo.InvariantCulture.NumberFormat));
            Values = props.Zip(splitInput).ToDictionary(x => x.First, y => y.Second);
            if (StatsUpdated is not null)
            {
                StatsUpdated(Values);
            }
        }
    }

    public void Start()
    {
        process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.EnableRaisingEvents = true;

        process.StartInfo.FileName = "nvidia-smi";
        process.StartInfo.Arguments = $"--format=csv,nounits,noheader -l 1 --query-gpu={String.Join(",", props)}";
        bool wasStarted = process.Start();
        if (wasStarted)
        {
            process.OutputDataReceived += (sender, args) => ParseInput(args.Data);
            process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine("Error from nvidia-smi: {0}", args.Data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
    }

    public void Dispose()
    {
        if (process != null)
        {
            Console.WriteLine($"Stopping process...");
            process.Kill();
            process.WaitForExit();
            process = null;
        }
    }
}
