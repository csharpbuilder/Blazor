// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.SignalR;
using System;

namespace Microsoft.AspNetCore.Blazor.Server.Bots
{
    internal class Circuit : IDisposable
    {
        private readonly IClientProxy _clientProxy;
        private readonly BrowserRenderer _renderer;

        public Circuit(IServiceProvider serviceProvider, IClientProxy clientProxy, Action<BrowserRenderer> startupAction)
        {
            _clientProxy = clientProxy ?? throw new ArgumentNullException(nameof(clientProxy));
            _renderer = new BrowserRenderer(serviceProvider);
            startupAction(_renderer);
        }

        public void Dispose()
        {
        }
    }
}
