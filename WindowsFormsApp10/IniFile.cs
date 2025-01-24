using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class IniFile
{
    private readonly string _filePath;
    private readonly Dictionary<string, Dictionary<string, string>> _sections;

    public IniFile(string filePath)
    {
        _filePath = filePath;
        _sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        Load();
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
            return;

        _sections.Clear();
        string currentSection = null;

        foreach (var line in File.ReadAllLines(_filePath))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                continue;

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else if (currentSection != null)
            {
                var keyValue = trimmedLine.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    _sections[currentSection][keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
        }
    }

    public void Save()
    {
        using (var writer = new StreamWriter(_filePath))
        {
            foreach (var section in _sections)
            {
                writer.WriteLine($"[{section.Key}]");
                foreach (var keyValue in section.Value)
                {
                    writer.WriteLine($"{keyValue.Key}={keyValue.Value}");
                }
                writer.WriteLine();
            }
        }
    }

    public string GetValue(string section, string key, string defaultValue = null)
    {
        if (_sections.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    public void SetValue(string section, string key, string value)
    {
        if (!_sections.ContainsKey(section))
        {
            _sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        _sections[section][key] = value;
    }

    public IEnumerable<string> GetKeys(string section)
    {
        return _sections.TryGetValue(section, out var sectionData) ? sectionData.Keys : Enumerable.Empty<string>();
    }

    public IEnumerable<string> GetSections()
    {
        return _sections.Keys;
    }
}