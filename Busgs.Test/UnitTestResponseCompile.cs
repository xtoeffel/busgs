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

using UsgsResponseCompile;
using Xunit;
using Busgs.Test.Data;
using System.Collections.Generic;
using UsgsResponseData;
using System.Linq;

namespace Busgs.Test.ResponseCompile
{
    public class TestResponseCompile
    {
        [Fact]
        public void CompileSingleServerResponse_Success()
        {
            // -- GIVEN --
            string response = ServerResponseTestData.SingleResponseJsonString_IBC2012;

            // -- WHEN --
            ServerResponseCompiler compiler = new();
            ServerResponseData compiled = compiler.Compile(response);

            // -- THEN --
            Assert.Equal("IBC-2012", compiled.Request.ReferenceDocument);
            Assert.Equal("success", compiled.Request.Status);
            Assert.Equal(
                "https://earthquake.usgs.gov/ws/designmaps/ibc-2012.json?latitude=34.1&longitude=-117.2&riskCategory=I&siteClass=A&title=Example IBC-2012",
                compiled.Request.Url
            );
            Assert.Equal("11/13/2021 8:17:16 PM", compiled.Request.Date.ToLocalTime().ToString());
            Assert.Equal("Example IBC-2012", compiled.Request.Parameters.Title);
            Assert.Equal("A", compiled.Request.Parameters.SiteClass);
            Assert.Equal("I", compiled.Request.Parameters.RiskCategory);
            Assert.Equal(34.1, compiled.Request.Parameters.Latitude);
            Assert.Equal(-117.2, compiled.Request.Parameters.Longitude);
            // TODO: add remaining unit test stuff
            var expectedBaseValueKeys = new string[]{
                "pgauh", "pgad", "pga", "fpga", "pgam", "ssrt", "crs", "ssuh", "ssd", "ss", "fa", "sms", "sds",
                "sdcs", "s1rt", "cr1", "s1uh", "s1d", "s1", "fv", "sm1", "sd1", "sdc1", "sdc", "tl", "t-sub-l"
            };
            Assert.Equal(expectedBaseValueKeys, compiled.SeismicDesignData.OrderedKeysBaseValues);
            var expectedSpectraKeys = new string[]{
                "twoPeriodDesignSpectrum", "twoPeriodMCErSpectrum"
            };
            Assert.Equal(expectedSpectraKeys, compiled.SeismicDesignData.OrderedKeysSpectra);
            var expectedMetadataKeys = new string[]{
                "modelVersion", "pgadFloor", "pgadPercentileFactor", "s1MaxDirFactor", "s1dFloor",
                "s1dPercentileFactor", "spatialInterpolationMethod", "ssMaxDirFactor", "ssdFloor",
                "ssdPercentileFactor",
            };
            Assert.Equal(expectedMetadataKeys, compiled.SeismicDesignData.OrderedKeysMetaData);
        }

        [Fact]
        public void CompileSingleServerResponseSpectraEmpty_Success()
        {
            // GIVEN
            string response = ServerResponseTestData.SingleResponseJsonString_ASCE7_16;

            // -- WHEN --
            ServerResponseCompiler compiler = new();
            ServerResponseData compiled = compiler.Compile(response);

            // -- THEN --
            Assert.Equal("ASCE7-16", compiled.Request.ReferenceDocument);
            Assert.True(compiled.SeismicDesignData.BaseValues.All(kvp => !kvp.Key.ToLower().EndsWith("spectrum")));
            Assert.Equal(4, compiled.SeismicDesignData.Spectra.Count);
            // TODO: add some more tests
            var expectedBaseValueKeys = new string[]{
                "pgauh", "pgad", "pga", "fpga", "pgam", "ssrt", "crs", "ssuh", "ssd", "ss",
                "fa", "sms", "sds", "sdcs", "s1rt", "cr1", "s1uh", "s1d", "s1", "ts", "t0",
                "fv", "fv_note", "sm1", "sd1", "sdc1", "sdc", "tl", "t-sub-l", "cv",
            };
            Assert.Equal(expectedBaseValueKeys, compiled.SeismicDesignData.OrderedKeysBaseValues);
            var expectedSpectraKeys = new string[]{
                "twoPeriodDesignSpectrum", "twoPeriodMCErSpectrum", "verticalDesignSpectrum", "verticalMCErSpectrum"
            };
            Assert.Equal(expectedSpectraKeys, compiled.SeismicDesignData.OrderedKeysSpectra);
            foreach (var kvp in compiled.SeismicDesignData.Spectra)
                Assert.Empty(kvp.Value);
        }

        [Fact]
        public void CompileTableServerResponse_Success()
        {
            // TODO: test compilation of table from ServerResponseData
        }
    }
}