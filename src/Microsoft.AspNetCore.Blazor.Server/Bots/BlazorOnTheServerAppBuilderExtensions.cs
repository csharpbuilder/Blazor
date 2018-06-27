// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Temporary vague approximation to server-side execution needed so I can
    /// build the rest of the interop.
    /// </summary>
    public static class BlazorOnTheServerAppBuilderExtensions
    {
        /// <summary>
        /// Temporary vague approximation to server-side execution needed so I can
        /// build the rest of the interop.
        /// </summary>
        public static IApplicationBuilder UseBlazorOnTheServer<TStartup>(this IApplicationBuilder builder)
        {
            builder.UseSignalR(route =>
            {
                route.MapHub<BlazorHub>("/_blazor");
            });

            return builder;
        }
    }
}
