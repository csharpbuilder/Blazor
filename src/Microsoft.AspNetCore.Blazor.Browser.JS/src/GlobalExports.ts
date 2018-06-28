import { platform } from './Environment';
import { navigateTo, internalFunctions as uriHelperInternalFunctions } from './Services/UriHelper';
import { internalFunctions as httpInternalFunctions } from './Services/Http';
import { attachRootComponentToElement, renderBatch } from './Rendering/Renderer';
import { Pointer } from './Platform/Platform';
import { RenderBatch } from './Rendering/RenderBatch/RenderBatch';

// Make the following APIs available in global scope for invocation from JS
window['Blazor'] = {
  platform,
  navigateTo,

  _internal: {
    attachRootComponentToElement,
    renderBatch,
    http: httpInternalFunctions,
    uriHelper: uriHelperInternalFunctions
  }
};
