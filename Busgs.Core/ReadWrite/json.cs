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

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Parameters;
using System.IO;
using BaseIO;


namespace JsonIO
{
    public static class JsonReader
    {
        public static T Read<T>(string file)
        {
            Utils.TestFileFolder(file);

            string jsonString = File.ReadAllText(file);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }

    public static class JsonWriter
    {
        public static void Write<T>(string file, T data)
        {
            Utils.TestFileFolder(file);

            var options = new JsonSerializerOptions() { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options: options);
            File.WriteAllText(file, jsonString);
        }
    }
}