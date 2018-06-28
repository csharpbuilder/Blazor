import '../../Microsoft.JSInterop/JavaScriptRuntime/src/Microsoft.JSInterop';
import './GlobalExports';
import * as Environment from './Environment';
import * as signalR from '@aspnet/signalr';
import { OutOfProcessRenderBatch } from './Rendering/RenderBatch/OutOfProcessRenderBatch';
import { internalFunctions as uriHelperFunctions } from './Services/UriHelper';
import { renderBatch } from './Rendering/Renderer';

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

  // TODO: Since we could convert the batchData to an OutOfProcessRenderBatch here,
  // remove renderBatchFactory from the Environment module.
  connection.on('JS.RenderBatch', (browserRendererId, batchData) => {
    renderBatch(browserRendererId, batchData);
  });

  connection.start()
    .then(() => {
      DotNet.attachDispatcher({
        beginInvokeDotNetFromJS: (callId, assemblyName, methodIdentifier, argsJson) => {
          connection.send('BeginInvokeDotNetFromJS', callId || null, assemblyName, methodIdentifier, argsJson);
        }
      });

      connection.send(
        'StartCircuit',
        uriHelperFunctions.getLocationHref(),
        uriHelperFunctions.getBaseURI()
      );
    })
    .catch(err => console.error(err.toString()));
}

boot();
