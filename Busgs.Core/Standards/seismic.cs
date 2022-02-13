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

namespace SeismicStandards
{
    public record Standard
    {
        public Standard(int year, string name, string UrlRequestRelative, string UrlDocRelative)
        {
            this.Year = year;
            this.Name = name;
            this.UrlRequestRelative = UrlRequestRelative;
            this.UrlDocRelative = UrlDocRelative;
        }

        public int Year{ get; init; }
        public string Name{ get; init; }
        public string UrlRequestRelative{ get; init; }
        public string UrlDocRelative{ get; init; }

        public string GetUrlRequest(string baseUrl) => baseUrl + UrlRequestRelative;
        public string GetUrlDoc(string baseUrl) => baseUrl + UrlDocRelative;
    };

    public static class UsaStandards
    {
        // use local or global https; global http is for testing only
        internal static string _LocalServerUrl = "http://localhost:4000/quake/";
        internal static string _GlobalServerUrlHttps = "https://earthquake.usgs.gov/ws/designmaps/";
        internal static string _GlobalServerUrlHttp = "http://earthquake.usgs.gov/ws/designmaps/";

        public static string BaseUrl => _GlobalServerUrlHttps;

        public static readonly Standard IBC_2012 = new(
            2012, "IBC-2012",
            "ibc-2012.json",
            "ibc-2012.html"
        );
        public static readonly Standard IBC_2015 = new(
            2015, "IBC-2015",
            "ibc-2015.json",
            "ibc-2015.html"
        );
        public static readonly Standard ASCE7_05 = new(
            2005, "ASCE7-05",
            "asce7-05.json",
            "asce7-05.html"
        );
        public static readonly Standard ASCE7_10 = new(
            2010, "ASCE7-10",
            "asce7-10.json",
            "asce7-10.html"
        );
        public static readonly Standard ASCE7_16 = new(
            2016, "ASCE7-16",
            "asce7-16.json",
            "asce7-16.html"
        );

        public static readonly ImmutableList<Standard> standards = ImmutableList.Create<Standard>(
            new Standard[5]{
                UsaStandards.ASCE7_05,
                UsaStandards.ASCE7_10,
                UsaStandards.ASCE7_16,
                UsaStandards.IBC_2012,
                UsaStandards.IBC_2015,
            }
        );
    }


    public class Identified
    {
        private readonly string _id;

        protected Identified(string id)
        {
            this._id = id;
        }

        public string id
        {
            get => this._id;
        }

        override public string ToString()
        {
            return this.id;
        }
    }


    public class RiskCategory : Identified
    {
        protected RiskCategory(string id) : base(id) { }

        public static readonly RiskCategory I = new("I");
        public static readonly RiskCategory II = new("II");
        public static readonly RiskCategory III = new("III");
        public static readonly RiskCategory IV = new("IV");

        public static readonly ImmutableArray<RiskCategory> riskCategories = ImmutableArray.Create<RiskCategory>(
            new RiskCategory[4] {
                RiskCategory.I,
                RiskCategory.II,
                RiskCategory.III,
                RiskCategory.IV
            }
        );
    }

    public class SiteClass : Identified
    {
        protected SiteClass(string id) : base(id) { }

        public static readonly SiteClass A = new("A");
        public static readonly SiteClass B = new("B");
        public static readonly SiteClass C = new("C");
        public static readonly SiteClass D = new("D");
        public static readonly SiteClass E = new("E");
        public static readonly SiteClass F = new("F");

        public static readonly ImmutableArray<SiteClass> siteClasses = ImmutableArray.Create<SiteClass>(
            new SiteClass[]{
                SiteClass.A,
                SiteClass.B,
                SiteClass.C,
                SiteClass.D,
                SiteClass.E,
                SiteClass.F
            }
        );
    }
}