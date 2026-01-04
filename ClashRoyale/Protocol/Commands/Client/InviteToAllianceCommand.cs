using System;
using ClashRoyale.Logic;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol
{
    internal class InviteToAllianceCommand : LogicCommand
    {
        public InviteToAllianceCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
            Process();
        }

        public override async void Process()
        {
            await new InviteToAllianceMessage(this.Device).SendAsync();
        }
    }
}