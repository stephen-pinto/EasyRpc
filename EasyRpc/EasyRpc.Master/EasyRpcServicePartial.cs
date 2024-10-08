﻿using EasyRpc.Core.Util;
using EasyRpc.Peer;
using Grpc.Core;

namespace EasyRpc.Master
{
    public partial class EasyRpcService
    {
        private readonly string _serviceHost;
        private readonly int _port;
        private Server? _masterServer;
        private Server? _peerServer;
        private string Address => $"https://{_serviceHost}:{_port}";

        public void Start()
        {
            SetupMasterServer();
            SetupPeerServer();
            foreach (var plugin in _plugins)
                plugin.Load();
        }

        public void Stop()
        {
            Task.WaitAll(_masterServer!.ShutdownAsync(), _peerServer!.ShutdownAsync());
            foreach (var plugin in _plugins)
                plugin.Unload();
        }

        private void SetupMasterServer()
        {
            _masterServer = new Server
            {
                Services = { MasterService.BindService(new EasyRpcMasterService(Register, Unregister)) },
                Ports = {
                    new ServerPort(_serviceHost,
                        _port,
                        GrpcChannelSecurityHelper.GetSecureServerCredentials(_serverCertificateProvider))
                    }
            };

            _masterServer.Start();
            System.Diagnostics.Debug.WriteLine($"Master server listening on port {_port}");
        }

        private void SetupPeerServer()
        {
            _peerServer = new Server
            {
                Services = { PeerService.BindService(new EasyRpcPeerService(MakeRequest, Notify)) },
                Ports = { new ServerPort(_serviceHost,
                    _port + 1,
                    GrpcChannelSecurityHelper.GetSecureServerCredentials(_serverCertificateProvider))
                }
            };

            _peerServer.Start();
            System.Diagnostics.Debug.WriteLine($"Peer server listening on port {_port + 1}");
        }
    }
}
