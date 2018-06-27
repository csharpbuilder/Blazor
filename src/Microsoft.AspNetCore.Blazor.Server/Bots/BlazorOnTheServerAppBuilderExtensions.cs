// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.Server.Bots;
using System;

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
        public static IApplicationBuilder UseBlazorOnTheServer(
            this IApplicationBuilder builder,
            Action<BrowserRenderer> startupAction)
        {
            var endpoint = "/_blazor";

            // I strongly expect this is not the preferred way to pass info
            // into the SignalR hub, but this is all temporary
            builder.Use((context, next) =>
            {
                if (context.Request.Path.StartsWithSegments(endpoint))
                {
                    context.Items["blazorstartup"] = startupAction;
                }

                return next();
            });

            builder.UseSignalR(route => route.MapHub<BlazorHub>(endpoint));

            return builder;
        }
    }
}
