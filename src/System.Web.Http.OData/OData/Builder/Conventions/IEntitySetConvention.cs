﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Builder.Conventions
{
    public interface IEntitySetConvention : IConvention
    {
        void Apply(IEntitySetConfiguration configuration, ODataModelBuilder model);
    }
}
