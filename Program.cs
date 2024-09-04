using NSO_Uninstaller;
using Pastel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

const bool FORCE_VALIDATION = false;

const string ARG_WAIT_FOR_DEBUGGER = "--wait-for-debugger";
const string ARG_NO_HEADER = "--no-header";
const string ARG_DRY_RUN = "--dry-run";
const string ARG_SKIP_CONFIRM = "--confirm";
const string ARG_ONLY_STARTUP = "--only-startup";
const string ARG_SKIP_EXIT_KEYPRESS = "--skip-exit-keypress";

if (args.Contains(ARG_WAIT_FOR_DEBUGGER))
{
	Debugger.Launch();
}

string selfDirectory = AppContext.BaseDirectory;
string root = Path.GetFullPath(Path.Combine(selfDirectory, "../.."));

if (! args.Contains(ARG_NO_HEADER))
{
	var file = Process.GetCurrentProcess().MainModule.FileName;
	var version = FileVersionInfo.GetVersionInfo(file).ProductVersion;

	Console.WriteLine
	(
		"Mod Uninstaller"
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
	Console.WriteLine($"Arg: {ARG_SKIP_CONFIRM} -> Skipping prompts for confirmation.");

var stormworksExepath = Path.Combine(root, "stormworks.exe");
var stormworksExepath64 = Path.Combine(root, "stormworks64.exe");
if (!File.Exists(stormworksExepath) && !File.Exists(stormworksExepath64))
{
	Console.WriteLine($"Did not find '{stormworksExepath}' unable to uninstall.".Pastel(ConsoleColor.Red));
	Console.WriteLine("The uninstaller should be placed in YOUR_STORMWORKS_INSTALLATION_FOLDER/rom/MOD_NAME.".Pastel(ConsoleColor.Red));
	Finish();
	return 1;
}


var modInfoFile = Path.Combine(selfDirectory, "files.txt");
if (!File.Exists(modInfoFile))
{
	Console.WriteLine($"Did not find '{modInfoFile}' unable to uninstall.".Pastel(ConsoleColor.Red));
	Console.WriteLine("The uninstaller should be placed in YOUR_STORMWORKS_INSTALLATION_FOLDER/rom/MOD_NAME.".Pastel(ConsoleColor.Red));
	Finish();
	return 1;
}

var modInfo = ModInfo.Parse(modInfoFile);

Console.WriteLine($"About to uninstall {modInfo.full_name} from: {root}");

if (args.Contains(ARG_ONLY_STARTUP))
{
	// For debugging.
	Console.WriteLine($"Arg: {ARG_ONLY_STARTUP} -> done.");
	return 0;
}

if (!skipConfirmation && !Confirm("Continue with uninstall?"))
{
	Console.ForegroundColor = ConsoleColor.DarkYellow;
	Console.WriteLine($"{modInfo.short_name} was not uninstalled");
	Console.ResetColor();
	Finish();
	return 1;
}


Uninstall();
Finish();
return 0;



void Uninstall()
{
	foreach (var line in modInfo.file_list)
	{
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

	Console.WriteLine($"{modInfo.short_name} related files deleted.".Pastel(ConsoleColor.DarkGreen));

	Console.ResetColor();
	Console.WriteLine("Automatic part finished; Please check file integrity in steam");
	Console.ForegroundColor = ConsoleColor.DarkYellow;
	Console.WriteLine("How?");
	Console.WriteLine(" > Right click the game in Steam.");
	Console.WriteLine(" > Properties");
	Console.WriteLine(" > Game Files");
	Console.WriteLine(" > Verify Game Integrity");

	if (FORCE_VALIDATION)
	{
		// If we get too many bug reports from people that didn't verify the files
		// we should just delete stormworks.exe
		// that way the user HAS TO verify the game to get it to work again.
		// And we won't get bug reports for stuff left behind.

		try { File.Delete(Path.Combine(root, "stormworks.exe"));   } catch { }
		try { File.Delete(Path.Combine(root, "stormworks64.exe")); } catch { }
	}

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
	Console.WriteLine($"{modInfo.short_name} has been uninstalled successfully!");
	Console.WriteLine($"Don't forget to also remove the Uninstaller after closing it.");
	Console.WriteLine("Happy storming!");
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
