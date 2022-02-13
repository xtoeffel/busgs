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

using System.IO;

namespace Busgs.Test.Base
{
    public class BaseTest
    {
        public string GetDataDir()
        {
            string currentDir = Directory.GetCurrentDirectory();
            // execution is in bin/Debug/net#.#
            return Path.Combine(currentDir, "..", "..", "..", "Data");
        }

        public string ResolveDataFileName(string fileName)
        {
            return Path.Combine(GetDataDir(), fileName);
        }

        public string GetTestOutDir()
        {
            string currentDir = Directory.GetCurrentDirectory();
            // execution is in bin/Debug/net#.#
            return Path.Combine(currentDir, "..", "..", "..", "TestOut");
        }

        public string ResolveTestOutFileName(string fileName)
        {
            return Path.Combine(GetTestOutDir(), fileName);
        }
    }
}