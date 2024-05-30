using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace NSO_Uninstaller
{
    internal class Program
    {
        const string tile_folder = "\\rom\\data\\tiles\\";
        const string trees = "*instances.xml";
        static string root;

        //const string test = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Stormworks";

        static void Main(string[] args)
        {
            Console.WriteLine("Uninstalling NSO!");
            root = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"Current Path: {root}");

            if (Confirm("Continue with uninstall?")) {
                Uninstall();
                Finish();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("NSO was not uninstalled");
            Console.ResetColor();
            Finish();
        }

        public static void Uninstall()
        {
            //string[] found_trees = Directory.GetDirectories(root);
            //Remove trees
            string[] found_trees = Directory.GetFiles(root + tile_folder, trees);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (string tree in found_trees)
            {
                Console.WriteLine($"... [{tree}]");
                File.Delete(tree);
            }
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("All trees were removed! Islands are now bald!");

            Console.ResetColor();
            //Console.WriteLine("End of automation");

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
                if (i > 3) {
                    Console.WriteLine("Trusting that the integrity has been verified!");
                    break;
                }
                Console.WriteLine("Please do!");
                i++;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("NSO has been uninstalled successfully! Happy storming!");
        }

        public static void Finish()
        {
            Console.WriteLine("Waiting for keypress before closing...");
            Console.ReadKey();
        }

        public static bool Confirm(string title)
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
    }


}
