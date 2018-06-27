// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.AspNetCore.Blazor.Server.Bots
{
    internal class BlazorHub : Hub
    {
        private static object _callerCircuitKey = new object();

        private readonly IServiceProvider _serviceProvider;

        public BlazorHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Circuit CallerCircuit
        {
            get => (Circuit)Context.Items[_callerCircuitKey];
            set
            {
                Context.Items[_callerCircuitKey] = value;
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var startupAction = (Action<BrowserRenderer>)Context.GetHttpContext().Items["blazorstartup"];
            CallerCircuit = new Circuit(_serviceProvider, Clients.Caller, startupAction);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            CallerCircuit.Dispose();
            return base.OnDisconnectedAsync(exception);
        }

        public void BeginInvokeDotNetFromJS(string callId, string assemblyName, string methodIdentifier, string argsJson)
        {
            CallerCircuit.BeginInvokeDotNetFromJS(callId, assemblyName, methodIdentifier, argsJson);
        }
    }
}
