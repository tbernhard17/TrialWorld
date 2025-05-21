using System;
using System.Collections.Generic;
using System.Linq;

namespace TrialWorld.Core.Models;

public class FilterChain
{
    protected readonly List<Filter> _filters = new();

    public void AddFilter(string name, string value)
    {
        _filters.Add(new Filter(name, value));
    }

    public void RemoveFilter(string name)
    {
        _filters.RemoveAll(f => f.Name == name);
    }

    public void ClearFilters()
    {
        _filters.Clear();
    }

    public IReadOnlyList<Filter> GetFilters() => _filters.AsReadOnly();

    public string BuildFilterString()
    {
        return string.Join(",", _filters.Select(f => $"{f.Name}={f.Value}"));
    }
}

public class VideoFilterChain : FilterChain
{
    public void AddScaleFilter(int width, int height)
    {
        AddFilter("scale", $"{width}:{height}");
    }

    public void AddSharpnessFilter(int level)
    {
        if (level < 0 || level > 100)
            throw new ArgumentOutOfRangeException(nameof(level), "Sharpness level must be between 0 and 100");

        AddFilter("unsharp", $"{level}:5:1:5:1:0");
    }

    public void AddDenoiseFilter(int level)
    {
        if (level < 0 || level > 100)
            throw new ArgumentOutOfRangeException(nameof(level), "Denoise level must be between 0 and 100");

        AddFilter("nlmeans", $"10:{level}:7:7");
    }

    public void AddBrightnessFilter(int level)
    {
        if (level < -100 || level > 100)
            throw new ArgumentOutOfRangeException(nameof(level), "Brightness level must be between -100 and 100");

        AddFilter("eq", $"brightness={level / 100.0}");
    }

    public void AddContrastFilter(int level)
    {
        if (level < -100 || level > 100)
            throw new ArgumentOutOfRangeException(nameof(level), "Contrast level must be between -100 and 100");

        AddFilter("eq", $"contrast={1 + level / 100.0}");
    }

    public void AddStabilizationFilter()
    {
        AddFilter("deshake", "1");
    }
}

public class AudioFilterChain : FilterChain
{
    public void AddVolumeFilter(double level)
    {
        if (level < 0 || level > 5)
            throw new ArgumentOutOfRangeException(nameof(level), "Volume level must be between 0 and 5");

        AddFilter("volume", level.ToString("F2"));
    }

    public void AddNoiseReductionFilter(int level)
    {
        if (level < 0 || level > 100)
            throw new ArgumentOutOfRangeException(nameof(level), "Noise reduction level must be between 0 and 100");

        AddFilter("arnndn", level.ToString());
    }

    public void AddEqualizerFilter(int frequency, double gain)
    {
        if (frequency < 20 || frequency > 20000)
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be between 20Hz and 20kHz");
        if (gain < -20 || gain > 20)
            throw new ArgumentOutOfRangeException(nameof(gain), "Gain must be between -20dB and +20dB");

        AddFilter("equalizer", $"f={frequency}:t=h:w=100:g={gain}");
    }

    public void AddCompressorFilter(double threshold, double ratio)
    {
        if (threshold < -60 || threshold > 0)
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between -60dB and 0dB");
        if (ratio < 1 || ratio > 20)
            throw new ArgumentOutOfRangeException(nameof(ratio), "Ratio must be between 1 and 20");

        AddFilter("acompressor", $"threshold={threshold}:ratio={ratio}:attack=20:release=250");
    }

    public void AddHighPassFilter(int frequency)
    {
        if (frequency < 20 || frequency > 20000)
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be between 20Hz and 20kHz");

        AddFilter("highpass", $"f={frequency}");
    }

    public void AddLowPassFilter(int frequency)
    {
        if (frequency < 20 || frequency > 20000)
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be between 20Hz and 20kHz");

        AddFilter("lowpass", $"f={frequency}");
    }
}

public class Filter
{
    public string Name { get; }
    public string Value { get; }

    public Filter(string name, string value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}