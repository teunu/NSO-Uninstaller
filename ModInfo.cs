using Pastel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NSO_Uninstaller;

internal class ModInfo
{
	public string short_code => properties["short_code"];
	public string short_name => properties["short_name"];
	public string full_name => properties["full_name"];
	public string version => properties["version"];

	public readonly Dictionary<string, string> properties = new()
	{
		{ "short_code", "???" },
		{ "short_name", "???" },
		{ "full_name", "Unknown" },
		{ "version", "v?.?.?" },
	};
	public readonly List<string> file_list = new();

	public static ModInfo Parse(string filePath)
	{
		// The script that generates the installation zip file also creates an inventory of files.
		// So to uninstall we simply delete all the files mentioned there.
		// Note: If an update to NSO stops including a file it would be left behind by the uninstaller, so the updater should remove it at update time.
		// Note: This file is also on the list to be deleted, so we have to read it all and close it before starting.
		var lines = File.ReadAllLines(filePath);

		var modInfo = new ModInfo();

		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
			{
				continue;
			}

			const string propertyHeader = "!property ";
			if (line.StartsWith(propertyHeader))
			{
				var key = line.Substring(propertyHeader.Length).Split(' ').First();
				var value = line.Substring(propertyHeader.Length + key.Length + 1 /* separator after key */);

				modInfo.properties[key] = value;
				continue;
			}

			modInfo.file_list.Add(line);
		}

		return modInfo;
	}
}
