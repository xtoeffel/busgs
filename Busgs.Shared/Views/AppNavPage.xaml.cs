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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Busgs.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppNavPage : Page
    {
        public AppNavPage()
        {
            this.InitializeComponent();
        }

        private void AppNavBar_Loaded(object sender, RoutedEventArgs e)
        {
            appMainFrame.Navigate(typeof(ServerRequest));
            NavigationHeaderImage.Source = new SvgImageSource(new Uri("ms-appx:///Assets/Images/server.svg"));
            NavigationHeaderTextBlock.Text = "Server Request";
        }


        private void AppNavBar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs navArgs)
        {
            var selectedItem = navArgs.SelectedItem as NavigationViewItem;
            var selectedItemTag = selectedItem.Tag.ToString();
            NavigationHeaderTextBlock.Text = selectedItemTag;
            switch (selectedItemTag)
            {
                case "Server Request":
                    appMainFrame.Navigate(typeof(ServerRequest));
                    NavigationHeaderImage.Source = new SvgImageSource(new Uri("ms-appx:///Assets/Images/server.svg"));
                    break;

                case "About":
                    appMainFrame.Navigate(typeof(AboutPage));
                    NavigationHeaderImage.Source = new SvgImageSource(new Uri("ms-appx:///Assets/Images/about.svg"));
                    break;
            }
        }
    }
}
