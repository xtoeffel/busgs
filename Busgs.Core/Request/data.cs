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
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BatchRequest
{
    public sealed class BatchResponse
    {
        private ImmutableList<HttpResponseMessage> _successes;
        private ImmutableList<HttpResponseMessage> _fails;
        private ImmutableList<string> _successMessages;

        internal static async Task<BatchResponse> CreateInstance(
            List<HttpResponseMessage> successfulResponses, 
            List<HttpResponseMessage> failedResponses
        ){
            List<string> successMessages = new();
            foreach (var r in successfulResponses)
                successMessages.Add(await r.Content.ReadAsStringAsync());
            
            return new BatchResponse(successfulResponses, failedResponses, successMessages);
        }

        private BatchResponse(List<HttpResponseMessage> successfulResponses, List<HttpResponseMessage> failedResponses, List<string> successMessages)
        {
            _successes = ImmutableList.Create(successfulResponses.ToArray());
            _fails = ImmutableList.Create(failedResponses.ToArray());
            _successMessages = ImmutableList.Create(successMessages.ToArray());
        }

        public ImmutableList<HttpResponseMessage> SuccessResponses => _successes;
        public ImmutableList<HttpResponseMessage> FailedResponses => _fails;
        public ImmutableList<string> SuccessMessages => _successMessages;
        public int TotalCount => _successes.Count + _fails.Count;

    }

}