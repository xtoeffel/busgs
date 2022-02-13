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

using Xunit;
using ExcelIO;
using System.Collections.Generic;
using Parameters;
using BaseIO;
using System.Linq;
using Busgs.Test.Base;
using Busgs.Test.Data;
using UsgsResponseData;
using UsgsResponseCompile;
using System.IO;
using SimpleTable;

namespace Busgs.Test.ReadWrite
{
    public class ReadWriteTest : BaseTest
    {
        [Fact]
        public void ReadExcel_Success()
        {
            // GIVEN
            string file = this.ResolveDataFileName("seismic_batch_sample.xlsx");

            // WHEN
            List<RequestParametersInput> inputParameters = ExcelReader.Read(file, "Batch", new TableHeader(1, 0));

            // THEN
            var expected_refDoc = new List<string> { "IBC-2012", "IBC-2015", "ASCE7-05", "ASCE7-10", "ASCE7-16" };
            var expected_lat = new List<double> { 34.1, 33.8, 34.2, 39.7, 33.9 };
            var expected_long = new List<double> { -117.2, -118.1, -118.4, -115.3, -118.9 };
            var expected_riskCat = new List<string> { "I", "II", "III", "IV", "I" };
            var expected_siteClass = new List<string> { "A", "B", "C", "D", "E" };
            var expected_title = expected_refDoc.Select(rd => "Example " + rd);
            Assert.Equal(5, inputParameters.Count);
            Assert.Equal(expected_refDoc, inputParameters.Select(rq => rq.RefDoc));
            Assert.Equal(expected_lat, inputParameters.Select(rq => rq.Latitude));
            Assert.Equal(expected_long, inputParameters.Select(rq => rq.Longitude));
            Assert.Equal(expected_riskCat, inputParameters.Select(rq => rq.RiskCategory));
            Assert.Equal(expected_siteClass, inputParameters.Select(rq => rq.SiteClass));
            Assert.Equal(expected_title, inputParameters.Select(rq => rq.Title));
        }

        [Fact]
        public void WriteExcel_Success()
        {
            // GIVEN 
            string file = this.ResolveTestOutFileName("test_out_server_response.xlsx");
            File.Delete(file);
            ServerResponseCompiler responseCompiler = new ServerResponseCompiler();
            var dataEntry1 =
                responseCompiler.Compile(ServerResponseTestData.SingleResponseJsonString_IBC2012);
            List<ServerResponseData> data = Enumerable.Range(0, 10).Select(i => dataEntry1).ToList();
            var dataEntry2 =
                responseCompiler.Compile(ServerResponseTestData.SingleResponseJsonString_ASCE7_10);
            data = data.Concat(Enumerable.Range(0, 7).Select(_ => dataEntry2)).ToList();
            // TODO: fix, spectrum is null, response compiler should set empty dict
            var dataEntry3 =
                responseCompiler.Compile(ServerResponseTestData.SingleResponseJsonString_ASCE7_16);
            data = data.Concat(Enumerable.Range(0, 11).Select(_ => dataEntry3)).ToList();
            var dataTableByRefDoc = new TableBuilderServerResponse().CreateGroupedByRefDoc(data);

            // WHEN
            ExcelWriter.Write(file, dataTableByRefDoc);

            // THEN
            Assert.True(File.Exists(file));
        }
    }
}