using System; 
using System.Diagnostics; 
using System.Linq; 
using System.Collections.Generic; 
using System.Threading.Tasks; 
using DotNetty.Buffers; 
using Newtonsoft.Json; 
using SharpRaven.Data; 
using ClashRoyale.Logic; 
using ClashRoyale.Protocol.Messages.Server; 
using ClashRoyale.Protocol; 
using ClashRoyale.Protocol.Messages.Client.Alliance; 
using ClashRoyale.Database; 
using ClashRoyale.Extensions; 
using ClashRoyale.Logic.Battle; 
using ClashRoyale.Logic.Home.StreamEntry; 
using ClashRoyale.Utilities.Netty; 
using ClashRoyale.Utilities.Utils;

namespace ClashRoyale.Protocol.Messages.Server
{
    public class FriendsAcceptMessage : PiranhaMessage
    {
        public FriendsAcceptMessage(Device Device) : base(Device)
        {
            Id = 20501;
        }

        public override void Encode()
        {
        }
    }
}