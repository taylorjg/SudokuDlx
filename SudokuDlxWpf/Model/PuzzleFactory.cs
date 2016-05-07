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
        public static Puzzle CreatePuzzleFromJsonResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new ApplicationException($"Failed to load resource {resourceName}");
                var jsonSerializer = new JsonSerializer();
                using (var streamReader = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var puzzleData = jsonSerializer.Deserialize<dynamic>(jsonTextReader);
                    var rowStrings = (JArray)puzzleData["rowStrings"];
                    var initialValues = RowStringsToInitialValues(rowStrings);
                    return new Puzzle(initialValues);
                }
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
