﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace LightningRod.Libraries.Byml
{
    public class BymlHashTable : IBymlNode
    {
        public BymlNodeId Id => BymlNodeId.Hash;

        public List<BymlHashPair> Pairs;

        public IBymlNode this[string key]
        {
            get
            {
                if (!TryGetValue(key, out var value))
                    throw new KeyNotFoundException("Couldn't find key " + key);
                return value;
            }
        }

        public void SetNode(string key, IBymlNode value)
        {
            var idx = Utils.BinarySearch(Pairs, key);

            if (idx < 0)
            {
                return;
            }


            BymlHashPair pair = Pairs[idx];
            pair.Value = value;
        }

        public bool ContainsKey(string key) => TryGetValue(key, out _);

        public bool TryGetValue(string key, [NotNullWhen(true)] out IBymlNode? value)
        {
            value = null;

            var idx = Utils.BinarySearch(Pairs, key);
            if (idx < 0)
                return false;

            value = Pairs[idx].Value;
            return true;
        }

        public KeyView Keys => new(this);
        public ValueView Values => new(this);

        public BymlHashTable(Byml by, Stream stream)
        {
            long position = stream.Position;
            BinaryReader reader = new(stream);
            uint count = reader.ReadUInt24();

            Pairs = new List<BymlHashPair>();
            for (int i = 0; i < count; i++)
            {
                var name = reader.ReadUInt24();
                var id = Byml.ReadNodeId(stream);
                if (!id.HasValue)
                    throw new InvalidDataException($"Invalid BYML node ID!");

                BymlHashPair entry = new()
                {
                    Id = id.Value,
                    Name = by.GetFromHashKeyTable(name),
                    Value = by.ReadNode(reader, id.Value),
                };

                Pairs.Add(entry);
            }
        }

        public BymlHashTable()
        {
            Pairs = new List<BymlHashPair>();
        }

        public void AddNode(BymlNodeId type, IBymlNode node, string nodeName)
        {
            BymlHashPair pair = new()
            {
                Id = type,
                Name = nodeName,
                Value = node,
            };

            Pairs.Add(pair);
        }

        public readonly struct KeyView : IReadOnlyList<string>
        {
            readonly BymlHashTable _this;

            public KeyView(BymlHashTable @this)
            {
                _this = @this;
            }

            public readonly string this[int index] => _this.Pairs[index].Name;

            public readonly int Count => _this.Pairs.Count;

            public readonly IEnumerable<string> AsEnumerable()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            public readonly IEnumerator<string> GetEnumerator() => AsEnumerable().GetEnumerator();

            readonly IEnumerator IEnumerable.GetEnumerator() => AsEnumerable().GetEnumerator();
        }

        public readonly struct ValueView : IReadOnlyList<IBymlNode>
        {
            private readonly BymlHashTable _this;

            public ValueView(BymlHashTable @this)
            {
                _this = @this;
            }

            public readonly IBymlNode this[int index] => _this.Pairs[index].Value;

            public readonly int Count => _this.Pairs.Count;

            public readonly IEnumerable<IBymlNode> AsEnumerable()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            public readonly IEnumerator<IBymlNode> GetEnumerator() => AsEnumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => AsEnumerable().GetEnumerator();
        }
    }
}
