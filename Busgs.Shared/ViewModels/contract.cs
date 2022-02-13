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

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Busgs.ViewModel
{
    public abstract class AViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // TODO: move notification stuff to separate file?
    public enum NotificationStatus
    {
        Info,
        Warning,
        Error
    }


    public class Notification
    {
        public object Source{ get; set; }
        public string Message{ get; set; }
        public string MessageWithStatus => $"{this.Status.ToString()} - {this.Message}";
        public string MessageWithErrorStatus 
            => $"{(this.Status == NotificationStatus.Error ? this.Status.ToString() + "! - " : "")}{this.Message}";
        public NotificationStatus Status{ get; set; }

        public Notification(string message)
        {
            this.Message = message;
            this.Status = NotificationStatus.Info;
        }
        public Notification(string message, NotificationStatus status)
        {
            this.Message = message;
            this.Status = status;
        }
        public Notification(string message, NotificationStatus status, object source = null) : this(message, status)
        {
            this.Source = source;
        }
        public Notification(string message, object source = null) : this(message)
        {
            this.Source = source;
        }

        public string Color
        {
            get
            {
                switch(this.Status)
                {
                    case NotificationStatus.Error:
                        return "Red";
                    case NotificationStatus.Warning:
                        return "Blue";
                    default:
                        return "{StaticResource TextFillColorPrimary}";
                } 
            }
        }

        override public string ToString() => this.MessageWithStatus;
    }

}