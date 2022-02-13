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
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Parameters;
using DebugUtils;
using BatchRequest;
using SimpleTable;
using SeismicStandards;

namespace Busgs.ViewModel
{
    // ViewModel controls state of the View
    // it does not perform any actions, that's done on the model by ModelActions
    public class ServerRequestViewModel : AViewModel
    {
        private const bool DEBUG_MODE = true;

        private List<RequestParametersInput> _requestParametersInput = new(); 
        private List<string> _successServerResponseMessages = new();
        private BatchResponse _batchResponse = null;
        private bool _initServerRequest = false;
        private ImmutableDictionary<string, SimpleDataTable> _serverResponseDataTables;
        // TODO: remove and do over compile action?
        private bool _isCompilingServerResponses = false;

        // TODO: normal lists might be better, no event on list.Clear()
        private ObservableCollection<Notification> _readingNotes = new();
        private ObservableCollection<Notification> _requestingNotes = new();
        private ObservableCollection<Notification> _savingNotes = new();

        // -- public properties --
        public List<RequestParametersInput> RequestParametersInput => _requestParametersInput;
        public ObservableCollection<Notification> ReadingNotes => _readingNotes;
        public ObservableCollection<Notification> RequestingNotes => _requestingNotes;
        public ObservableCollection<Notification> SavingNotes => _savingNotes;
        public List<string> SuccessServerResponseMessages => _successServerResponseMessages;
        public ImmutableDictionary<string, SimpleDataTable> ServerResponseDataTables => _serverResponseDataTables;

        // -- helper properties and methods --
        public bool IsReadingErrorPresent 
            => _readingNotes.Any(n => n.Status == NotificationStatus.Error);
        public bool IsRequestingErrorPresent 
            => _requestingNotes.Any(n => n.Status == NotificationStatus.Error);
        public bool IsSavingErrorPresent 
            => _savingNotes.Any(n => n.Status == NotificationStatus.Error);
        private static string ToggleVisibility(string visibility) 
            => visibility == "Visible" ? "Collapsed" : "Visible";
        public bool IsCompilingServerResponses => _isCompilingServerResponses; 

        // -- view state control properties --
        public string ReadingUserHintsVisibility
            => ToggleVisibility(ReadingNotesVisibility);
        public string ReadingNotesVisibility 
            => _readingNotes.Count == 0 ? "Collapsed" : "Visible";

        public bool IsRequestingEnabled
            => (_requestParametersInput.Count > 0 && !IsReadingErrorPresent);
        public string RequestingUserHintsVisibility
            => ToggleVisibility(RequestingNotesVisibility);
        public string RequestingNotesVisibility 
            => (_requestingNotes.Count > 0 || _initServerRequest) ? "Visible" : "Collapsed";

        public bool IsSavingEnabled
            => (_serverResponseDataTables != null && _serverResponseDataTables.Count > 0 && !IsRequestingErrorPresent);
        public string SavingUserHintsVisibility
            => ToggleVisibility(SavingNotesVisibility);
        public string SavingNotesVisibility 
            => _savingNotes.Count == 0 ? "Collapsed" : "Visible";

        public string ProgressBarColor
        {
            get {
                if (_batchResponse == null)
                    return "#007acc";
                if (_batchResponse.FailedResponses.Count == 0)
                    return "#0d8646";
                if (_batchResponse.SuccessResponses.Count == 0)
                    return "#b80000";
                return "#ca7a00";
            }
        }

        // -- state manipulation methods --
        private void _Notify(
            string infoMessage, 
            ObservableCollection<Notification> target, 
            bool firePropertyChangeEvent = false
        ) => _Notify(new Notification(infoMessage), target, firePropertyChangeEvent);

