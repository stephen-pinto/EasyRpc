﻿using Microsoft.AspNetCore.SignalR.Client;

namespace SigRTestClient
{
    public interface IEasyRpcSignalRHub
    {
        Task Register(RegistrationRequestSigr request);
        Task SendRegisterResponse(RegistrationResponseSigr response);
        Task Unregister(RegistrationRequestSigr request);
        Task SendUnregisterResponse(RegistrationResponseSigr response);
        Task MakeRequest(MessageSigr message);
        Task SendMakeRequestResponse(MessageSigr message);
        Task Notify(MessageSigr message);
    }

    internal class TestClient
    {
        private string _selfAddress = string.Empty;
        private string _type = "SignalRClient";
        private string _peerAddress = string.Empty;
        private RegistrationRequestSigr? _registration;

        internal void Run()
        {
            Thread.Sleep(2000);

            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:55155/peer", opts =>
                {
                    opts.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                        {
                            // clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                            clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                        }
                        return handler;
                    };
                })
                .Build();

            connection.StartAsync();

            //Handle registration
            connection.On<RegistrationResponseSigr>(nameof(IEasyRpcSignalRHub.SendRegisterResponse), (message) =>
            {
                Console.WriteLine($"[HUB]: {message}");
                _selfAddress = message.RegistrationId;
            });

            _registration = new RegistrationRequestSigr("", _type, "SigRClient1");
            connection.InvokeAsync(nameof(IEasyRpcSignalRHub.Register), _registration).GetAwaiter().GetResult();

            //Handle requests
            connection.On<MessageSigr>(nameof(IEasyRpcSignalRHub.MakeRequest), (message) =>
            {
                Console.WriteLine($"[HUB]: {message}");
                _peerAddress = message.From;
                
                Task.Run(() =>
                {
                    connection.InvokeAsync(nameof(IEasyRpcSignalRHub.SendMakeRequestResponse),
                    new MessageSigr(_peerAddress, _selfAddress,
                    message.Id, MessageTypeSigr.Response,
                    "This is some data from SigR Client/Peer for Request: " + message.Data));
                });
            });

            //Handle close and reconnect
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            Console.WriteLine("Press any key to send notification...");
            Console.ReadKey();

            connection.InvokeAsync(nameof(IEasyRpcSignalRHub.Notify),
                    new MessageSigr(_peerAddress, _selfAddress,
                    Guid.NewGuid().ToString(), MessageTypeSigr.Response,
                    "Notification from SigR Client/Peer"));

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            connection.InvokeAsync(nameof(IEasyRpcSignalRHub.Unregister), _registration);
            Console.WriteLine("Unregistered...");
        }
    }
}
