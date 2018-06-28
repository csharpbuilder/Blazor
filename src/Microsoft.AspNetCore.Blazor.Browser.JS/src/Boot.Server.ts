import '../../Microsoft.JSInterop/JavaScriptRuntime/src/Microsoft.JSInterop';
import './GlobalExports';
import * as Environment from './Environment';
import * as signalR from '@aspnet/signalr';
import { OutOfProcessRenderBatch } from './Rendering/RenderBatch/OutOfProcessRenderBatch';

function boot() {
  Environment.configure(
    /* platform */ null as any, // TODO: Figure out if 'platform' ever should apply to out-of-proc scenarios
    batchData => new OutOfProcessRenderBatch(batchData)
  );

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
