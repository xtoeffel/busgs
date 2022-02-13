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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;

namespace DIServices
{
    public static class ServiceBuilder
    {
        private static IHost _host;

        public static IHost GetHost()
        {
            if (_host == null)
                _host = Build();

            return _host;
        }

        /*
        ISSUE:
        Performing batch requests to `https://earthquake.usgs.gov/ws/designmaps/`
        (and the server locally running in docker container)
        with no limitation on the number of simultaneous connections often 
        fail with errors:
        - Socket hang up
        - Client network socket disconnected before secure TLS connection was 
          established
        Larger requests >~30 will have failed requests for the first attempt
        of batch request. The second attempt will likely pass but even then
        some requests might fail.

        Not fully clear if this issue is due to the client or the server.
        MaxConnectionPerServer = 5 seems to work w/ little to no errors
        for local or global server. 
        Some platforms will not allow this setting, like WASM.
        */
        private static IHost Build()
        {
            // not all platforms support creation of HttpClientHandler
            // set it to null if not supported on platform
            HttpClientHandler httpClientHandler;
            try
            {
                httpClientHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    MaxConnectionsPerServer = 5
                };
            }
            catch (PlatformNotSupportedException)
            {
                httpClientHandler = null;
            }

            IHost host;
            if (httpClientHandler == null)
            {
                // HttpClientHandler not supported on executing platform
                host = new HostBuilder().ConfigureServices(services => {
                    services.AddHttpClient("BatchHttpClient");
                })
                .Build();
            }
            else 
            {
                host = new HostBuilder().ConfigureServices(services => {
                    services.AddHttpClient("BatchHttpClient")
                        .ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);
                })
                .Build();
            }

            return host;
        }
    }
}