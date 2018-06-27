// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.AspNetCore.Builder
{
    internal class BlazorHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("MyMethod", "Hello");
            await base.OnConnectedAsync();
        }
    }
}
