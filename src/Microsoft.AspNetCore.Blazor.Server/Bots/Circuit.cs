// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;

namespace Microsoft.AspNetCore.Blazor.Server.Bots
{
    internal class Circuit : IDisposable
    {
        private readonly BrowserRenderer _renderer;
        private readonly IJSRuntime _jsRuntime;

        public Circuit(IClientProxy clientProxy, Action<BrowserRenderer> startupAction, string uriAbsolute, string baseUriAbsolute)
        {
            if (clientProxy == null)
            {
                throw new ArgumentNullException(nameof(clientProxy));
            }

            _jsRuntime = new RemoteJSRuntime(clientProxy);
            JSRuntime.SetCurrentJSRuntime(_jsRuntime);

            // TODO: Consolidate the service providers
            var perCircuitServiceCollection = new ServiceCollection();
            var uriHelper = new RemoteUriHelper(_jsRuntime, uriAbsolute, baseUriAbsolute);
            perCircuitServiceCollection.AddSingleton<IUriHelper>(uriHelper);

            _renderer = new BrowserRenderer(perCircuitServiceCollection.BuildServiceProvider());
            _renderer.OnException += (sender, exception) =>
            {
                // TODO: Somehow dispatch this back to the SignalR hub to make the client's
                // connection abort and log the exception
                Console.Error.WriteLine(exception.ToString());
            };

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
