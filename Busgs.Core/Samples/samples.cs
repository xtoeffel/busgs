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
using System.Linq;
using Parameters;
using SeismicStandards;

namespace Samples
{
    public static class SampleRequestParameters
    {
        public static ImmutableList<RequestParameters> GetSamples()
        {
            double latitude = 34.987;
            double longitude = -118.2657;
            RiskCategory riskCategory = RiskCategory.III;
            SiteClass siteClass = SiteClass.D;

            return SeismicStandards.UsaStandards.standards
                .Select(
                    refDoc => new RequestParameters(
                        refDoc: refDoc,
                        lat: latitude,
                        lng: longitude,
                        riskCat: riskCategory,
                        siteClass: siteClass,
                        title: $"Example {refDoc.Name}"
                    )
                )
                .ToImmutableList();
        }
    }

}