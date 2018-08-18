using System;
using System.Threading;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace ChatParser
{
    public class SaltChatParser
    {
        public bool Exit = false;

        public event EventHandler Connected;
        public event EventHandler<WaifuMessageEventArgs> WaifuMessage;

        TwitchClient _client;

        public SaltChatParser(string userName, string accessToken)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(userName, accessToken);

            _client = new TwitchClient();
            _client.Initialize(credentials, "SaltyBet");

            _client.OnMessageReceived += OnMessageReceived;
            _client.OnConnected += OnConnected;
        }

        public  void Run()
        {
            _client.Connect();

            while (!Exit)
            {

                Thread.Sleep(500);
            }            
        }

        public void OnConnected(object sender, OnConnectedArgs e)
        {
            OnConnected(new EventArgs());
        }

        public void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Username.ToLower() == "WAIFU4u".ToLower())
            {
                OnWaifuMessage(new WaifuMessageEventArgs {Message = e.ChatMessage.Message});
            }
        }

        protected virtual void OnConnected(EventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        protected virtual void OnWaifuMessage(WaifuMessageEventArgs e)
        {
            WaifuMessage?.Invoke(this, e);
        }
    }
}
