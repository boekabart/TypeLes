using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeLes
{
    public static class OefeningRenderer
    {
        public static string[] Words(string invoer) =>
            invoer.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        public static (string Voorbeeld, string Feedback, bool Klaar) LiveFeedback(string voorbeeld, string feedback)
        {
            feedback = feedback.TrimStart();
            var words = Words(voorbeeld);
            var typedWords = Words(feedback);
            var completedWords = Math.Max(0,typedWords.Length - 1) + (feedback.EndsWith(' ') ? 1 : 0);
            var klaar = completedWords == words.Length || typedWords.Length == words.Length && typedWords.Last().Length == words.Last().Length;

            var vb = string.Join(' ', words.Select((w, i) => !klaar && i == completedWords ? $"*{w}*" : w));
            var fb = string.Join(' ', typedWords.Select((w, i) => StarsFor(w, words[i])));
            if (feedback.EndsWith(' '))
                fb += " ";
            else
                fb = fb.Trim();
            return (vb, fb, klaar);
        }

        public static (string Voorbeeld, string Feedback, int FouteWoorden) FinalFeedback(string voorbeeld, string feedback)
        {
            var words = Words(voorbeeld);
            var typedWords = Words(feedback);

            var badWords = words.Where((w, i) => !typedWords[i].Equals(w)).ToArray();

            return (voorbeeld, feedback, badWords.Length);
        }

        internal static string StarsFor(string invoer, string example)
        {
            var starCount = Math.Min(invoer.Length, example.Length);
            var spaceCount = example.Length - starCount;
            return new string('*', starCount) + new string(' ', spaceCount);
        }

        public static void ToScreen(this IEnumerable<Action> actions)
        {
            foreach (var a in actions) a();
        }

        public static IEnumerable<Action> Render(string voorbeeld, string feedback, int consoleWidth)
        {
            var words = Words(voorbeeld);
            var typedWords = Words(feedback.TrimStart() + "·");
            var wordNo = 0;
            var bold = false;
            var inputRow = 0;
            var inputPos = 0;
            for (var row = 0; ; row++)
            {
                yield return () => Console.SetCursorPosition(0, row * 3);
                int pos = 0;
                var startWordNo = wordNo;
                string todo = String.Empty;

                if (bold)
                    yield return () => Console.ForegroundColor = ConsoleColor.White;

                for (; wordNo < words.Length; wordNo++)
                {
                    var currentWord = words[wordNo].Replace("*", "");
                    if (pos + 1 +todo.Length + currentWord.Length > consoleWidth)
                        break;
                    if (words[wordNo].StartsWith('*'))
                    {
                        if (todo.Length > 0)
                        {
                            yield return () => Console.Write(todo);
                            pos += todo.Length;
                            todo = String.Empty;
                        }

                        if (bold)
                            yield return () => Console.ForegroundColor = ConsoleColor.Gray;
                        else
                            yield return () => Console.ForegroundColor = ConsoleColor.White;
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
                            yield return () => Console.Write(todo);
                            pos += todo.Length;
                            todo = String.Empty;
                        }

                        if (bold)
                            yield return () => Console.ForegroundColor = ConsoleColor.Gray;
                        else
                            yield return () => Console.ForegroundColor = ConsoleColor.White;

                        bold = !bold;
                    }
                }

                if (todo.Length > 0)
                {
                    yield return () => Console.Write(todo);
                    pos += todo.Length;
                    todo = String.Empty;
                }

                if (bold)
                    yield return () => Console.ForegroundColor = ConsoleColor.Gray;

                pos = 0;

                for (var word2 = startWordNo; word2 < wordNo && word2 < typedWords.Length; word2++)
                {
                    yield return () => Console.SetCursorPosition(pos, row * 3 + 1);
                    var starredWord = typedWords[word2];
                    yield return () => Console.Write(starredWord.Replace('·', ' '));
                    if (starredWord.EndsWith('·'))
                    {
                        inputRow = row;
                        inputPos = pos + starredWord.Length - 1;
                    }

                    pos += 1 + words[word2].Length;
                }

                if (wordNo >= words.Length)
                {
                    yield return () => Console.SetCursorPosition(inputPos, inputRow * 3 + 1);
                    break;
                }
            }
        }
    }
}