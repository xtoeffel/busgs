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

using System.Collections.Immutable;
using System.Threading.Tasks;
using System;
using BatchRequest;
using UsgsResponseCompile;
using UsgsResponseData;
using SimpleTable;
using Busgs.ViewModel;

namespace Busgs.ModelAction
{

    public class CompileResponseAction : AModelAction
    {
        public ServerRequestViewModel ViewModel { get; init; }
        public BatchResponse ServerResponse { get; init; }

        override public async Task Perform()
        {
            ViewModel.InitCompileServerResponses();
            await Task.Yield();

            // TODO: put in Task.Run() as this would be supported Windows (not WASM though)
            ServerResponseCompiler responseCompiler = new ServerResponseCompiler();
            // TODO: include the failed responses w/ error message
            var compiledResponse = responseCompiler.Compile(ViewModel.SuccessServerResponseMessages);
            var tableBuilder = new TableBuilderServerResponse();
            ImmutableDictionary<string, SimpleDataTable> responseTables = tableBuilder.CreateGroupedByRefDoc(compiledResponse);

            ViewModel.SetServerResponseDataTables(responseTables);
        }
    }
}