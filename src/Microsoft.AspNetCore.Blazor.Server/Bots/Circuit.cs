// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.JSInterop;
using System;

namespace Microsoft.AspNetCore.Blazor.Server.Bots
{
    internal class Circuit : IDisposable
    {
        private readonly BrowserRenderer _renderer;
        private readonly IJSRuntime _jsRuntime;

        public Circuit(IServiceProvider serviceProvider, IClientProxy clientProxy, Action<BrowserRenderer> startupAction)
        {
            if (clientProxy == null)
            {
                throw new ArgumentNullException(nameof(clientProxy));
            }

            _jsRuntime = new RemoteJSRuntime(clientProxy);
            JSRuntime.SetCurrentJSRuntime(_jsRuntime);

            _renderer = new BrowserRenderer(serviceProvider);
            startupAction(_renderer);
        }

        public void BeginInvokeDotNetFromJS(string callId, string assemblyName, string methodIdentifier, string argsJson)
        {
            JSRuntime.SetCurrentJSRuntime(_jsRuntime);
            DotNetDispatcher.BeginInvoke(callId, assemblyName, methodIdentifier, argsJson);
        }

        public void Dispose()
        {
        }
    }
}
