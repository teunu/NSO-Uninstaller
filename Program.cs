using Pastel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;


const string ARG_NO_HEADER = "--no-header";
const string ARG_DRY_RUN = "--dry-run";
const string ARG_SKIP_CONFIRM = "--confirm";
const string ARG_ONLY_STARTUP = "--only-startup";
const string ARG_SKIP_EXIT_KEYPRESS = "--skip-exit-keypress";

string root = AppContext.BaseDirectory;

if (! args.Contains(ARG_NO_HEADER))
{
	var name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
	var file = Path.Combine(root, $"{name}.exe");
	var version = FileVersionInfo.GetVersionInfo(file).ProductVersion;

	Console.WriteLine
	(
		"NSO Uninstaller"
		+ "\nCopyright 2024 teunu, CodeLeopard"
		+ "\nThis program comes with ABSOLUTELY NO WARRANTY"
		//+ "\nLicense: LGPL-3.0-or-later See: https://www.gnu.org/licenses/" // todo: pick license and add to repo.
		+ "\nSource code: https://github.com/teunu/NSO-Uninstaller"
		+ $"\nVersion: {version}"
	);
}


bool dryRun = args.Contains(ARG_DRY_RUN);
bool skipConfirmation = args.Contains(ARG_SKIP_CONFIRM);
bool skipExitKeyPress = args.Contains(ARG_SKIP_EXIT_KEYPRESS);


if (dryRun)
	Console.WriteLine($"Arg: {ARG_DRY_RUN} -> Will only list files, not delete them.");

if (skipConfirmation)
	Console.WriteLine($"Arg: {ARG_SKIP_CONFIRM} -> Skipping prompts for confrmation.");

var stormworksExepath = Path.Combine(root, "stormworks.exe");
var stormworksExepath64 = Path.Combine(root, "stormworks64.exe");
if (!File.Exists(stormworksExepath) && !File.Exists(stormworksExepath64))
{
	Console.WriteLine($"Did not find '{stormworksExepath}' unable to uninstall NSO.".Pastel(ConsoleColor.Red));
	Console.WriteLine("The uninstaller should be placed in the stormworks installation folder, next to 'stormworks.exe'.".Pastel(ConsoleColor.Red));
	Finish();
	return 1;
}

Console.WriteLine($"About to uninstall NSO from: {root}");

if (args.Contains(ARG_ONLY_STARTUP))
{
	// For debugging.
	Console.WriteLine($"Arg: {ARG_ONLY_STARTUP} -> done.");
	return 0;
}

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
	// Note: This file is also on the list to be deleted, so we have to read it all and close it before starting.
	var lines = File.ReadAllLines(Path.Combine(root, "rom/nso_mod/files.txt"));
	foreach (var line in lines)
	{
		if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
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
		catch (DirectoryNotFoundException ex)
		{
			Debug.WriteLine(line);
			Debug.WriteLine(ex.ToString());
		}
		catch (FileNotFoundException ex)
		{
			Debug.WriteLine(line);
			Debug.WriteLine(ex.ToString());
		}
		catch (Exception ex)
		{
			Debug.WriteLine(line);
			Debug.WriteLine(ex.ToString());

			Console.WriteLine($"Failed to delete '{line}'.".Pastel(ConsoleColor.Yellow));
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
	if (skipExitKeyPress)
		return;

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
