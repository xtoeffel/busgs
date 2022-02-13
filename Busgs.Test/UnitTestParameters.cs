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
using Parameters;
using System.Collections.Generic;

namespace Busgs.Test
{
    public class RequestParametersTest
    {
        [Fact]
        public void InputToRequest_Success()
        {
            // -- GIVEN --
            RequestParametersInput inpParams = new()
            {
                RefDoc = "ASCE7-10",
                Title = "Test",
                Latitude = -123.23,
                Longitude = -9.38743,
                SiteClass = "D",
                RiskCategory = "II"
            };

            // -- WHEN --
            RequestParameters param = inpParams.ToRequestParameters();

            // -- THEN --
            Assert.Equal("Test", param.Title);
            Assert.Equal(SeismicStandards.SiteClass.D, param.SiteClass);
            Assert.Equal(SeismicStandards.UsaStandards.ASCE7_10, param.Standard);
            Assert.Equal(SeismicStandards.RiskCategory.II, param.RiskCategory);
            Assert.Equal(-123.23, param.Latitude);
            Assert.Equal(-9.38743, param.Longitude);
        }


        [Fact]
        public void DictionaryToRequestInput_Success()
        {
            // -- GIVEN --
            var data = new Dictionary<string, object>(){
                {"RefDoc", "ASCE7-10"},
                {"Latitude", -123.23},
                {"Longitude", -9.2348},
                {"RiskCategory", "III"},
                {"SiteClass", "D"},
                {"Title", "Test Dict"}
            };

            // -- WHEN --
            var param_input = RequestParametersInput.FromDict(data);

            // -- THEN --
            Assert.Equal("Test Dict", param_input.Title);
            Assert.Equal("D", param_input.SiteClass);
            Assert.Equal("III", param_input.RiskCategory);
            Assert.Equal("ASCE7-10", param_input.RefDoc);
            Assert.Equal(-123.23, param_input.Latitude);
            Assert.Equal(-9.2348, param_input.Longitude);
        }
    }
}
