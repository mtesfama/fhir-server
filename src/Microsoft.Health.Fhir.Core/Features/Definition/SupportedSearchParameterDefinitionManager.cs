﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Fhir.Core.Features.Search;
using Microsoft.Health.Fhir.Core.Models;

namespace Microsoft.Health.Fhir.Core.Features.Definition
{
    /// <summary>
    /// A SearchParameterDefinitionManager that only returns actively searchable parameters.
    /// </summary>
    public class SupportedSearchParameterDefinitionManager : ISupportedSearchParameterDefinitionManager
    {
        private readonly ISearchParameterDefinitionManager _inner;

        public SupportedSearchParameterDefinitionManager(ISearchParameterDefinitionManager inner)
        {
            EnsureArg.IsNotNull(inner, nameof(inner));

            _inner = inner;
        }

        public IEnumerable<SearchParameterInfo> AllSearchParameters => _inner.AllSearchParameters.Where(x => x.IsSupported);

        public IReadOnlyDictionary<string, string> SearchParameterHashMap => _inner.SearchParameterHashMap;

        public IEnumerable<SearchParameterInfo> GetSearchParameters(string resourceType)
        {
            return _inner.GetSearchParameters(resourceType)
                .Where(x => x.IsSupported);
        }

        public bool TryGetSearchParameter(string resourceType, string name, out SearchParameterInfo searchParameter)
        {
            searchParameter = null;
            if (_inner.TryGetSearchParameter(resourceType, name, out var parameter) && parameter.IsSupported)
            {
                searchParameter = parameter;

                return true;
            }

            return false;
        }

        public SearchParameterInfo GetSearchParameter(string resourceType, string name)
        {
            SearchParameterInfo parameter = _inner.GetSearchParameter(resourceType, name);
            if (parameter.IsSupported)
            {
                return parameter;
            }

            throw new SearchParameterNotSupportedException(resourceType, name);
        }

        public SearchParameterInfo GetSearchParameter(Uri definitionUri)
        {
            SearchParameterInfo parameter = _inner.GetSearchParameter(definitionUri);
            if (parameter.IsSupported)
            {
                return parameter;
            }

            throw new SearchParameterNotSupportedException(definitionUri);
        }

        public IEnumerable<SearchParameterInfo> GetSupportedButNotSearchableParams()
        {
            return _inner.AllSearchParameters.Where(p => p.IsSearchable == false && p.IsSupported == true);
        }

        public void SetSearchParameterEnabled(Uri searchParameterUri)
        {
            var supportedParameter = GetSearchParameter(searchParameterUri);
            supportedParameter.IsSearchable = true;
        }

        public void UpdateSearchParameterHashMap(Dictionary<string, string> updatedSearchParamHashMap)
        {
            _inner.UpdateSearchParameterHashMap(updatedSearchParamHashMap);
        }

        public string GetSearchParameterHashForResourceType(string resourceType)
        {
            return _inner.GetSearchParameterHashForResourceType(resourceType);
        }
    }
}
