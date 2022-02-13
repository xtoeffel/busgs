/* **********************************************************************
 Copyright 2022 Christof Dittmar
 
 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
********************************************************************** */

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using UsgsResponseData;
using System.Collections.Immutable;

namespace UsgsResponseCompile
{
    public class ServerResponseCompiler
    {
        private class ObjectConverter : JsonConverter<object>
        {
            private List<object> ReadList(ref Utf8JsonReader reader)
            {
                var list = new List<object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        return list;
                    else if (reader.TokenType == JsonTokenType.StartArray)
                        list.Add(ReadList(ref reader));
                    else
                        list.Add(GetValue(ref reader));
                }
                return list;
            }

            private object GetValue(ref Utf8JsonReader reader)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        return reader.GetString();
                    case JsonTokenType.Number:
                        if (reader.TryGetDouble(out var double_value)) return double_value;
                        if (reader.TryGetInt32(out var int32_value)) return int32_value;
                        throw new NotSupportedException($"Unable to detect number type");
                    case JsonTokenType.False:
                    case JsonTokenType.True:
                        return reader.GetBoolean();
                    case JsonTokenType.Null:
                        return null;
                    default:
                        throw new NotSupportedException($"Unable to convert type '{reader.TokenType.ToString()}'");
                }
            }

            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                    return ReadList(ref reader);
                else
                    return GetValue(ref reader);
            }

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
                => throw new NotImplementedException();

            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(object);
        }

        private static ImmutableArray<SpectraEntry> ToArraySpectraEntries(object listOrNull, string spectraName)
        {
            if (listOrNull == null)
                return ImmutableArray.Create<SpectraEntry>();
            else if (listOrNull.GetType() == typeof(List<object>))
            {
                List<object> list = (List<object>)listOrNull;
                return list
                    .Select(e => (List<object>)e)
                    .Select(e => new SpectraEntry((double)e[0], (double)e[1])).ToImmutableArray();
            }
            else
                throw new NotSupportedException(
                    $"Unable to convert spectra {spectraName}, data is not list of object"
                );
        }

        // TODO: fix non-existing data, 'response', 'data', 'metaData' might be null
        // TODO: data might not have been replied correctly, maybe with server-note explaining why
        //          convert in a try-catch and filter all the replies that could not be converted 
        //          put in a special section of ServerResponseData
        //          include that into result file - sheet 'Fails'
        //          check if port exhaustion might be the issue
        public ServerResponseData Compile(string serverResponse)
        {
            var serializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new ObjectConverter() }
            };

            using (JsonDocument document = JsonDocument.Parse(serverResponse))
            {
                var rootJE = document.RootElement;

                if (!rootJE.TryGetProperty("request", out var requestJE))
                    throw new ArgumentException("missing element 'request'");
                if (!rootJE.TryGetProperty("response", out var responseJE))
                    throw new ArgumentException("missing element 'response'");
                if (!responseJE.TryGetProperty("metadata", out var metadataJE))
                    throw new ArgumentException("missing element 'response'->'metadata'");
                if (!responseJE.TryGetProperty("data", out var dataJE))
                    throw new ArgumentException("missing element 'response'->'data'");

                var requestData =
                    JsonSerializer.Deserialize<RequestData>(requestJE.ToString(), serializerOptions);
                var metaData =
                    JsonSerializer.Deserialize<ImmutableDictionary<string, object>>(
                        metadataJE.ToString(), serializerOptions
                    );

                var data =
                    JsonSerializer.Deserialize<ImmutableDictionary<string, object>>(
                        dataJE.ToString(), serializerOptions
                    );

                Dictionary<string, object> spectra = data
                    .Where(kvp => kvp.Key.ToLower().EndsWith("spectrum"))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var spectraTyped = spectra.ToDictionary(
                    e => e.Key,
                    e => ToArraySpectraEntries(e.Value, e.Key)
                );

                var dataOrderedKeys =
                    ImmutableArray.Create<string>(
                        dataJE.EnumerateObject()
                            .Select(jsonProp => jsonProp.Name)
                            .Where(name => !name.ToLower().EndsWith("spectrum"))
                            .ToArray()
                    );
                var spectraOrderedKeys =
                    ImmutableArray.Create<string>(
                        dataJE.EnumerateObject()
                            .Select(jsonProp => jsonProp.Name)
                            .Where(name => name.ToLower().EndsWith("spectrum"))
                            .ToArray()
                    );
                var metadataOrderedKeys =
                    ImmutableArray.Create<string>(
                        metadataJE.EnumerateObject()
                            .Select(jsonProp => jsonProp.Name)
                            .ToArray()
                    );

                return new ServerResponseData()
                {
                    Request = requestData,
                    SeismicDesignData = new DesignData()
                    {
                        OrderedKeysBaseValues = dataOrderedKeys,
                        OrderedKeysSpectra = spectraOrderedKeys,
                        OrderedKeysMetaData = metadataOrderedKeys,
                        BaseValues = data.Where(kvp => dataOrderedKeys.Contains(kvp.Key)).ToImmutableDictionary(),
                        Spectra = spectraTyped.ToImmutableDictionary(),
                        MetaData = metaData
                    }
                };
            }
        }

        // TODO: add list of failed response mesages, default = empty list
        public List<ServerResponseData> Compile(IList<string> serverResponse)
        {
            return serverResponse.Select(res => this.Compile(res)).ToList();
        }
    }
}