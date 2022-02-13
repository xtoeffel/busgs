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

using System.Collections.Generic;
using System.Collections;
using System.IO;
using Samples;
using SimpleTable;
using ExcelIO;

namespace WriteSamples
{
    public static class Excel
    {
        private static Dictionary<string, SimpleDataTable> GetSampleInput()
        {
            var sampleRequestParameters = SampleRequestParameters.GetSamples();
            var simpleTable = new TableBuilderRequestParameters().Create(sampleRequestParameters);
            return new Dictionary<string, SimpleDataTable>
            {
                {"Batch", simpleTable}
            };
        }

        public static void WriteSampleRequestParameters(string toExcelFile)
        {
            ExcelIO.ExcelWriter.Write(toExcelFile, GetSampleInput());
        }

        public static void WriteSampleRequestParameters(Stream stream)
        {
            ExcelIO.ExcelWriter.WriteToStream(stream, GetSampleInput());
        }
    }
}