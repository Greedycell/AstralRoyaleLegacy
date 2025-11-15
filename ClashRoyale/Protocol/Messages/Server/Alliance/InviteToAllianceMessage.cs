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
using ClashRoyale.Database; 
using ClashRoyale.Extensions;
using ClashRoyale.Utilities.Netty; 
using ClashRoyale.Utilities.Utils;

namespace ClashRoyale.Protocol.Messages.Server
{
    public class InviteToAllianceMessage : PiranhaMessage
    {
        public string Token { get; private set; }

        public InviteToAllianceMessage(Device Device) : base(Device)
        {
            Token = GenerateRandomToken(8);
        }

        public override void Encode()
        {
            var packet = this.Writer;
            packet.WriteBoolean(true);
            packet.WriteScString(Token);
            Device.Player.Home.InviteAllianceToken = Token;
        }

        private static string GenerateRandomToken(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}