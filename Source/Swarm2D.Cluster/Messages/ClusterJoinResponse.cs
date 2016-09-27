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

using Swarm2D.Engine.Logic;
using Swarm2D.Engine.Multiplayer;
using Swarm2D.Library;

namespace Swarm2D.Cluster
{
    class ClusterJoinResponse : ResponseData
    {
        public NetworkID ClusterObjectManagerId { get; private set; }
        public NetworkID RootClusterObjectNetworkId { get; private set; }
        public ClusterNodeInformation[] ClusterNodeInformations { get; private set; }

        public ClusterJoinResponse()
        {

        }

        public ClusterJoinResponse(NetworkID clusterObjectManagerId, NetworkID rootClusterObjectNetworkId, ClusterNodeInformation[] clusterNodeInformations)
        {
            ClusterObjectManagerId = clusterObjectManagerId;
            RootClusterObjectNetworkId = rootClusterObjectNetworkId;
            ClusterNodeInformations = clusterNodeInformations;
        }

        protected override void Serialize(IDataWriter writer)
        {
            writer.WriteNetworkID(ClusterObjectManagerId);
            writer.WriteNetworkID(RootClusterObjectNetworkId);

            writer.WriteInt32(ClusterNodeInformations.Length);

            foreach (var clusterNodeInformation in ClusterNodeInformations)
            {
                writer.WriteUnicodeString(clusterNodeInformation.Address);
                writer.WriteInt32(clusterNodeInformation.Port);
                writer.WriteNetworkID(clusterNodeInformation.Id);
            }
        }

        protected override void Deserialize(IDataReader reader)
        {
            ClusterObjectManagerId = reader.ReadNetworkID();
            RootClusterObjectNetworkId = reader.ReadNetworkID();

            int length = reader.ReadInt32();

            ClusterNodeInformations = new ClusterNodeInformation[length];

            for (int i = 0; i < length; i++)
            {
                var clusterNodeInformation = new ClusterNodeInformation();

                clusterNodeInformation.Address = reader.ReadUnicodeString();
                clusterNodeInformation.Port = reader.ReadInt32();
                clusterNodeInformation.Id = reader.ReadNetworkID();

                ClusterNodeInformations[i] = clusterNodeInformation;
            }
        }
    }
}