﻿using EasyRpc.Core.Client;
using EasyRpc.Core.Util;
using EasyRpc.Master;
using Grpc.Core;

namespace EasyRpc.Peer.Net
{
    public class PeerNetServices
    {
        private Server? _server;
        private IEasyRpcClient? _peerClient;

        public IPeerClient PeerClient => _peerClient!;

        public async Task Start(
            string masterAddress,
            string masterPeerAddress,
            string myAddress,
            MakeRequestDelegate makeRequestHandler,
            NotifyDelegate notifyHandler)
        {
            ICertificateProvider certificateProvider = new DefaultClientCertificateProvider();

            //TODO: Move the masterPeerAddress to fetch from registration response
            _peerClient = new PeerNetClient(masterAddress, masterPeerAddress);
            var response = await _peerClient.Register(new RegistrationRequest
            {
                Address = masterPeerAddress,
                Name = "Peer1",
                Type = "Grpc",
                Properties = { { "OS", "Windows" }, { "Version", "10" } },
            }).ConfigureAwait(false);

            _listener = new Server
            {
                Services = { PeerService.BindService(new PeerNetService(makeRequestHandler, notifyHandler)) },
                Ports = { new ServerPort("localhost", 50055, GrpcChannelSecurityHelper.GetSecureServerCredentials(certificateProvider)) }
            };
            _listener.Start();
        }

        public void Stop()
        {
            var task1 = _peerClient!.Unregister(new RegistrationRequest() { Name = "Peer1", Type = "Grpc" });
            var task2 = _listener?.ShutdownAsync();
            Task.WaitAll([task1!, task2!]);
        }
    }
}
