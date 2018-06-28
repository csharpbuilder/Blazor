import { Platform } from './Platform/Platform';
import { RenderBatchFactory } from './Rendering/RenderBatch/RenderBatch';

// These variables are readable anywhere else in the system, acting as a
// simple servicelocator-style DI mechanism
export let platform: Platform;
export let renderBatchFactory: RenderBatchFactory;

export function configure(_platform: Platform, _renderBatchFactory: RenderBatchFactory) {
  platform = _platform;
  renderBatchFactory = _renderBatchFactory;
}
