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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

using Busgs.ViewModel;
using ExcelIO;
using JsonIO;
using BaseIO;
using Parameters;
using UsgsResponseCompile;
using SimpleTable;
using DebugUtils;
using WriteSamples;

namespace Busgs.ModelAction
{
    public class FileReadAction : AModelAction
    {
        public ServerRequestViewModel ViewModel{ get; set; }

        override public async Task Perform()
        {
            var filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add(".xlsx");
            filePicker.FileTypeFilter.Add(".json");

            List<RequestParametersInput> requestParametersInput;

            try
            {
                StorageFile storageFile = await filePicker.PickSingleFileAsync();
                if (storageFile != null)
                {
                    if (storageFile.FileType.ToLower() == ".xlsx")
                        requestParametersInput = await readExcelFileAsync(storageFile);
                    else if (storageFile.FileType.ToLower() == ".json")
                        throw new NotImplementedException(
                            "Reading *.json is not yet implemented. Please select an Excel-file instead."
                        );
                    else
                        throw new ArgumentException($"Invalid file type {storageFile.FileType}");

                    this.ViewModel.SetRequestParameters(
                        requestParametersInput,
                        storageFile.Name
                    );
                }
            } 
            catch (Exception e)
            {
                this.ViewModel.NotifyReading(new Notification(e.Message, NotificationStatus.Error, source: e));
            }
        }

        private static async Task<List<RequestParametersInput>> readExcelFileAsync(StorageFile storageFile)
        {
            var buffer = await FileIO.ReadBufferAsync(storageFile);
            var memoryStream = new MemoryStream();
            await buffer.AsStream().CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            List<RequestParametersInput> requestParametersInput = 
                ExcelReader.ReadFromStream(memoryStream, "Batch", new TableHeader(1, 0));
            memoryStream.Dispose();
            return requestParametersInput;
        }
    }

    public class FileWriteAction : AModelAction
    {
        public ServerRequestViewModel ViewModel{ get; set; }

        override public async Task Perform()
        {
            var filePicker = new FileSavePicker();
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeChoices.Add("Microsoft Excel", new List<string>(){ ".xlsx" });
            filePicker.SuggestedFileName = "design-parameters";

            try
            {
                StorageFile storageFile = await filePicker.PickSaveFileAsync();
                if (storageFile != null)
                {
                    var memoryStream = new MemoryStream();
                    ExcelWriter.WriteToStream(memoryStream, ViewModel.ServerResponseDataTables);

                    await FileIO.WriteBytesAsync(storageFile, memoryStream.ToArray());

                    this.ViewModel.NotifySaving(
                        new Notification($"Successfully wrote server respones to \"{storageFile.Name}\"")
                    );
                }
                else
                {
                    this.ViewModel.NotifySaving(
                        new Notification("Saving cancled", NotificationStatus.Warning)
                    );
                }
            }
            catch (Exception e)
            {
                this.ViewModel.NotifySaving(new Notification(e.Message, NotificationStatus.Error, source: e));
            }
        }
    }

    public class FileWriteSampleInputAction : AModelAction
    {
        override public async Task Perform()
        {
            var filePicker = new FileSavePicker();
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeChoices.Add("Microsoft Excel", new List<string>(){ ".xlsx" });
            filePicker.SuggestedFileName = "busgs-sample-input";

            try
            {
                StorageFile storageFile = await filePicker.PickSaveFileAsync();
                if (storageFile != null)
                {
                    var memoryStream = new MemoryStream();
                    Excel.WriteSampleRequestParameters(memoryStream);

                    await FileIO.WriteBytesAsync(storageFile, memoryStream.ToArray());
                    var dialog = new ContentDialog()
                    {
                        Title = "Wrote Sample File",
                        Content = $"Successfully wrote \"{storageFile.Name}\"",
                        CloseButtonText = "Ok"
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception e)
            {
                var dialog = new ContentDialog(){
                    Title = "Error occurred",
                    Content = e.Message,
                    CloseButtonText = "Ok"
                };
                await dialog.ShowAsync();
            }
        }
    }
}