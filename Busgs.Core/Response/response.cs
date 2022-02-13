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
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleTable;

namespace UsgsResponseData
{
    public readonly struct SpectraEntry
    {
        public SpectraEntry(double time, double acc)
        {
            Time = time;
            Acceleration = acc;
        }

        public double Time { get; init; }
        public double Acceleration { get; init; }

        public override string ToString() => $"T={Time}, Acc={Acceleration}";
    }

    public sealed class DesignData
    {
        public ImmutableArray<string> OrderedKeysBaseValues { get; init; }
        public ImmutableArray<string> OrderedKeysSpectra { get; init; }
        public ImmutableArray<string> OrderedKeysMetaData { get; init; }
        public ImmutableDictionary<string, object> BaseValues { get; init; }
        public ImmutableDictionary<string, ImmutableArray<SpectraEntry>> Spectra { get; init; }
        public ImmutableDictionary<string, object> MetaData { get; init; }

        public bool HasSpectra => Spectra != null && !Spectra.IsEmpty;
    }

    public sealed class RequestData
    {

        public class SeismicParameters
        {
            public double Latitude { get; init; }
            public double Longitude { get; init; }
            public string SiteClass { get; init; }
            public string RiskCategory { get; init; }
            public string Title { get; init; }
        }

        public DateTime Date { get; init; }
        public string ReferenceDocument { get; init; }
        public string Status { get; init; }
        public string Url { get; init; }
        public SeismicParameters Parameters { get; init; }

    }

    public sealed class ServerResponseData
    {
        public RequestData Request { get; init; }
        public DesignData SeismicDesignData { get; init; }
    }
}