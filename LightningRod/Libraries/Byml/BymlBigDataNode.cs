﻿using System.ComponentModel.DataAnnotations.Schema;

namespace LightningRod.Libraries.Byml
{
    public class BymlBigDataNode<T> : IBymlNode, IBymlValueNode
    {
        public BymlNodeId Id { get; }
        public T Data { get; set; }

        object IBymlValueNode.GetValue() => Data!;

        public BymlBigDataNode(BymlNodeId id, BinaryReader reader, Func<BinaryReader, T> valueReader)
        {
            Id = id;
            using (reader.BaseStream.TemporarySeek(reader.ReadUInt32(), SeekOrigin.Begin))
            {
                Data = valueReader(reader);
            }
        }

        public BymlBigDataNode(BymlNodeId id, T data)
        {
            Id = id;
            Data = data;
        }

        public void SetData(T data)
        {
            Data = data;
        }
    }
}
