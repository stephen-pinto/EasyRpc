﻿using EasyRpc.Core.Client;

namespace EasyRpc.Master.PeerManagement
{
    public interface IPeerMappingCriteria
    {
        bool TryGetMatchingPeer(PeerInfo sourcePeer, out PeerInfo matchedPeer);
    }
}
