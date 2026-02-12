using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InMemoryVesselDb
{
    class Program
    {
        static void Main()
        {
            // Load vessels.json
            string json = File.ReadAllText("vessels.json");

            
            if (json.TrimStart().StartsWith("var"))
            {
                int index = json.IndexOf("[");
                json = json.Substring(index);

                // Remove trailing semicolon if present
                int semicolonIndex = json.LastIndexOf(';');
                if (semicolonIndex != -1)
                {
                    json = json.Substring(0, semicolonIndex);
                }
            }

            var vessels = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);


            Console.WriteLine($"Total records loaded: {vessels.Count}");

            // Take dynamic query input
            Console.WriteLine("\nEnter your WHERE query:");
            string query = Console.ReadLine();

            var results = ApplyQuery(vessels, query);

            // Print results
            Console.WriteLine($"\nMatched Records: {results.Count}\n");

            foreach (var vessel in results)
            {
                Console.WriteLine(JsonSerializer.Serialize(vessel,
                    new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        static List<Dictionary<string, object>> ApplyQuery(
            List<Dictionary<string, object>> data,
            string query)
        {
            if (!query.Trim().StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Query must start with WHERE");

            // Remove WHERE
            query = query.Substring(5).Trim();

            // Split AND conditions
            var conditions = Regex.Split(query, @"\bAND\b", RegexOptions.IgnoreCase)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            var parsedConditions = conditions.Select(ParseCondition).ToList();

            return data.Where(record =>
            {
                foreach (var condition in parsedConditions)
                {
                    if (!record.ContainsKey(condition.Field))
                        return false;

                    var recordValue = ExtractValue(record[condition.Field]);

                    if (!Compare(recordValue, condition.Operator, condition.Value))
                        return false;
                }
                return true;
            }).ToList();
        }

        static (string Field, string Operator, object Value) ParseCondition(string condition)
        {
            var match = Regex.Match(condition, @"(\w+)\s*(=|<|>)\s*(.+)");

            if (!match.Success)
                throw new Exception("Invalid condition: " + condition);

            string field = match.Groups[1].Value;
            string op = match.Groups[2].Value;
            string rawValue = match.Groups[3].Value.Trim();

            object value;

            if (rawValue.StartsWith("'") && rawValue.EndsWith("'"))
            {
                value = rawValue.Substring(1, rawValue.Length - 2);
            }
            else if (double.TryParse(rawValue, out double number))
            {
                value = number;
            }
            else
            {
                value = rawValue;
            }

            return (field, op, value);
        }

        static bool Compare(object recordValue, string op, object queryValue)
        {
            if (recordValue == null)
                return false;

            // Numeric comparison
            if (recordValue is double || recordValue is int || recordValue is long)
            {
                double recordNum = Convert.ToDouble(recordValue);
                double queryNum = Convert.ToDouble(queryValue);

                switch (op)
                {
                    case "=": return recordNum == queryNum;
                    case "<": return recordNum < queryNum;
                    case ">": return recordNum > queryNum;
                }
            }

            // String comparison
            if (op == "=")
            {
                return recordValue.ToString().Trim()
                    .Equals(queryValue.ToString().Trim(),
                            StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        static object ExtractValue(object value)
        {
            if (value is JsonElement element)
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.Number:
                        return element.GetDouble();
                    case JsonValueKind.String:
                        return element.GetString();
                    case JsonValueKind.Null:
                        return null;
                }
            }

            return value;
        }
    }

}
