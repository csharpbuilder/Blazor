// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Blazor.Rendering
{
    /// <summary>
    /// Describes a set of UI changes.
    /// </summary>
    public readonly struct RenderBatch
    {
        /// <summary>
        /// Gets the changes to components that were added or updated.
        /// </summary>
        public ArrayRange<RenderTreeDiff> UpdatedComponents { get; }

        /// <summary>
        /// Gets render frames that may be referenced by entries in <see cref="UpdatedComponents"/>.
        /// For example, edit entries of type <see cref="RenderTreeEditType.PrependFrame"/>
        /// will point to an entry in this array to specify the subtree to be prepended.
        /// </summary>
        [JsonConverter(typeof(TemporaryReferenceFramesJsonConverter))]
        public ArrayRange<RenderTreeFrame> ReferenceFrames { get; }

        /// <summary>
        /// Gets the IDs of the components that were disposed.
        /// </summary>
        public ArrayRange<int> DisposedComponentIDs { get; }

        /// <summary>
        /// Gets the IDs of the event handlers that were disposed.
        /// </summary>
        public ArrayRange<int> DisposedEventHandlerIDs { get; }

        internal RenderBatch(
            ArrayRange<RenderTreeDiff> updatedComponents,
            ArrayRange<RenderTreeFrame> referenceFrames,
            ArrayRange<int> disposedComponentIDs,
            ArrayRange<int> disposedEventHandlerIDs)
        {
            UpdatedComponents = updatedComponents;
            ReferenceFrames = referenceFrames;
            DisposedComponentIDs = disposedComponentIDs;
            DisposedEventHandlerIDs = disposedEventHandlerIDs;
        }
    }

    internal class TemporaryReferenceFramesJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => objectType == typeof(ArrayRange<RenderTreeFrame>);

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var frames = (ArrayRange<RenderTreeFrame>)value;
            writer.WriteStartArray();

            foreach (var frame in frames)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("frameType");
                writer.WriteValue((int)frame.FrameType);

                switch (frame.FrameType)
                {
                    case RenderTreeFrameType.Element:
                        writer.WritePropertyName("subtreeLength");
                        writer.WriteValue(frame.ElementSubtreeLength);

                        writer.WritePropertyName("elementName");
                        writer.WriteValue(frame.ElementName);
                        break;

                    case RenderTreeFrameType.Text:
                        writer.WritePropertyName("textContent");
                        writer.WriteValue(frame.TextContent);
                        break;

                    case RenderTreeFrameType.Attribute:
                        writer.WritePropertyName("attributeEventHandlerId");
                        writer.WriteValue(frame.AttributeEventHandlerId);

                        writer.WritePropertyName("attributeName");
                        writer.WriteValue(frame.AttributeName);

                        writer.WritePropertyName("attributeValue");
                        writer.WriteValue(frame.AttributeValue as string);
                        break;

                    case RenderTreeFrameType.Component:
                        writer.WritePropertyName("subtreeLength");
                        writer.WriteValue(frame.ComponentSubtreeLength);

                        writer.WritePropertyName("componentId");
                        writer.WriteValue(frame.ComponentId);
                        break;

                    case RenderTreeFrameType.Region:
                        writer.WritePropertyName("subtreeLength");
                        writer.WriteValue(frame.RegionSubtreeLength);
                        break;

                    case RenderTreeFrameType.ElementReferenceCapture:
                        writer.WritePropertyName("elementReferenceCaptureId");
                        writer.WriteValue(frame.ElementReferenceCaptureId);
                        break;

                    case RenderTreeFrameType.ComponentReferenceCapture:
                        writer.WritePropertyName("componentReferenceCapture");
                        writer.WriteValue(frame.ComponentReferenceCaptureParentFrameIndex);
                        break;
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }
}
