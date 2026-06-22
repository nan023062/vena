// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Vena.Blockly
{

    [BlocklySource("行为节点/时间轴", typeof(Timeline))]
    public class TimelineSource : BehaviorNodeSource
    {
        [BlocklySourceSlot("帧率", 1)]
        public int frameRate = 30;

        [BlocklySourceSlot("组", 2)]
        public readonly GroupSource group = new GroupSource();

        public interface ISource
        {
            void Serialize(IBlocklySerializer readWriter);

            void Deserialize(IBlocklySerializer readWriter);
        }

        public interface ITrackSource : ISource
        {
            Timeline.ITrack InstantiateTrack(Timeline.Group parent);
        }

        /// <summary>
        /// 时间轴轨道配置
        /// </summary>
        public class TrackSource<TClip, TSource> : ITrackSource where TClip : ITimelineClip where TSource : ClipSource, new()
        {
            public Timeline.ITrack InstantiateTrack(Timeline.Group parent)
            {
                return new Timeline.Track<TClip, TSource>(parent);
            }

            [System.Serializable]
            public class Clip
            {
                public int frame;

                public TSource source;
            }

            /// <summary>
            /// Clip配置
            /// </summary>
            public List<Clip> clips;

            /// <summary>
            /// 信号配置
            /// </summary>
            public Dictionary<int, Signal> signals;

            public void Add(int frame, Signal signal)
            {
                signals ??= new Dictionary<int, Signal>();

                signals.Add(frame, signal);
            }

            public void AddClip(int frame, TSource input)
            {
                clips ??= new List<Clip>();

                clips.Add(new Clip()
                {
                    frame = frame,
                    source = input,
                });
            }

            public void Serialize(IBlocklySerializer readWriter)
            {
                int count = clips?.Count ?? 0;

                readWriter.WriteInt32(count);

                if (clips != null && count > 0)
                {
                    foreach (var segment in clips)
                    {
                        readWriter.WriteInt32(segment.frame);

                        segment.source.Serialize(readWriter);
                    }
                }

                int signalCount = signals?.Count ?? 0;

                readWriter.WriteInt32(signalCount);

                if (signals != null && signalCount > 0)
                {
                    foreach (var keyValue in signals)
                    {
                        readWriter.WriteInt32(keyValue.Key);

                        readWriter.WriteString(keyValue.Value.GetType().AssemblyQualifiedName);

                        keyValue.Value.Serialize(readWriter);
                    }
                }
            }

            public void Deserialize(IBlocklySerializer readWriter)
            {
                int clipCount = readWriter.ReadInt32();

                clips = null;

                if (clipCount > 0)
                {
                    clips = new List<Clip>(clipCount);

                    for (int i = 0; i < clipCount; i++)
                    {
                        Clip segment = new Clip();
                        segment.frame = readWriter.ReadInt32();
                        segment.source = new TSource();
                        segment.source.Deserialize(readWriter);

                        clips.Add(segment);
                    }
                }

                int signalCount = readWriter.ReadInt32();

                signals = null;

                if (signalCount > 0)
                {
                    signals = new Dictionary<int, Signal>(signalCount);

                    for (int i = 0; i < signalCount; i++)
                    {
                        int frame = readWriter.ReadInt32();

                        string classFullName = readWriter.ReadString();

                        Type type = Type.GetType(classFullName);
                        if (null == type) throw new Exception($"Type {classFullName} not found");

                        var signal = (Signal)Activator.CreateInstance(type);
                        signal.Deserialize(readWriter);

                        signals.Add(frame, signal);
                    }
                }
            }
        }

        /// <summary>
        /// 时间轴组配置
        /// </summary>
        public sealed class GroupSource : ISource
        {
            public readonly List<ISource> TrackList = new List<ISource>();

            public void Clear()
            {
                TrackList?.Clear();
            }

            public void Add(int frame, object input)
            {
                ISource trackData = (ISource)input;

                TrackList.Add(trackData);
            }

            public void Serialize(IBlocklySerializer readWriter)
            {
                int count = TrackList.Count;

                readWriter.WriteInt32(count);

                if (count > 0)
                {
                    foreach (var source in TrackList)
                    {
                        if (source is GroupSource groupData)
                        {
                            readWriter.WriteBoolean(true);
                            groupData.Serialize(readWriter);
                            continue;
                        }

                        readWriter.WriteBoolean(false);
                        readWriter.WriteString(source.GetType().AssemblyQualifiedName);
                        source.Serialize(readWriter);
                    }
                }
            }

            public void Deserialize(IBlocklySerializer readWriter)
            {
                int count = readWriter.ReadInt32();

                TrackList.Clear();

                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (readWriter.ReadBoolean())
                        {
                            GroupSource group = new GroupSource();

                            group.Deserialize(readWriter);

                            TrackList.Add(group);

                            continue;
                        }

                        string classFullName = readWriter.ReadString();

                        Type type = Type.GetType(classFullName);

                        if (null == type) throw new Exception($"Type {classFullName} not found");

                        ITrackSource track = (ITrackSource)Activator.CreateInstance(type);

                        track.Deserialize(readWriter);

                        TrackList.Add(track);
                    }
                }
            }
        }
    }
}
