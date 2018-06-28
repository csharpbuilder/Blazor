import { RenderBatch, ArrayRange, RenderTreeDiff, ArrayValues, RenderTreeFrame, RenderTreeDiffReader, RenderTreeFrameReader, RenderTreeEditReader, ArrayRangeReader, ArraySegmentReader } from './RenderBatch';

export class OutOfProcessRenderBatch implements RenderBatch {
  constructor(private batchData: any) {
  }

  updatedComponents(): ArrayRange<RenderTreeDiff> {
    return this.batchData.updatedComponents;
  }

  referenceFrames(): ArrayRange<RenderTreeFrame> {
    return this.batchData.referenceFrames;
  }

  disposedComponentIds(): ArrayRange<number> {
    return this.batchData.disposedComponentIDs;
  }

  disposedEventHandlerIds(): ArrayRange<number> {
    return this.batchData.disposedEventHandlerIDs;
  }

  updatedComponentsEntry(values: ArrayValues<RenderTreeDiff>, index: number): RenderTreeDiff {
    return (values as any)[index];
  }

  referenceFramesEntry(values: ArrayValues<RenderTreeFrame>, index: number): RenderTreeFrame {
    return (values as any)[index];
  }

  disposedComponentIdsEntry(values: ArrayValues<number>, index: number): number {
    return (values as any)[index];
  }

  disposedEventHandlerIdsEntry(values: ArrayValues<number>, index: number): number {
    return (values as any)[index];
  }

  diffReader = diffReader;
  editReader = editReader;
  frameReader = frameReader;
  arrayRangeReader = arrayRangeReader;
  arraySegmentReader = arraySegmentReader;
}

const diffReader: RenderTreeDiffReader = {
  componentId: diff => (diff as any).componentId,
  edits: diff => (diff as any).edits,
  editsEntry: (values, index) => (values as any)[index]
};

const editReader: RenderTreeEditReader = {
  editType: edit => (edit as any).type,
  newTreeIndex: edit => (edit as any).referenceFrameIndex,
  siblingIndex: edit => (edit as any).siblingIndex,
  removedAttributeName: edit => (edit as any).removedAttributeName,
};

const frameReader: RenderTreeFrameReader = {
  attributeEventHandlerId: frame => (frame as any).attributeEventHandlerId,
  attributeName: frame => (frame as any).attributeName,
  attributeValue: frame => (frame as any).attributeValue,
  componentId: frame => (frame as any).componentId,
  elementName: frame => (frame as any).elementName,
  elementReferenceCaptureId: frame => (frame as any).elementReferenceCaptureId,
  frameType: frame => (frame as any).frameType,
  subtreeLength: frame => (frame as any).subtreeLength,
  textContent: frame => (frame as any).textContent,
};

const arrayRangeReader: ArrayRangeReader = {
  count: arrayRange => (arrayRange as any).length,
  values: arrayRange => arrayRange as any
};

const arraySegmentReader: ArraySegmentReader = {
  count: arraySegment => (arraySegment as any).length,
  offset: arraySegment => 0, // Is this right?
  values: arraySegment => arraySegment as any
};
