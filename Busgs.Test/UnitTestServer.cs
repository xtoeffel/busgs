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
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

using DIServices;
using Parameters;
using ExcelIO;
using Busgs.Test.Base;
using BatchRequest;
using SeismicStandards;

namespace Busgs.Test.Server
{
    public class ServerRequestTest : BaseTest
    {
        [Fact]
        public async Task BatchRequestTest_Success()
        {

            string file = this.ResolveDataFileName("seismic_batch_sample.xlsx");
            List<RequestParametersInput> inpParams = ExcelReader.Read(file, "Batch", new(1, 0));
            List<RequestParameters> requestParams = inpParams.Select(
                ip => ip.ToRequestParameters()
            ).ToList();
            List<string> encoded = requestParams.Select(
                rp => rp.EncodeUri(SeismicStandards.UsaStandards.BaseUrl)
            ).ToList();

            IHttpClientFactory httpClientFactory = 
                ServiceBuilder.GetHost().Services.GetRequiredService<IHttpClientFactory>();
            BatchClient batchRequest = new(httpClientFactory);
            Task<BatchResponse> requesting = batchRequest.RequestAsync(requestParams);
            BatchResponse response = await requesting;

            Assert.Equal(5, response.SuccessResponses.Count);
            Assert.Empty(response.FailedResponses);
            Assert.Equal(5, response.TotalCount);

        }
    }
}