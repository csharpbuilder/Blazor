import * as signalR from '@aspnet/signalr';

function boot() {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl('/_blazor')
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.on('JS.BeginInvokeJS', DotNet.jsCallDispatcher.beginInvokeJSFromDotNet);

  connection.start()
    .then(() => {
      DotNet.attachDispatcher({
        beginInvokeDotNetFromJS: (callId, assemblyName, methodIdentifier, argsJson) => {
          connection.send('BeginInvokeDotNetFromJS', callId || null, assemblyName, methodIdentifier, argsJson);
        }
      });
    })
    .catch(err => console.error(err.toString()));
}

boot();
