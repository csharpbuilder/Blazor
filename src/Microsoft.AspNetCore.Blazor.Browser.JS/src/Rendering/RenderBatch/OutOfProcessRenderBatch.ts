import { RenderBatch, ArrayRange, RenderTreeDiff, ArrayValues, RenderTreeFrame, RenderTreeDiffReader, RenderTreeFrameReader, RenderTreeEditReader, ArrayRangeReader, ArraySegmentReader } from './RenderBatch';

export class OutOfProcessRenderBatch implements RenderBatch {
  constructor(batchData: any) {
  }

  updatedComponents(): ArrayRange<RenderTreeDiff> {
    throw new Error("Method not implemented.");
  }
  referenceFrames(): ArrayRange<RenderTreeFrame> {
    throw new Error("Method not implemented.");
  }
  disposedComponentIds(): ArrayRange<number> {
    throw new Error("Method not implemented.");
  }
  disposedEventHandlerIds(): ArrayRange<number> {
    throw new Error("Method not implemented.");
  }
  updatedComponentsEntry(values: ArrayValues<RenderTreeDiff>, index: number): RenderTreeDiff {
    throw new Error("Method not implemented.");
  }
  referenceFramesEntry(values: ArrayValues<RenderTreeFrame>, index: number): RenderTreeFrame {
    throw new Error("Method not implemented.");
  }
  disposedComponentIdsEntry(values: ArrayValues<number>, index: number): number {
    throw new Error("Method not implemented.");
  }
  disposedEventHandlerIdsEntry(values: ArrayValues<number>, index: number): number {
    throw new Error("Method not implemented.");
  }
  diffReader: RenderTreeDiffReader = null as any;
  editReader: RenderTreeEditReader = null as any;
  frameReader: RenderTreeFrameReader = null as any;
  arrayRangeReader: ArrayRangeReader = null as any;
  arraySegmentReader: ArraySegmentReader = null as any;
}
