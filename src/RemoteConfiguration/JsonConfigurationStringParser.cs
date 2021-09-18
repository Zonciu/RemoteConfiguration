// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// From https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Configuration.Json/src/JsonConfigurationFileParser.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RemoteConfiguration
{
    public sealed class JsonConfigurationStringParser
    {
        private JsonConfigurationStringParser()
        { }

        private readonly SortedDictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _paths = new();

        public static IDictionary<string, string?> Parse(string input)
            => new JsonConfigurationStringParser().ParseString(input);

        private IDictionary<string, string?> ParseString(string input)
        {
            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            using (var doc = JsonDocument.Parse(input, jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
                }

                VisitElement(doc.RootElement);
            }

            return _data;
        }

        private void VisitElement(JsonElement element)
        {
            var isEmpty = true;

            foreach (var property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            if (isEmpty && _paths.Count > 0)
            {
                _data[_paths.Peek()] = null;
            }
        }

        private void VisitValue(JsonElement value)
        {
            Debug.Assert(_paths.Count > 0);

            switch (value.ValueKind)
            {
            case JsonValueKind.Object:
                VisitElement(value);
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var arrayElement in value.EnumerateArray())
                {
                    EnterContext(index.ToString());
                    VisitValue(arrayElement);
                    ExitContext();
                    index++;
                }

                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                string key = _paths.Peek();
                if (_data.ContainsKey(key))
                {
                    throw new FormatException($"A duplicate key '{key}' was found.");
                }

                _data[key] = value.ToString();
                break;

            default:
                throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found.");
            }
        }

        private void EnterContext(string context) =>
            _paths.Push(_paths.Count > 0 ? _paths.Peek() + ConfigurationPath.KeyDelimiter + context : context);

        private void ExitContext() => _paths.Pop();
    }
}