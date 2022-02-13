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
using SeismicStandards;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Parameters
{
    sealed public class RequestParametersInput
    {

        public static RequestParametersInput FromDict(Dictionary<string, object> data)
        {
            Dictionary<string, Type> expected_types = new Dictionary<string, Type>(){
                {"RefDoc", typeof(string)},
                {"Longitude", typeof(double)},
                {"Latitude", typeof(double)},
                {"RiskCategory", typeof(string)},
                {"SiteClass", typeof(string)},
                {"Title", typeof(string)}
            };

            if (!Enumerable.SequenceEqual(data.Keys.OrderBy(e => e), expected_types.Keys.OrderBy(e => e)))
            {
                throw new ArgumentException(
                    String.Format(
                        "Invalid names {0}: expected {1} (in any order)",
                        String.Join(", ", expected_types.Keys.OrderBy(e => e).Select(e => "\"" + e + "\"")),
                        String.Join(", ", data.Keys.OrderBy(e => e).Select(e => "\"" + e + "\""))
                    )
                );
            }

            foreach (var kvp in data)
            {
                var value = kvp.Value;
                if (kvp.Value.GetType() != expected_types[kvp.Key])
                {
                    throw new ArgumentException(
                        String.Format(
                            "Invalid type {0} for \"{1}\"={2}: expected {3}",
                            kvp.Value.GetType(), kvp.Key, kvp.Value, expected_types[kvp.Key]
                        )
                    );
                }
            }

            return new RequestParametersInput(
                stdName: (string)data["RefDoc"],
                lat: (double)data["Latitude"],
                lng: (double)data["Longitude"],
                riskCatId: (string)data["RiskCategory"],
                siteClassId: (string)data["SiteClass"],
                title: (string)data["Title"]
            );
        }

        public RequestParametersInput()
        {
            this.RefDoc = "";
            this.RiskCategory = "";
            this.SiteClass = "";
            this.Title = "";
            this.Latitude = 0.0;
            this.Longitude = 0.0;
        }

        public RequestParametersInput(string stdName, double lat, double lng, string riskCatId, string siteClassId, string title)
        {
            this.RefDoc = stdName;
            this.RiskCategory = riskCatId;
            this.SiteClass = siteClassId;
            this.Latitude = lat;
            this.Longitude = lng;
            this.Title = title;
        }

        public RequestParameters ToRequestParameters()
        {
            return new RequestParameters(
                refDoc: UsaStandards.standards.Single(st => st.Name == this.RefDoc),
                lat: this.Latitude,
                lng: this.Longitude,
                riskCat: SeismicStandards.RiskCategory.riskCategories.Single(rc => rc.id == this.RiskCategory),
                siteClass: SeismicStandards.SiteClass.siteClasses.Single(sc => sc.id == this.SiteClass),
                title: this.Title
            );
        }

        public string RefDoc { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string RiskCategory { get; set; }
        public string SiteClass { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return string.Format(
                "RefDoc='{0}', Lat={1}, Long={2}, RiskCat={3}, SiteClass={4}, Title='{5}'",
                this.RefDoc, this.Latitude, this.Longitude, this.RiskCategory, this.SiteClass, this.Title
            );
        }
    }


    public static class SampleInputParameters
    {
        public static ImmutableArray<RequestParametersInput> GetSamples()
        {
            return ImmutableArray.Create<RequestParametersInput>(new RequestParametersInput[]{
                new RequestParametersInput(stdName: "ASCE7-05", lat:34.1, lng:118.01, riskCatId:"III", siteClassId:"D", title:"Example ASCE7-05"),
                new RequestParametersInput(stdName: "ASCE7-10", lat:34.1, lng:118.01, riskCatId:"III", siteClassId: "D", title:"Example ASCE7-10"),
                new RequestParametersInput(stdName: "ASCE7-16", lat:34.1, lng:118.01, riskCatId:"III", siteClassId: "D", title:"Example ASCE7-16"),
                new RequestParametersInput(stdName: "IBC-2012", lat:34.1, lng:118.01, riskCatId:"III", siteClassId: "D", title:"Example IBC-2012"),
                new RequestParametersInput(stdName: "IBC-2015", lat:34.1, lng:118.01, riskCatId:"III", siteClassId: "D", title:"Example IBC-2015"),
            });
        }
    }

    sealed public class RequestParameters : IDeepCopy<RequestParameters>
    {
        private Standard _standard;
        private RiskCategory _riskCategory;
        private SiteClass _siteClass;
        private string _title;
        private double _latitude;
        private double _longitude;

        public RequestParameters(
            Standard refDoc, double lat, double lng, RiskCategory riskCat, SiteClass siteClass, string title
        )
        {
            this._standard = refDoc;
            this._latitude = lat;
            this._longitude = lng;
            this._riskCategory = riskCat;
            this._siteClass = siteClass;
            this._title = title;
        }

        public Standard Standard => this._standard;
        public double Latitude => this._latitude;
        public double Longitude => this._longitude;
        public RiskCategory RiskCategory => this._riskCategory;
        public SiteClass SiteClass => this._siteClass;
        public string Title => this._title;

        /// <Summary>
        /// Encode the full URI (including base URI). 
        /// Use this is the the base URI was not set on the HttpClient. 
        /// </Summary>
        public string EncodeUri(string baseUri)
        {
            string full = $"{baseUri}/{EncodeRelativeUri()}";
            return full;
        }

        /// <Summary>
        /// Encode relative uri (excluding base URI). 
        /// Use this is the the base URI is set on the HttpClient. 
        /// </Summary>
        public string EncodeRelativeUri()
        {
            string titleEncoded = System.Uri.EscapeDataString(this.Title);
            string full = $"{_standard.UrlRequestRelative}?"
                        + $"latitude={Latitude}&longitude={Longitude}&riskCategory={RiskCategory}"
                        + $"&siteClass={SiteClass}&title={titleEncoded}";
            return full; 
        }

        public RequestParameters DeepCopy()
        {
            return new RequestParameters(
                this._standard,
                this._latitude,
                this.Longitude,
                this._riskCategory,
                this._siteClass,
                this._title
            );
        }

        public override string ToString()
        {
            return this.Standard + ", " + this.Latitude + ", " + this.Longitude + ", "
                + this.SiteClass + ", " + this.RiskCategory + ", " + this._title;
        }
    }

}