using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace TypeLes
{
    public class Oefening
    {
        public string Zin
        {
            set => Zinnen = value
                .Replace("\r", string.Empty)
                .Replace('\n', ' ')
                .Replace("  ", " ")
                .Trim('\n', ' ', '\t');
        }

        public string Zinnen
        {
            get => _zinnen;
            set => _zinnen = value
                .Replace("\r",string.Empty)
                .Trim('\n', ' ', '\t');
        }

        public int LesNr { get; set; }
        public int DagNr { get; set; }
        public string DagNaam => ((DayOfWeek) (((int) DayOfWeek.Thursday + DagNr) % 7)).ToString();
        public int OefeningNr { get; set; }

        private string _zinnen;
    }

    class GemaakteOefening
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string Invoer { get; set; }
        public Oefening Oefening { get; set; }
        public bool BoekGebruikt { get; set; }
        public int AantalBackspaces { get; set; }
    }

    public class Program
    {
        private static readonly string[] Leerlingen = {"Sebbe", "Sophie", "Papa", "Mama"};

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var persoon = KiesPersoon();
            if (persoon is null)
                return;

            while (true)
            {
                var oefening = KiesOefening(persoon);
                if (oefening is null)
                    break;

                while (true)
                {
                    var gemaakt = DoeOefening(oefening, oefening.LesNr > 2);
                    if (gemaakt is null)
                        break;

                    SlaOp(gemaakt, persoon);
                    Feedback(gemaakt, persoon);

                    if (!VraagJaNee("Oefening herhalen?"))
                        break;
                }
            }
        }

        private static void SlaOp(GemaakteOefening gemaakt, string persoon)
        {
            var (voorbeeld, feedback, fouteWoorden) = OefeningRenderer.FinalFeedback(gemaakt.Oefening.Zinnen, gemaakt.Invoer);
            var hisWords = OefeningRenderer.Words(gemaakt.Oefening.Zinnen);
            var wordCount = hisWords.Length;
            var wpm = wordCount / gemaakt.Duration.TotalMinutes;

            var fn =
                $"{persoon}_{gemaakt.StartTime:yyyyMMdd_HHmmss}_{gemaakt.Oefening.LesNr}_{gemaakt.Oefening.DagNr}_{gemaakt.Oefening.OefeningNr}.txt";

            var tekst = $@"Leerling: {persoon}
Datum: {gemaakt.StartTime.LocalDateTime:D}
Tijd: {gemaakt.StartTime.LocalDateTime:t}
Oefening:
* Les {gemaakt.Oefening.LesNr}
* Dag {gemaakt.Oefening.DagNr} ({gemaakt.Oefening.DagNaam})
* Oefening {gemaakt.Oefening.DagNr}

Opdracht:
{gemaakt.Oefening.Zinnen}

Getypt:
{gemaakt.Invoer}

Overgetypt van: {(gemaakt.BoekGebruikt?"Boek":"Scherm")}

Backspace gebruikt: {gemaakt.AantalBackspaces} keer

Foute woorden: {fouteWoorden}
Woorden per minuut: {wpm}
";
            Console.Clear();
            Console.WriteLine(tekst);
            var fp = Path.Combine(DocumentsFolder(), fn);
            File.WriteAllText(fp, tekst);
            Console.ReadLine();
        }

        private static string DocumentsFolder()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TypeLes");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        private static Oefening KiesOefening(string persoon)
        {
            var list = Data.Oefeningen
                .OrderBy(x => x.LesNr)
                .ThenBy(x => x.DagNr)
                .ThenBy(x => x.OefeningNr)
                .ToList();

            var list2 = AddDoneCounts(list, persoon).ToList();

            var doneCount = list2.TakeWhile(x => x.DoneCount != 0).Count();
            var options = persoon == "Papa"?list2:list2.Take(doneCount + 1).ToList();
            var choice = VraagKeuze(options.Select(o =>
                    $"Les {o.Oefening.LesNr}; Dag {o.Oefening.DagNr} ({o.Oefening.DagNaam}); Oefening {o.Oefening.OefeningNr} - {o.DoneCount} keer gemaakt")
                .ToArray());
            if (choice is null)
                return null;
            return options[choice.Value].Oefening;
        }

        private static IEnumerable<(Oefening Oefening, int DoneCount)> AddDoneCounts(List<Oefening> list,
            string persoon)
        {
            var dir = DocumentsFolder();
            var files = Directory.GetFiles(dir, $"{persoon}_????????_??????_*.txt");
            var regex = new Regex(@"_(\d+)_(\d+)_(\d+)\.txt$", RegexOptions.Compiled);
            var done = files
                .Select(fn => regex.Match(fn))
                .Where(m => m.Success)
                .Select(m => new
                {
                    Les = int.Parse(m.Groups[1].Value),
                    Dag = int.Parse(m.Groups[2].Value),
                    Oefening = int.Parse(m.Groups[3].Value),
                }).ToList();
            return list.Select(o => (o, done.Count(d => d.Les==o.LesNr && d.Dag == o.DagNr && d.Oefening == o.OefeningNr)));
        }

        private static GemaakteOefening DoeOefening(Oefening oefening, bool gebruikBoek)
        {
            Console.Beep();
            var input = string.Empty;
            Console.Clear();
            DateTimeOffset startTime = default;
            var opdracht = oefening.Zinnen;
            int consoleWidth = Console.WindowWidth;
            int windowHeight = Console.WindowHeight;
            var backspaceCount = 0;
            bool mustRender = true;
            while (true)
            {
                if (consoleWidth != Console.WindowWidth || windowHeight != Console.WindowHeight)
                {
                    consoleWidth = Console.WindowWidth;
                    windowHeight = Console.WindowHeight;
                    Console.Clear();
                    mustRender = true;
                }

                if (mustRender)
                {
                    var (voorbeeld, feedback, klaar) = OefeningRenderer.LiveFeedback(opdracht, input, gebruikBoek);

                    if (klaar)
                        break;

                    OefeningRenderer.Render(voorbeeld, feedback, consoleWidth, windowHeight).ToScreen();
                    mustRender = false;
                    if (feedback.EndsWith("+"))
                        Console.Beep();
                }

                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(10);

                }
                else
                {
                    var key = Console.ReadKey(true);
                    mustRender = true;
                    if (key.Key == ConsoleKey.Escape)
                    {
                        if (VraagEchtAfsluiten())
                            return null;
                        Console.Clear();
                        continue;
                    }

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length > 0)
                        {
                            input = input.Substring(0, input.Length - 1);
                            backspaceCount++;
                        }
                    }
                    else if (key.Key == ConsoleKey.Spacebar)
                    {
                        if (input.Length != 0)
                            input += " ";
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (input.Length != 0)
                            input += "\n";
                    }
                    else
                    {
                        if (input.Length == 0)
                            startTime = DateTimeOffset.UtcNow;
                        input += key.KeyChar;
                    }
                }
            }

            var endTime = DateTimeOffset.UtcNow;
            return new GemaakteOefening()
            {
                StartTime = startTime,
                EndTime = endTime,
                Invoer = input,
                Oefening = oefening,
                BoekGebruikt = gebruikBoek,
                AantalBackspaces = backspaceCount
            };
        }

        private static string KiesPersoon()
        {
            var keus = VraagKeuze(Leerlingen);
            if (keus is null)
                return null;
            return Leerlingen[keus.Value];
        }

        private static int? VraagKeuze(string[] opties)
        {
            while (true)
            {
                Console.Clear();
                var n = 0;
                Console.WriteLine(" 0 : Afsluiten");
                foreach (var o in opties)
                {
                    n++;
                    Console.WriteLine($"{n,2} : {o}");
                }

                Console.Write("Maak een keuze: ");
                if (opties.Length < 10)
                {
                    while (true)
                    {
                        var k = Console.ReadKey();
                        if (int.TryParse(k.KeyChar.ToString(), out var choice))
                        {
                            if (choice == 0)
                                return null;
                            if (choice > 0 && choice <= opties.Length) return choice - 1;
                        }
                    }
                }

                var k2 = Console.ReadLine();
                if (int.TryParse(k2, out var choice2))
                {
                    if (choice2 == 0)
                        return null;
                    if (choice2 > 0 && choice2 <= opties.Length) return choice2 - 1;
                }
            }
        }

        private static void Feedback(GemaakteOefening gemaakt, string persoon)
        {
            Console.Clear();
            var (voorbeeld, feedback, fouteWoorden) = OefeningRenderer.FinalFeedback(gemaakt.Oefening.Zinnen, gemaakt.Invoer);
            OefeningRenderer.Render(voorbeeld, feedback, Console.WindowWidth, Console.WindowHeight).ToScreen();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            var hisWords = OefeningRenderer.Words(gemaakt.Oefening.Zinnen);
            var myWords = OefeningRenderer.Words(gemaakt.Oefening.Zinnen);
            var wordCount = hisWords.Length;
            var wpm = wordCount / gemaakt.Duration.TotalMinutes;
            Console.WriteLine($"{(int) gemaakt.Duration.TotalSeconds} seconden");
            Console.WriteLine($"{wpm} woorden per minuut");
        }

        private static bool VraagEchtAfsluiten() => VraagJaNee("Weet je zeker dat je wilt afsluiten?");

        private static bool VraagJaNee(string vraag)
        {
            Console.Clear();
            Console.WriteLine(vraag);
            var jnkey = Console.ReadKey();
            if (jnkey.Key == ConsoleKey.J || jnkey.Key == ConsoleKey.Y)
                return true;
            return false;
        }
    }
}
