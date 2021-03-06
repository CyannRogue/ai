﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace AIMLbot.Spell
{
    /// <summary>
    /// Conversion from http://norvig.com/spell-correct.html by C.Small
    /// </summary>
    /// <remarks>
    /// http://www.anotherchris.net/csharp/how-to-write-a-spelling-corrector-in-csharp/
    /// </remarks>
    public class Spelling
    {
        private Dictionary<string, int> _dictionary = new Dictionary<string, int>();
        private static readonly Regex Words = new Regex(@"\b([a-zA-Z]+)\b", RegexOptions.Compiled);

        public void ReadTextFile(string fileName = "big.txt")
        {
            using (var file = new StreamReader(fileName))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    Parse(line);
                }
            }
        }

        public void SaveToBinaryFile(string fileName = "dictionary.dat")
        {
            var fullPath = $@"{Environment.CurrentDirectory}\{fileName}";
            SaveToBinaryFile(new FileInfo(fullPath));
        }

        /// <summary>
        ///     Saves the dictionary to a binary file to avoid processing the text each time the
        ///     spell checker is used.
        /// </summary>
        public void SaveToBinaryFile(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            using (var stream = fileInfo.Create())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, _dictionary);
            }
        }

        /// <summary>
        ///     Loads a dump of the dictionary into memory 
        /// </summary>
        public void LoadFromBinaryFile(string fileName = "dictionary.dat")
        {
            var fullPath = $@"{Environment.CurrentDirectory}\{fileName}";
            LoadFromBinaryFile(new FileInfo(fullPath));
        }

        /// <summary>
        ///     Loads a dump of the graphmaster into memory so avoiding processing the AIML files again
        /// </summary>
        /// <param name="fileInfo">The specific file to load.</param>
        public void LoadFromBinaryFile(FileInfo fileInfo)
        {
            if (fileInfo != null && fileInfo.Exists)
            {
                using (var stream = fileInfo.OpenRead())
                {
                    var formatter = new BinaryFormatter();
                    _dictionary = (Dictionary<string, int>)formatter.Deserialize(stream);
                }
            }
            else
            {
                throw new FileNotFoundException("Unable to find the spelling dictionary.");
            }
        }


        private void Parse(string line) { 
            var matches = Words.Matches(line);            
            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                foreach (Group group in groups) {  
                    string trimmedWord = group.Value.ToLower();
                    if (_dictionary.ContainsKey(trimmedWord))
                        _dictionary[trimmedWord]++;
                    else
                        _dictionary.Add(trimmedWord, 1);
                }
            }
        }

        public string Correct(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            word = word.ToLower();

            // known()
            if (_dictionary.ContainsKey(word))
                return word;

            List<string> list = Edits(word);
            Dictionary<string, int> candidates = new Dictionary<string, int>();

            foreach (string wordVariation in list)
            {
                if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                    candidates.Add(wordVariation, _dictionary[wordVariation]);
            }

            if (candidates.Count > 0)
                return candidates.OrderByDescending(x => x.Value).First().Key;

            // known_edits2()
            foreach (string item in list)
            {
                foreach (string wordVariation in Edits(item))
                {
                    if (_dictionary.ContainsKey(wordVariation) && !candidates.ContainsKey(wordVariation))
                        candidates.Add(wordVariation, _dictionary[wordVariation]);
                }
            }

            return (candidates.Count > 0) ? candidates.OrderByDescending(x => x.Value).First().Key : word;
        }

        private static List<string> Edits(string word)
        {
            var replaces = new List<string>();
            var inserts = new List<string>();

            // Splits
            var splits = word.Select((t, i) => new Tuple<string, string>(word.Substring(0, i), word.Substring(i))).ToList();

            // Deletes
            var deletes = (from t in splits let a = t.Item1 let b = t.Item2 where !string.IsNullOrEmpty(b) select a + b.Substring(1)).ToList();

            // Transposes
            var transposes = (from t in splits let a = t.Item1 let b = t.Item2 where b.Length > 1 select a + b[1] + b[0] + b.Substring(2)).ToList();

            // Replaces
            foreach (Tuple<string, string> t in splits)
            {
                string a = t.Item1;
                string b = t.Item2;
                if (!string.IsNullOrEmpty(b))
                {
                    for (var c = 'a'; c <= 'z'; c++)
                    {
                        replaces.Add(a + c + b.Substring(1));
                    }
                }
            }

            // Inserts
            foreach (Tuple<string, string> t in splits)
            {
                string a = t.Item1;
                string b = t.Item2;
                for (var c = 'a'; c <= 'z'; c++)
                {
                    inserts.Add(a + c + b);
                }
            }

            return deletes.Union(transposes).Union(replaces).Union(inserts).ToList();
        }
    }
}