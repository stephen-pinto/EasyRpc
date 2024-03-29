﻿using EasyRpc.Master.PeerBase;

namespace EasyRpc.Master.PeerManagement
{
    public class DefaultPeerMappingCriteria : IPeerMappingCriteria
    {
        private readonly IPeerRegistry _registry;

        public DefaultPeerMappingCriteria(IPeerRegistry registry)
        {
            _registry = registry;
        }

        public bool TryGetMatchingPeer(PeerInfo sourcePeer, out PeerInfo matchedPeer)
        {
            ReadOnlySpan<PeerRegistryEntry> peers = new ReadOnlySpan<PeerRegistryEntry>(_registry.Values.ToArray());
            foreach (var item in peers)
            {
                if (item.Peer.Type == sourcePeer.Type)
                {
                    matchedPeer = item.Peer;
                    return true;
                }
            }

            matchedPeer = null!;
            return false;
        }
    }
}
