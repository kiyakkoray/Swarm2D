/******************************************************************************
Copyright (c) 2015 Koray Kiyakoglu

http://www.swarm2d.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.Multiplayer
{
    public class NetworkID
    {
        private short _chunk1;
        private short _chunk2;
        private short _chunk3;
        private short _chunk4;
        private short _chunk5;
        private short _chunk6;
        private short _chunk7;
        private short _chunk8;

        private const byte LevelLiteral = 0xF; // b.0000.1111
        private const byte TypeLiteral = 0x30; // b.0011.0000

        internal enum IDType
        {
            Root,
            SessionId,
            ChildSessionIdController,
            Object
        }

        internal IDType Type { get; private set; }

        internal int Level { get; private set; }

        internal short GetIdOfLevel(int level)
        {
            short id = 0;

            if (level == 1) id = _chunk1;
            if (level == 2) id = _chunk2;
            if (level == 3) id = _chunk3;
            if (level == 4) id = _chunk4;
            if (level == 5) id = _chunk5;
            if (level == 6) id = _chunk6;
            if (level == 7) id = _chunk7;
            if (level == 8) id = _chunk8;

            return id;
        }

        private NetworkID _parentId;
        private NetworkID _parentSessionId;
        private NetworkID _parentObjectId;

        internal NetworkID ParentId
        {
            get
            {
                if (_parentId == null)
                {
                    if (Type != IDType.Root)
                    {
                        if (Level == 1)
                        {
                            _parentId = Root;
                        }
                        else
                        {
                            NetworkID networkId = new NetworkID();

                            networkId.Level = Level;

                            networkId._chunk1 = _chunk1;
                            networkId._chunk2 = _chunk2;
                            networkId._chunk3 = _chunk3;
                            networkId._chunk4 = _chunk4;
                            networkId._chunk5 = _chunk5;
                            networkId._chunk6 = _chunk6;
                            networkId._chunk7 = _chunk7;
                            networkId._chunk8 = _chunk8;

                            networkId.Id = 0;

                            networkId.Level = Level - 1;

                            IDType parentType = IDType.Root;

                            for (int i = 1; i <= networkId.Level; i++)
                            {
                                short currentId = networkId.GetIdOfLevel(i);
                                switch (parentType)
                                {
                                    case IDType.Root:
                                        {
                                            if (currentId == 0)
                                            {
                                                parentType = IDType.ChildSessionIdController;
                                            }
                                            else
                                            {
                                                parentType = IDType.Object;
                                            }
                                        }
                                        break;
                                    case IDType.SessionId:
                                        {
                                            if (currentId == 0)
                                            {
                                                parentType = IDType.ChildSessionIdController;
                                            }
                                            else
                                            {
                                                parentType = IDType.Object;
                                            }
                                        }
                                        break;
                                    case IDType.ChildSessionIdController:
                                        {
                                            parentType = IDType.SessionId;
                                        }
                                        break;
                                    case IDType.Object:
                                        {
                                            parentType = IDType.Object;
                                        }
                                        break;
                                }
                            }

                            networkId.Type = parentType;

                            _parentId = networkId;
                        }
                    }
                }

                return _parentId;
            }
            private set { _parentId = value; }
        }

        internal NetworkID ParentSessionId
        {
            get
            {
                if (_parentSessionId == null)
                {
                    NetworkID parentId = ParentId;

                    if (parentId != null)
                    {
                        if (parentId.Type == IDType.SessionId || parentId.Type == IDType.Root)
                        {
                            _parentSessionId = parentId;
                        }
                        else
                        {
                            _parentSessionId = parentId.ParentSessionId;
                        }
                    }
                }

                return _parentSessionId;
            }
            private set { _parentSessionId = value; }
        }

        internal NetworkID ParentObjectId //root or object
        {
            get
            {
                if (_parentObjectId == null)
                {
                    NetworkID parentId = ParentId;

                    if (parentId != null)
                    {
                        if (parentId.Type == IDType.Object || parentId.Type == IDType.Root)
                        {
                            _parentObjectId = parentId;
                        }
                        else
                        {
                            _parentObjectId = parentId.ParentObjectId;
                        }
                    }
                }

                return _parentObjectId;
            }
            private set { _parentObjectId = value; }
        }

        public static NetworkID Root
        {
            get
            {
                NetworkID networkId = new NetworkID();
                networkId.Type = IDType.Root;
                networkId.Level = 0;

                return networkId;
            }
        }

        public short Id
        {
            get
            {
                int level = Level;

                short id = 0;

                if (level == 1) id = _chunk1;
                if (level == 2) id = _chunk2;
                if (level == 3) id = _chunk3;
                if (level == 4) id = _chunk4;
                if (level == 5) id = _chunk5;
                if (level == 6) id = _chunk6;
                if (level == 7) id = _chunk7;
                if (level == 8) id = _chunk8;

                return id;
            }
            private set
            {
                int level = Level;

                if (level == 1) _chunk1 = value;
                if (level == 2) _chunk2 = value;
                if (level == 3) _chunk3 = value;
                if (level == 4) _chunk4 = value;
                if (level == 5) _chunk5 = value;
                if (level == 6) _chunk6 = value;
                if (level == 7) _chunk7 = value;
                if (level == 8) _chunk8 = value;
            }
        }

        public static NetworkID GenerateNewNetworkId(NetworkID parent, short id)
        {
            NetworkID networkId = new NetworkID();

            networkId.Level = parent.Level + 1;

            networkId._chunk1 = parent._chunk1;
            networkId._chunk2 = parent._chunk2;
            networkId._chunk3 = parent._chunk3;
            networkId._chunk4 = parent._chunk4;
            networkId._chunk5 = parent._chunk5;
            networkId._chunk6 = parent._chunk6;
            networkId._chunk7 = parent._chunk7;
            networkId._chunk8 = parent._chunk8;

            networkId.Id = id;

            networkId.ParentId = parent;

            switch (parent.Type)
            {
                case IDType.Root:
                    {
                        if (id == 0)
                        {
                            networkId.Type = IDType.ChildSessionIdController;
                        }
                        else
                        {
                            networkId.Type = IDType.Object;
                        }

                        networkId.ParentObjectId = parent;
                        networkId.ParentSessionId = parent;
                    }
                    break;
                case IDType.SessionId:
                    {
                        if (id == 0)
                        {
                            networkId.Type = IDType.ChildSessionIdController;
                        }
                        else
                        {
                            networkId.Type = IDType.Object;
                        }

                        networkId.ParentObjectId = parent.ParentObjectId;
                        networkId.ParentSessionId = parent;
                    }
                    break;
                case IDType.ChildSessionIdController:
                    {
                        networkId.Type = IDType.SessionId;
                        networkId.ParentObjectId = parent.ParentObjectId;
                        networkId.ParentSessionId = parent.ParentSessionId;
                    }
                    break;
                case IDType.Object:
                    {
                        networkId.Type = IDType.Object;
                        networkId.ParentObjectId = parent;
                        networkId.ParentSessionId = parent.ParentSessionId;
                    }
                    break;
            }

            return networkId;
        }

        public bool IsOwnedBy(NetworkID sessionId)
        {
            Debug.Assert(sessionId.Type == IDType.SessionId || sessionId.Type == IDType.Root, "id is not session id");

            if (this == sessionId)
            {
                return true;
            }

            return ParentSessionId == sessionId;
        }

        public void ReadFrom(IDataReader reader)
        {
            Level = reader.ReadByte();
            Type = (IDType)reader.ReadByte();
            //_options = reader.ReadByte();

            if (Level > 0) _chunk1 = reader.ReadInt16();
            if (Level > 1) _chunk2 = reader.ReadInt16();
            if (Level > 2) _chunk3 = reader.ReadInt16();
            if (Level > 3) _chunk4 = reader.ReadInt16();
            if (Level > 4) _chunk5 = reader.ReadInt16();
            if (Level > 5) _chunk6 = reader.ReadInt16();
            if (Level > 6) _chunk7 = reader.ReadInt16();
            if (Level > 7) _chunk8 = reader.ReadInt16();
        }

        public void WriteTo(IDataWriter writer)
        {
            writer.WriteByte((byte)Level);
            writer.WriteByte((byte)Type);
            //writer.WriteByte(_options);

            if (Level > 0) writer.WriteInt16(_chunk1);
            if (Level > 1) writer.WriteInt16(_chunk2);
            if (Level > 2) writer.WriteInt16(_chunk3);
            if (Level > 3) writer.WriteInt16(_chunk4);
            if (Level > 4) writer.WriteInt16(_chunk5);
            if (Level > 5) writer.WriteInt16(_chunk6);
            if (Level > 6) writer.WriteInt16(_chunk7);
            if (Level > 7) writer.WriteInt16(_chunk8);
        }

        public override string ToString()
        {
            string result = "NetworkID: " + Type + ":" + Level + "::"
                + _chunk1 + "-" + _chunk2 + "-" + _chunk3 + "-" + _chunk4 + "-"
                + _chunk5 + "-" + _chunk6 + "-" + _chunk7 + "-" + _chunk8;

            return result;
        }

        public static bool operator ==(NetworkID a, NetworkID b)
        {
            bool aIsNull = Object.ReferenceEquals(a, null);
            bool bIsNull = Object.ReferenceEquals(b, null);

            if (aIsNull && bIsNull)
            {
                return true;
            }

            if (aIsNull || bIsNull)
            {
                return false;
            }

            return a.Level == b.Level && a.Type == b.Type &&
                a._chunk1 == b._chunk1 && a._chunk2 == b._chunk2 && a._chunk3 == b._chunk3 && a._chunk4 == b._chunk4 &&
                a._chunk5 == b._chunk5 && a._chunk6 == b._chunk6 && a._chunk7 == b._chunk7 && a._chunk8 == b._chunk8;
        }

        public static bool operator !=(NetworkID a, NetworkID b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is NetworkID))
            {
                return false;
            }

            NetworkID otherId = (NetworkID)obj;

            return this == otherId;
        }

        public override int GetHashCode()
        {
            int hashCode = Options << 24;

            hashCode += _chunk1 << 21;
            hashCode += _chunk2 << 18;
            hashCode += _chunk3 << 15;
            hashCode += _chunk4 << 12;
            hashCode += _chunk5 << 9;
            hashCode += _chunk6 << 6;
            hashCode += _chunk7 << 3;
            hashCode += _chunk8 << 0;

            return hashCode;
        }

        private Byte Options
        {
            get
            {
                byte options = (byte)Level;

                options += (byte)((byte)Type << 4);

                return options;
            }
        }

        public NetworkID CreateNewIdWithNewParentId(NetworkID parent)
        {
            Debug.Assert(this.Type == IDType.Object && parent.Type == IDType.Object, "both ids must be object id");

            short currentLevelId = Id;

            return GenerateNewNetworkId(parent, currentLevelId);
        }
    }

    public static class NetworkIdHelper
    {
        public static void WriteNetworkID(this IDataWriter writer, NetworkID networkId)
        {
            networkId.WriteTo(writer);
        }

        public static NetworkID ReadNetworkID(this IDataReader reader)
        {
            NetworkID result = new NetworkID();
            result.ReadFrom(reader);

            return result;
        }
    }
}
