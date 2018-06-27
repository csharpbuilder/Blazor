// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.IO;

namespace Microsoft.AspNetCore.Blazor.Server.Bots
{
    internal static class RenderBatchBinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, in RenderBatch renderBatch)
        {
            var pos = 0;

            var updatedComponentsStartIndex = pos;
            var referenceFramesStartIndex = updatedComponentsStartIndex + BinaryLength(renderBatch.UpdatedComponents);
            var disposedComponentIdsStartIndex = referenceFramesStartIndex + BinaryLength(renderBatch.ReferenceFrames);
            var disposedEventHandlerIdsStartIndex = disposedComponentIdsStartIndex + BinaryLength(renderBatch.DisposedComponentIDs);

            writer.Write(updatedComponentsStartIndex);
            writer.Write(referenceFramesStartIndex);
            writer.Write(disposedComponentIdsStartIndex);
            writer.Write(disposedEventHandlerIdsStartIndex);
            pos += 16;

            Write(ref pos, writer, renderBatch.UpdatedComponents);
        }

        private static int BinaryLength(ArrayRange<RenderTreeDiff> diffs)
        {
            var result = 8; // prefixed by array ptr, count
            for (var diffIndex = 0; diffIndex < diffs.Count; diffIndex++)
            {
                result += BinaryLength(diffs.Array[diffIndex]);
            }
            return result;
        }

        private static int BinaryLength(ArrayRange<RenderTreeFrame> frames)
            => 8 + 28 * frames.Count; // prefixed by array ptr, count, then 28-byte frames

        private static int BinaryLength(ArraySegment<RenderTreeEdit> edits)
            => 12 + edits.Count * 16; // prefixed by array ptr, offset, count, then 16-byte structs

        private static int BinaryLength(RenderTreeDiff diff)
            => 4 + BinaryLength(diff.Edits);

        private static int BinaryLength(ArrayRange<int> ints)
            => 4 * (1 + ints.Count); // length-prefixed, then one int per entry

        public static void Write(ref int pos, BinaryWriter writer, ArrayRange<RenderTreeDiff> diffs)
        {
            WriteHeader(ref pos, writer, diffs);
            for (var i = 0; i < diffs.Count; i++)
            {
                Write(ref pos, writer, ref diffs.Array[i]);
            }
        }

        public static void Write(ref int pos, BinaryWriter writer, ref RenderTreeDiff diff)
        {
            writer.Write(diff.ComponentId);
            pos += 4;

            WriteHeader(ref pos, writer, diff.Edits);
            var offset = diff.Edits.Offset;
            for (var i = 0; i < diff.Edits.Count; i++)
            {
                Write(ref pos, writer, ref diff.Edits.Array[offset + i]);
            }
        }

        public static void Write(ref int pos, BinaryWriter writer, ref RenderTreeEdit edit)
        {
            throw new NotImplementedException();
        }

        private static void WriteHeader<T>(ref int pos, BinaryWriter writer, ArrayRange<T> arrayRange)
        {
            pos += 8;
            writer.Write(pos); // Array contents pointer
            writer.Write(arrayRange.Count);
        }

        private static void WriteHeader<T>(ref int pos, BinaryWriter writer, ArraySegment<T> arraySegment)
        {
            pos += 12;
            writer.Write(pos); // Array contents pointer
            writer.Write(0); // Offset
            writer.Write(arraySegment.Count);
        }
    }
}