        private void _Notify(
            Notification notification, 
            ObservableCollection<Notification> target, 
            bool firePropertyChangeEvent = false
        )
        {
            if (notification.Status == NotificationStatus.Error)
                target.Clear();
                if (DEBUG_MODE && notification.Source != null && notification.Source is Exception) 
                    DebugToolsUI.ShowStackTrace((Exception)notification.Source);

            target.Add(notification);
            if (firePropertyChangeEvent)
                OnPropertyChanged("");
        }
        public void InitServerRequest()
        {
            _initServerRequest = true;
            _requestingNotes.Clear();
            _requestingNotes.Add(new Notification($"Requesting from {UsaStandards.BaseUrl} ..."));
            OnPropertyChanged("");
        }
        public void InitCompileServerResponses()
        {
            if (_successServerResponseMessages.Count == 0)
                throw new ArgumentException("Server response empty, nothing to complie");
            
            _isCompilingServerResponses = true;
            string tailMessage = _successServerResponseMessages.Count > 15 ? "(this might take a moment)" : "";
            _requestingNotes.Add(new Notification($"Compiling server responses ... {tailMessage}"));
            OnPropertyChanged("");
        }

        // -- notification methods (all fire property change events) --
        public void NotifyReading(Notification notification)
        {
            if (notification.Status == NotificationStatus.Error)
                _initServerRequest = false;
            _Notify(notification, _readingNotes, true);
        }
        public void NotifyRequesting(Notification notification)
        {
            if (notification.Status == NotificationStatus.Error)
                _initServerRequest = false;
            _Notify(notification, _requestingNotes, true);
        }
        public void NotifySaving(Notification notification)
        {
            _Notify(notification, _savingNotes, true);
        }

        private void Clear()
        {
            _requestParametersInput.Clear();
            _successServerResponseMessages.Clear();
            _serverResponseDataTables = null;
            _readingNotes.Clear();
            _requestingNotes.Clear();
            _savingNotes.Clear();
        }

        // -- set new data methods --
        public void SetRequestParameters(List<RequestParametersInput> parameters, string source)
        {
            string referenceDocuments = 
                String.Join(", ", parameters.Select(p => p.RefDoc).Distinct().OrderBy(s => s));
            string riskCategories = 
                String.Join(", ", parameters.Select(p => p.RiskCategory).Distinct().OrderBy(s => s));
            string siteClasses = 
                String.Join(", ", parameters.Select(p => p.SiteClass).Distinct().OrderBy(s => s));

            Clear();
            _initServerRequest = false;

            _Notify($"Successfully read \"{source}\"", _readingNotes);
            _Notify($"Found {parameters.Count} parameter sets", _readingNotes);
            _Notify($"Reference documents: {referenceDocuments}", _readingNotes);
            _Notify($"Risk categories: {riskCategories}", _readingNotes);
            _Notify($"Site classes: {siteClasses}", _readingNotes);

            foreach (var p in parameters)
                _requestParametersInput.Add(p);

            OnPropertyChanged("");
        }

        // TODO: should the Set...() methods be async? but then all has to be in a Task.Run()??!!!!
        public void SetServerResponse(BatchResponse batchResponse)
        {
            if (_successServerResponseMessages.Count != 0)
                _successServerResponseMessages.Clear();

            _requestingNotes.Clear();
            _Notify(
                $"Received {batchResponse.TotalCount} responses from server: "
                + $"{batchResponse.SuccessResponses.Count} were successful, "
                + $"{batchResponse.FailedResponses.Count} failed", 
                _requestingNotes
            );

            _batchResponse = batchResponse;
            foreach(string r in batchResponse.SuccessMessages)
                _successServerResponseMessages.Add(r);

            // TODO: write also fail messages to allow for later compilation and writing to file
            OnPropertyChanged("");
        }

        public void SetServerResponseDataTables(ImmutableDictionary<string, SimpleDataTable> response)
        {
            _isCompilingServerResponses = false;
            _serverResponseDataTables = response;
            _Notify($"Compiled responses, ready to save to file", _requestingNotes);
            OnPropertyChanged("");
        }

    }
}