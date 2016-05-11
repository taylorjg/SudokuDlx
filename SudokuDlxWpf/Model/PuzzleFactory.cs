using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SudokuDlxWpf.Model
{
    public static class PuzzleFactory
    {
        public static IEnumerable<Puzzle> LoadSamplePuzzles()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(resourceName => resourceName.StartsWith("SudokuDlxWpf.SamplePuzzles."))
                .Select(CreatePuzzleFromJsonResource);
        }

        private static Puzzle CreatePuzzleFromJsonResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new ApplicationException($"Failed to load resource {resourceName}");
                return CreatePuzzleFromStream(stream);
            }
        }

        private static Puzzle CreatePuzzleFromStream(Stream stream)
        {
            var jsonSerializer = new JsonSerializer();
            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var puzzleData = jsonSerializer.Deserialize<dynamic>(jsonTextReader);
                var rowStrings = (JArray)puzzleData["rowStrings"];
                var initialValues = RowStringsToInitialValues(rowStrings);
                var title = (string) puzzleData["title"];
                return new Puzzle(initialValues, title);
            }
        }

        private static IImmutableList<InitialValue> RowStringsToInitialValues(JArray rowStrings)
        {
            return rowStrings.SelectMany(RowStringToInitialValues).ToImmutableList();
        }

        private static IEnumerable<InitialValue> RowStringToInitialValues(JToken rowString, int row)
        {
            var indices = Enumerable.Range(0, int.MaxValue);
            return ((string) rowString)
                .Zip(indices, Tuple.Create)
                .Where(t => char.IsDigit(t.Item1))
                .Select(t => new InitialValue(new Coords(row, t.Item2), t.Item1 - '0'));
        }
    }
}
