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
using System.Collections.Generic;
using UsgsResponseData;
using System.Linq;
using System;
using Parameters;

namespace SimpleTable
{
    public class SimpleDataTable
    {
        public ImmutableArray<ImmutableArray<string>> Header { get; init; }
        public ImmutableArray<ImmutableArray<object>> Body { get; init; }

        public bool IsEmpty => Body.IsEmpty;
    }

    public abstract class TableBuilder<T>
    {
        public abstract SimpleDataTable Create(IEnumerable<T> data);
    }

    public class TableBuilderServerResponse : TableBuilder<ServerResponseData>
    {
        public override SimpleDataTable Create(IEnumerable<ServerResponseData> data)
        {
            var orderedKeysBaseValues = data.First().SeismicDesignData.OrderedKeysBaseValues;
            var orderedKeysMetaData = data.First().SeismicDesignData.OrderedKeysMetaData;

            var header = new List<string>() { "Latitude", "Longitude", "RiskCat", "SiteClass", "Title" };
            header = header.Concat(data.First().SeismicDesignData.OrderedKeysBaseValues).ToList();
            header = header.Concat(data.First().SeismicDesignData.OrderedKeysMetaData).ToList();

            var body = new List<ImmutableArray<object>>();
            foreach (var responseData in data)
            {
                if (!Enumerable.SequenceEqual(orderedKeysBaseValues, responseData.SeismicDesignData.OrderedKeysBaseValues))
                    throw new ArgumentException(
                        $"Creation of table failed due to deviating ordered keys: {String.Join(", ", orderedKeysBaseValues)} " +
                        $"!= {String.Join(", ", responseData.SeismicDesignData.OrderedKeysBaseValues)}"
                    );

                var row = new List<object>(){
                    responseData.Request.Parameters.Latitude,
                    responseData.Request.Parameters.Longitude,
                    responseData.Request.Parameters.RiskCategory,
                    responseData.Request.Parameters.SiteClass,
                    responseData.Request.Parameters.Title
                };
                foreach (var key in orderedKeysBaseValues)
                    row.Add(responseData.SeismicDesignData.BaseValues[key]);
                foreach (var key in orderedKeysMetaData)
                    row.Add(responseData.SeismicDesignData.MetaData[key]);

                body.Add(ImmutableArray.Create<object>(row.ToArray()));
            }

            return new SimpleDataTable()
            {
                Header = ImmutableArray.Create(ImmutableArray.Create<string>(header.ToArray())),
                Body = ImmutableArray.Create(body.ToArray())
            };
        }

        public ImmutableDictionary<string, SimpleDataTable> CreateGroupedByRefDoc(
            IEnumerable<ServerResponseData> data
        )
        {
            var refDocs = data.Select(e => e.Request.ReferenceDocument).Distinct();

            var groupedTables = new Dictionary<string, SimpleDataTable>();
            foreach (var refDoc in refDocs)
            {
                var dataOfGroup = data.Where(e => e.Request.ReferenceDocument == refDoc);
                groupedTables[refDoc] = Create(dataOfGroup);
            }

            return groupedTables.ToImmutableDictionary();
        }
    }

    public class TableBuilderRequestParameters : TableBuilder<RequestParameters>
    {
        public override SimpleDataTable Create(IEnumerable<RequestParameters> data)
        {
            var header = new List<string>() { "Title", "RefDoc", "Latitude", "Longitude", "SiteClass", "RiskCategory" };

            var body = new List<ImmutableArray<object>>();
            foreach (var reqParas in data)
            {
                var row = new List<object> {
                    reqParas.Title,
                    reqParas.Standard.Name,
                    reqParas.Latitude,
                    reqParas.Longitude,
                    reqParas.SiteClass.ToString(),
                    reqParas.RiskCategory.ToString(),
                };
                body.Add(ImmutableArray.Create<object>(row.ToArray()));
            }

            return new SimpleDataTable
            {
                Header = ImmutableArray.Create(ImmutableArray.Create(header.ToArray())),
                Body = ImmutableArray.Create(body.ToArray())
            };
        }
    }
}