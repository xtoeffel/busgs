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

using Windows.UI.Xaml.Controls;
using Busgs.ViewModel;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using Parameters;
using BatchRequest;
using DIServices;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Busgs.ModelAction
{
    public class ServerRequestAction : AModelAction
    {
        public ServerRequestViewModel ViewModel{ get; init; }

        private double _progressValue = 0.0;
        private CancellationTokenSource _cancelTokenSource = null;
        public double ProgressValue => _progressValue;

        private void updateProgress(double progressValue)
        {
            _progressValue = progressValue;
            OnPropertyChanged("ProgressValue");
        }

        private void finishRequest()
        {
            _progressValue = 1.0;
            OnPropertyChanged("ProgressValue");
        }

        public bool IsCancelable
            => _cancelTokenSource != null;

        public void Cancel()
        {
            // no action will be performed if IsCancable is False
            // that's because the request might be done already but the 
            // UI not updated at the point when the user pressed Cancel
            if (IsCancelable)
                _cancelTokenSource.Cancel();
        }

        override public async Task Perform()
        {
            try 
            {
                _cancelTokenSource = new CancellationTokenSource();
                // fire property changed event to allow UI to update
                OnPropertyChanged("IsCancelable");

                updateProgress(0.0);
                if (!ViewModel.IsRequestingEnabled)
                    throw new ArgumentException("server request is disabled by view-model");

                ViewModel.InitServerRequest();

                List<RequestParameters> requestParameters = 
                    ViewModel.RequestParametersInput.Select(p => p.ToRequestParameters()).ToList();

                var host = ServiceBuilder.GetHost();
                var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
                var client = new BatchClient(httpClientFactory);
                client.UpdateProgressCallback = updateProgress;
                client.FinishProgressCallback = finishRequest;

                BatchResponse response;
                Task<BatchResponse> requesting = 
                    client.RequestAsync(requestParameters, _cancelTokenSource.Token);
                response = await requesting;

                ViewModel.SetServerResponse(response);
            }
            catch (OperationCanceledException)
            {
                ViewModel.NotifyRequesting(new Notification(
                    "Cancelled server request", NotificationStatus.Warning
                ));
            }
            catch (Exception e)
            {
                
                ViewModel.NotifyRequesting(new Notification(e.Message, NotificationStatus.Error, source: e));
            }
            finally
            {
                _cancelTokenSource.Dispose();
                _cancelTokenSource = null;
            }
        }
    }
}