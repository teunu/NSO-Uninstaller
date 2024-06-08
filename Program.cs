﻿using Pastel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.System;


if (! args.Contains("--no-header"))
{
	var assembly = Assembly.GetExecutingAssembly();
	var version = "alpha.0.2"; //FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

	Console.WriteLine
	(
		"Copyright 2024 teunu, CodeLeopard"
		+ "\nThis program comes with ABSOLUTELY NO WARRANTY"
		//+ "\nLicense: LGPL-3.0-or-later See: https://www.gnu.org/licenses/" // todo: pick license and add to repo.
		+ "\nSource code: https://github.com/teunu/NSO-Uninstaller"
		+ $"\nVersion: {version}"
	);
}

bool dryRun = args.Contains("--dry-run");
bool skipConfirmation = args.Contains("--confirm");

string root = AppDomain.CurrentDomain.BaseDirectory;

//Temp override
root = "C:\\Users\\teunu\\Desktop\\Project Repositories\\NSO\\stormworks_nso";


Console.WriteLine($"About to uninstall NSO from: {root}");
if (!skipConfirmation && !Confirm("Continue with uninstall?"))
{
	Console.ForegroundColor = ConsoleColor.DarkYellow;
	Console.WriteLine("NSO was not uninstalled");
	Console.ResetColor();
	Finish();
	return 1;
}


Uninstall();
Finish();
return 0;


void Uninstall()
{
	// The script that generates the installation zip file also creates an inventory of files.
	// We simply delete all the files mentioned there.
	// Note: If an update to NSO stops including a file it would be left behind by the uninstaller, so the updater should remove it at update time.
	var lines = File.ReadAllLines(Path.Combine(root, "rom/nso_mod/files.txt"));
	foreach (var line in lines)
	{
		if (string.IsNullOrWhiteSpace(line))
		{
			continue;
		}

		var finalPath = Path.Combine(root, line);

		try
		{
			if (!dryRun)
			{
				File.Delete(finalPath);
			}
			Console.WriteLine(finalPath.Pastel(ConsoleColor.Gray));
		}
		catch (DirectoryNotFoundException)
		{
			// Ignore.
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to delete '{line}'.".Pastel(ConsoleColor.Yellow));
			Debug.WriteLine(ex.ToString());
		}
	}

	Console.WriteLine("NSO related files deleted.".Pastel(ConsoleColor.DarkGreen));

	Console.ResetColor();
	Console.WriteLine("Automatic part finished; Please check file integrity in steam");
	Console.ForegroundColor = ConsoleColor.DarkYellow;
	Console.WriteLine("How?");
	Console.WriteLine(" > Right click the game in Steam.");
	Console.WriteLine(" > Properties");
	Console.WriteLine(" > Game Files");
	Console.WriteLine(" > Verify Game Integrity");

	Console.ResetColor();

	int i = 0;
	while (!Confirm("Verified integrity?"))
	{
		if (i > 3)
		{
			Console.WriteLine("Trusting that the integrity has been verified!");
			break;
		}
		Console.WriteLine("Please do!");
		i++;
	}

	Console.ForegroundColor = ConsoleColor.Green;
	Console.WriteLine("NSO has been uninstalled successfully! Happy storming!");
}

void Finish()
{
	Console.WriteLine("Waiting for keypress before closing...");
	Console.ReadKey();
}

bool Confirm(string title)
{
	ConsoleKey response;
	do
	{
		Console.ForegroundColor = ConsoleColor.DarkYellow;

		Console.Write($"{title} [y/n] ");
		response = Console.ReadKey(false).Key;
		if (response != ConsoleKey.Enter)
		{
			Console.WriteLine();
		}
	} while (response != ConsoleKey.Y && response != ConsoleKey.N);

	Console.ResetColor();

	return (response == ConsoleKey.Y);
}
