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
using Windows.UI.Xaml.Controls;
using System.Windows.Input;

namespace DebugUtils
{
    public static class DebugToolsUI
    {
        public static async void ShowStackTrace(Exception e)
        {
            var ex = e;

            var os = Environment.OSVersion;

            string output = os.Platform.ToString() + "\n";
            while (ex != null)
            {
                output += ex.ToString() + "\n";
                ex = ex.InnerException;
            }

            var tb = new TextBlock(){ 
                Text = output,
                IsTextSelectionEnabled = true
            };
            var dialog = new ContentDialog()
            {
                Title = "Error occurred",
                Content = new ScrollViewer(){ Content = tb },
                CloseButtonText = "Ok"
            };
            await dialog.ShowAsync();
        }
    }
}