using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TypeLes
{
    public class Oefening
    {
        private string _zin;
        public string Zin
        {
            get => _zin;
            set => _zin = value.Replace("\r\n", " ");
        }
        
        public int LesNr { get; set; }
        public int DagNr { get; set; }
        public string DagNaam => ((DayOfWeek) (((int) DayOfWeek.Thursday + DagNr) % 7)).ToString();
        public int OefeningNr { get; set; }
    }

    class GemaakteOefening
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string Invoer { get; set; }
        public Oefening Oefening { get; set; }
    }

    public class Program
    {
        public static Oefening[] Oefeningen = new[]
        {
            new Oefening
            {
                LesNr = 1,
                DagNr = 1,
                OefeningNr = 1,
                Zin = "jfjf ffjj jfjj jjff"
            },
            new Oefening
            {
                LesNr = 2, DagNr = 1, OefeningNr = 1,
                Zin =
                    @"rfrf frfr frrf rrff rfrf juju juuj juuj frju ujfr rfuj rfuj rfrj ujuf rfuj rfuj rfrj ujuf rfju frju
rfuj rfuj rfrj ujuf rfuj ffjr fjuj fjru rfuj fjru jjuu frrf juuj rfuj frfj jujr frju
ujfr rfuj rfuj fjrf jfrj ujfr rfju rfju rfuj juuj frrf ujju rfuj rfuj fjru jjuu frrf juuj
rfrj ujuf rfuj fjrf fjuf"
            }
        };

        private static readonly string[] Leerlingen = {"Sebbe", "Sophie", "Papa", "Mama"};

        static void Main(string[] args)
        {
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
                    var gemaakt = DoeOefening(oefening);
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
            var (voorbeeld, feedback, fouteWoorden) = OefeningRenderer.FinalFeedback(gemaakt.Oefening.Zin, gemaakt.Invoer);
            var hisWords = OefeningRenderer.Words(gemaakt.Oefening.Zin);
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
{gemaakt.Oefening.Zin}

Getypt:
{gemaakt.Invoer}

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
            var list = Oefeningen
                .OrderBy(x => x.LesNr)
                .ThenBy(x => x.DagNr)
                .ThenBy(x => x.OefeningNr)
                .ToList();

            var list2 = AddDoneCounts(list, persoon);

            var doneCount = list2.TakeWhile(x => x.DoneCount != 0).Count();
            var options = list2.Take(doneCount + 1).ToList();
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

        private static GemaakteOefening DoeOefening(Oefening oefening)
        {
            var input = string.Empty;
            Console.Clear();
            DateTimeOffset startTime = default;
            var opdracht = oefening.Zin;
            while (true)
            {
                var (voorbeeld, feedback, klaar) = OefeningRenderer.LiveFeedback(opdracht, input);

                if (klaar)
                    break;

                Console.Clear();
                OefeningRenderer.Render(voorbeeld, feedback, Console.WindowWidth).ToScreen();

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    if (VraagEchtAfsluiten())
                        return null;
                    continue;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                        input = input.Substring(0, input.Length - 1);
                }
                else if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Spacebar)
                {
                    if (input.Length != 0)
                        input += " ";
                }
                else
                {
                    if (input.Length == 0)
                        startTime = DateTimeOffset.UtcNow;
                    input += key.KeyChar;
                }
            }

            var endTime = DateTimeOffset.UtcNow;
            return new GemaakteOefening()
            {
                StartTime = startTime,
                EndTime = endTime,
                Invoer = input,
                Oefening = oefening
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
            var (voorbeeld, feedback, fouteWoorden) = OefeningRenderer.FinalFeedback(gemaakt.Oefening.Zin, gemaakt.Invoer);
            OefeningRenderer.Render(voorbeeld, feedback, Console.WindowWidth).ToScreen();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            var hisWords = OefeningRenderer.Words(gemaakt.Oefening.Zin);
            var myWords = OefeningRenderer.Words(gemaakt.Oefening.Zin);
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
