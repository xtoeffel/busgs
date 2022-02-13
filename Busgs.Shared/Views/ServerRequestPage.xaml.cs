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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using ExcelIO;
using JsonIO;
using BaseIO;
using Parameters;
using BatchRequest;
using UsgsResponseCompile;
using SimpleTable;
using Busgs.ViewModel;
using Busgs.ModelAction;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Busgs.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerRequest : Page
    {
        private ServerRequestViewModel _viewModel;
        private FileReadAction _fileReadAction;
        private ServerRequestAction _serverRequestAction;
        private FileWriteAction _fileWriteAction;
        private CompileResponseAction _compileResponseAction;
        private FileWriteSampleInputAction _fileWriteSampleInputAction;

        public ServerRequest()
        {
            InitializeComponent();

            _viewModel = new ServerRequestViewModel();

            _fileReadAction = new FileReadAction(){
                ViewModel = _viewModel
            };
            _fileWriteAction = new FileWriteAction(){
                ViewModel = _viewModel
            };
            _compileResponseAction = new CompileResponseAction(){
                ViewModel = _viewModel
            };
            _serverRequestAction = new ServerRequestAction(){
                ViewModel = _viewModel
            };
            _fileWriteSampleInputAction = new FileWriteSampleInputAction();
            
        }

        public ServerRequestViewModel ViewModel => _viewModel;
        public FileReadAction ReadAction => _fileReadAction;
        public FileWriteAction WriteAction => _fileWriteAction;
        public ServerRequestAction RequestAction => _serverRequestAction;
        public FileWriteSampleInputAction WriteSampleInputAction => _fileWriteSampleInputAction;

        public async void OnClick_SelectInputFileButton(object sender, RoutedEventArgs eArgs)
            => await _fileReadAction.Perform();
        public async void OnClick_RequestFromServerButton(object sender, RoutedEventArgs eArgs)
        {
            await _serverRequestAction.Perform();
            await _compileResponseAction.Perform();
        }
        public async void OnClick_SaveFileButton(object sender, RoutedEventArgs eArgs)
            => await _fileWriteAction.Perform();

        // does not work on WASM because it's currently running single thread only
        // future extension of Uno Platform likely support threads for WASM
        public void OnClick_CancelServerRequest(object sender, RoutedEventArgs eArgs)
            => _serverRequestAction.Cancel();

        public async void OnClick_WriteSampleInputFileLink(object sender, RoutedEventArgs eArgs)
            => await _fileWriteSampleInputAction.Perform();
    }

}
