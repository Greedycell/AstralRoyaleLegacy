using System;
using ClashRoyale.Logic;
using ClashRoyale.Logic.Home;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;
using ClashRoyale.Database;
using DotNetty.Buffers;
using System.Threading.Tasks;
using ClashRoyale.Logic.Clan;

namespace ClashRoyale.Protocol
{
    internal class LogicShareReplayCommand : LogicCommand
    {
        public LogicShareReplayCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
        }

        public override async void Process()
        {
            await new ServerErrorMessage(Device)
            {
                Message = "Not implemented yet."
            }.SendAsync();
        }
    }
}