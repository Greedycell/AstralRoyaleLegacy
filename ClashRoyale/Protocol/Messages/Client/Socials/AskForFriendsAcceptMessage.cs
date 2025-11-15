using System;
using ClashRoyale.Logic;
using DotNetty.Buffers;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;

namespace ClashRoyale.Protocol.Messages.Client.Socials
{
    public class AskForFriendsAcceptMessage : PiranhaMessage
    {
        public AskForFriendsAcceptMessage(Device device, IByteBuffer buffer) : base(device, buffer)
        {
            Id = 10505;

            Process();
        }

        public string FriendTag { get; set; }
        public string FriendToken { get; set; }

        public override void Decode()
        {
            FriendTag = Reader.ReadScString();
            FriendToken = Reader.ReadScString();
        }

        public override async void Process()
        {
            Console.WriteLine($"[Friend Accept] Tag: {FriendTag}, Token: {FriendToken}");
            await new FriendsAcceptMessage(this.Device).SendAsync();
        }
    }
}