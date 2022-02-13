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

using Parameters;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using BusgsReflect;
using SeismicStandards;

namespace BatchRequest
{
    public class ServerRequestError : Exception
    {
        private RequestParameters _requestParameters;

        public ServerRequestError(RequestParameters parameters, Exception inner)
            : base(parameters.ToString(), inner)
        {
            this._requestParameters = parameters;
        }

        public RequestParameters Parameters => this._requestParameters;
    }

    public class BatchClient
    {
        private static IHttpClientFactory _httpClientFactory;

        public BatchClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Action<double> UpdateProgressCallback { get; set; }
        public Action FinishProgressCallback { get; set; }

        private int _requestCount = 0;
        private int _completedRequests = 0;

        public int RequestCount { get; }
        public int CompletedRequests { get; }

        private void IncrementProgress()
        {
            Interlocked.Increment(ref _completedRequests);
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            if (UpdateProgressCallback != null)
                UpdateProgressCallback((double)_completedRequests / (double)_requestCount);
        }

        private void InitProgress(int requestCount)
        {
            _requestCount = requestCount;
            if (UpdateProgressCallback != null)
                UpdateProgressCallback(0.0);
        }

        public void FinishProgress()
        {
            if (FinishProgressCallback != null)
                FinishProgressCallback();
        }

        public async Task<BatchResponse> RequestAsync(
            List<RequestParameters> requestParameters,
            string baseUrl = ""
        )
        {
            var token = new CancellationTokenSource().Token;
            return await RequestAsync(requestParameters, token, baseUrl);
        }

        public async Task<BatchResponse> RequestAsync(
            List<RequestParameters> requestParameters,
            CancellationToken token,
            string baseUrl = ""
        )
        {
            var _baseUrl = baseUrl != "" ? baseUrl : UsaStandards.BaseUrl;

            var httpClient = _httpClientFactory.CreateClient("BatchHttpClient");

            // timeout is not per request but for the http-client
            // with all connections open -> 5sec per request is
            // reasonable as some requests might be slower, others fast
            int timeOutSec = requestParameters.Count * 5;
            string contentType = "application/json";
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.Timeout = new TimeSpan(0, 0, timeOutSec);

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            httpClient.DefaultRequestHeaders.Add("User-Agent", $"{BusgsAssembly.Name}/{BusgsAssembly.GetVersion()}");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("Keep-Alive", "1000");
            httpClient.DefaultRequestHeaders.ConnectionClose = false;

            InitProgress(requestParameters.Count);
            List<Task<HttpResponseMessage>> gettingResponses = new();
            foreach (var param in requestParameters)
            {
                Task<HttpResponseMessage> getting = httpClient.GetAsync(param.EncodeRelativeUri(), token);
                gettingResponses.Add(getting);
            }

            Task<HttpResponseMessage[]> awaitingResponses = Task.WhenAll(gettingResponses);
            Task tProgress = Task.Run(
                async () => {
                    while (!awaitingResponses.IsCompleted)
                    {
                        await Task.Delay(100);
                        Interlocked.Exchange(ref _completedRequests, gettingResponses.Count(t => t.IsCompleted));
                        UpdateProgress();
                    }
                    FinishProgress();
                }, 
                token
            );

            HttpResponseMessage[] responses = await awaitingResponses;
            List<HttpResponseMessage> successfulResponses = responses.Where(r => r.IsSuccessStatusCode).ToList();
            List<HttpResponseMessage> failedResponses = responses.Where(r => !r.IsSuccessStatusCode).ToList();
            return await BatchResponse.CreateInstance(successfulResponses, failedResponses);
        }
    }
}