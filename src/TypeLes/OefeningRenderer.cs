﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeLes
{
    public static class OefeningRenderer
    {
        public static string ReplaceAll(this string input, string find, string replace) => !input.Contains(find)
            ? input
            : input.Replace(find, replace).ReplaceAll(find, replace);

        public static string[] Words(string invoer) =>
            invoer
                .Split(new [] {'\n',' '}, StringSplitOptions.RemoveEmptyEntries);

        private const string NewLine = "\n";

        private const char EnterChar = '⏎';
        private const string Enter = @"⏎";
        private const string EnterSpace = @"⏎ ";
        private const string SpaceEnter = @" ⏎";
        private const string EnterStar = @"⏎*";
        private const string EnterStarSpace = @"⏎* ";

        public static (string Voorbeeld, string Feedback, bool Klaar) LiveFeedback(string voorbeeld,
            string actualinvoer, bool verbergVoorbeeldTotGetypt = false)
        {
            voorbeeld = voorbeeld.Replace(NewLine, EnterSpace);
            actualinvoer = actualinvoer.TrimStart();
            var invoer = actualinvoer
                    .Replace(NewLine, EnterSpace)
                    .ReplaceAll("  ", " ")
                    .Replace(" " + Enter, Enter);

            var words = Words(voorbeeld);
            var typedWords = Words(invoer);
            var completedWords = Math.Max(0, typedWords.Length - 1) +
                                 (invoer.EndsWith(' ') || invoer.EndsWith(NewLine) ? 1 : 0);
            var klaar = completedWords == words.Length || typedWords.Length == words.Length &&
                        typedWords.Last().Length == words.Last().Length;

            var vb = string.Join(' ', words.Select((w, i) => verbergVoorbeeldTotGetypt && i>= completedWords?Verborgen(w):w ).Select((w, i) => !klaar && i == completedWords ? $"*{w}*" : w));
            var fb = string.Join(' ', typedWords.Select((w, i) => StarsWithEnterFor(w, words[i], i < completedWords)))
                .Replace(EnterSpace, Enter);
            if (actualinvoer.EndsWith(' '))
                fb += " ";
            else if (!actualinvoer.EndsWith(NewLine))
                fb = fb.Trim();
            fb = fb.Replace(NewLine + " ", NewLine);

            vb = vb
                .Replace(EnterStarSpace, EnterStar + NewLine)
                .Replace(EnterSpace, Enter + NewLine);

            return (vb, fb, klaar);
        }

        private static string Verborgen(string s)
        {
            if (s.EndsWith(Enter))
                return new string('?', s.Length - 1) + Enter;
            return new string('?', s.Length);
        }

        public static (string Voorbeeld, string Feedback, int FouteWoorden) FinalFeedback(string opdracht, string invoer)
        {
            try
            {

                var words = Words(opdracht);
                var typedWords = Words(invoer);

                var badWords = words.Where((w, i) => !typedWords[i].Equals(w)).ToArray();

                return (opdracht, invoer, badWords.Length);
            }
            catch
            {
                return (opdracht, invoer, -1);
            }
        }

        internal static string StarsWithEnterFor(string invoer, string example, bool invoerCompleted)
        {
            var tepmVal = StarsFor(invoer.Replace(Enter, string.Empty), example.Replace(Enter, string.Empty));
            if (invoer.EndsWith(Enter))
                tepmVal += Enter;
            if (example.EndsWith(Enter))
            {
                if (invoerCompleted)
                    tepmVal += NewLine;
            }

            return tepmVal;
        }

        internal static string StarsFor(string invoer, string example)
        {
            var plusCount = 0;
            var starCount = Math.Min(invoer.Length, example.Length);
            var spaceCount = example.Length - starCount;
            if (invoer.Length > example.Length)
            {
                starCount--;
                plusCount++;
            }

            return new string('*', starCount) + new string('+', plusCount) + new string(' ', spaceCount);
        }

        public static void ToScreen(this IEnumerable<Action> actions)
        {
            foreach (var a in actions) a();
        }

        public static List<Action> Render(string voorbeeld, string feedback, int consoleWidth, int windowHeight, int emptyLines = 1)
        {
            string[] Wordz(string invoer) =>
                invoer
                    .Split(new []{'\n',' '}, StringSplitOptions.RemoveEmptyEntries);

            var words = Wordz(voorbeeld);
            var typedWords = Wordz(feedback.ReplaceAll(SpaceEnter,Enter).Replace(Enter,EnterSpace).TrimStart() + "·");
            var wordNo = 0;
            var bold = false;
            var inputRow = 0;
            var inputPos = 0;
            var render = new List<Action>();
            for (var rowNumber = 0; ; rowNumber++)
            {
                var linesPerLine = 2 + emptyLines;

                if (emptyLines != 0 && rowNumber * linesPerLine + 2 >= windowHeight)
                    return Render(voorbeeld, feedback, consoleWidth, windowHeight, emptyLines - 1);

                var top = rowNumber * linesPerLine;
                render.Add(() => Console.SetCursorPosition(0, top));
                int pos = 0;
                var startWordNo = wordNo;
                string todo = String.Empty;

                if (bold)
                    render.Add(() => Console.ForegroundColor = ConsoleColor.White);

                for (; wordNo < words.Length; wordNo++)
                {
                    var currentWord = words[wordNo].Replace("*", "");
                    // Don't allow words to touch the right edge, as there would be no space for the cursor left!
                    if (pos + 1 +todo.Length + currentWord.Length >= consoleWidth)
                        break;
                    if (words[wordNo].StartsWith('*'))
                    {
                        if (todo.Length > 0)
                        {
                            var toPrint = todo;
                            render.Add(() => Console.Write(toPrint));
                            pos += todo.Length;
                            todo = String.Empty;
                        }

                        if (bold)
                            render.Add(() => Console.ForegroundColor = ConsoleColor.Gray);
                        else
                            render.Add(() => Console.ForegroundColor = ConsoleColor.White);
                        bold = !bold;
                    }

                    if (wordNo == startWordNo)
                        todo = currentWord;
                    else
                        todo += " " + currentWord;

                    if (words[wordNo].EndsWith('*'))
                    {
                        if (todo.Length > 0)
                        {
                            var toPrint = todo;
                            render.Add(() => Console.Write(toPrint));
                            pos += todo.Length;
                            todo = String.Empty;
                        }

                        if (bold)
                            render.Add(() => Console.ForegroundColor = ConsoleColor.Gray);
                        else
                            render.Add(() => Console.ForegroundColor = ConsoleColor.White);

                        bold = !bold;
                    }

                    if (currentWord.EndsWith(Enter))
                    {
                        wordNo++;
                        break;
                    }
                }

                if (todo.Length > 0)
                {
                    var toPrint = todo;
                    render.Add(() => Console.Write(toPrint));
                    pos += todo.Length;
                    todo = String.Empty;
                }

                if (bold)
                    render.Add(() => Console.ForegroundColor = ConsoleColor.Gray);

                pos = 0;

                for (var word2 = startWordNo; word2 < wordNo && word2 < typedWords.Length; word2++)
                {
                    var thePos = pos;
                    var theRow = rowNumber * linesPerLine + 1;
                    render.Add(() => Console.SetCursorPosition(thePos, theRow));
                    var starredWord = typedWords[word2];
                    render.Add(() => Console.Write(starredWord.Replace('·', ' ')));
                    if (starredWord.EndsWith('·'))
                    {
                        inputRow = rowNumber;
                        inputPos = pos + starredWord.Length - 1;
                    }

                    pos += 1 + words[word2].Length;
                }

                if (wordNo >= words.Length)
                {
                    render.Add(() => Console.SetCursorPosition(inputPos, inputRow * linesPerLine + 1));
                    break;
                }
            }

            return render;
        }
    }
}