using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace S100Framework.WPF
{
    public class JsonUnflattener
    {
        public static JsonNode Unflatten(string jsonString) {
            // 1. Parse the flat string into a dictionary
            var flatDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
            var root = new JsonObject();

            if (flatDict is null) return root;

            foreach (var kvp in flatDict) {
                // Split by dot, but ignore dots inside brackets if necessary 
                // (Simple split works for your example)
                string[] parts = kvp.Key.Split('.');
                ProcessPath(root, parts, kvp.Value);
            }

            return root;
        }

        public static IEnumerable<(string Path, object Value)> GetAllProperties(JsonNode node, string path = "") {
            if (node is JsonObject obj) {
                foreach (var prop in obj) {
                    string subPath = string.IsNullOrEmpty(path) ? prop.Key : $"{path}.{prop.Key}";
                    if (prop.Value is null) continue;
                    foreach (var child in GetAllProperties(prop.Value, subPath))
                        yield return child;
                }
            }
            else if (node is JsonArray array) {
                for (int i = 0; i < array.Count; i++) {
                    foreach (var child in GetAllProperties(array[i]!, $"{path}[{i}]"))
                        yield return child;
                }
            }
            else {
                // Try to get the underlying value (bool, double, string, etc.)
                object val = node?.AsValue().ToString(); // Simplified for example
                yield return (path, val);
            }
        }


        private static void ProcessPath(JsonObject currentParent, string[] parts, JsonElement value) {
            JsonNode currentNode = currentParent;

            for (int i = 0; i < parts.Length; i++) {
                string part = parts[i];
                bool isLast = (i == parts.Length - 1);

                // Check if the part indicates an array, e.g., "zoneOfConfidence[0]"
                var arrayMatch = Regex.Match(part, @"^(.+)\[(\d+)\]$");

                if (arrayMatch.Success) {
                    string arrayName = arrayMatch.Groups[1].Value;
                    int index = int.Parse(arrayMatch.Groups[2].Value);

                    // Ensure the array exists
                    if (!currentParent.ContainsKey(arrayName) || currentParent[arrayName] == null) {
                        currentParent[arrayName] = new JsonArray();
                    }

                    JsonArray array = currentParent[arrayName].AsArray();

                    // Expand array with nulls if index is higher than current count
                    while (array.Count <= index) { array.Add(null); }

                    if (isLast) {
                        array[index] = JsonValue.Create(value);
                    }
                    else {
                        // If not last, we need an object at this index to continue
                        if (array[index] == null) { array[index] = new JsonObject(); }
                        currentParent = array[index].AsObject();
                    }
                }
                else {
                    // It's a regular property
                    if (isLast) {
                        currentParent[part] = JsonValue.Create(value);
                    }
                    else {
                        if (!currentParent.ContainsKey(part) || currentParent[part] == null) {
                            currentParent[part] = new JsonObject();
                        }
                        currentParent = currentParent[part].AsObject();
                    }
                }
            }
        }
    }
}
