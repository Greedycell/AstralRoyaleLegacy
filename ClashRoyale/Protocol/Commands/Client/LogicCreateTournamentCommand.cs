using System;
using ClashRoyale.Logic;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol
{
    internal class LogicCreateTournamentCommand : LogicCommand
    {
        public LogicCreateTournamentCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
            Process();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool SetPassword { get; set; }
        public string Password { get; set; }
        public int ChestPrize { get; set; }
        public bool ShowToClan { get; set; }

        public override void Decode()
        {
            base.Decode();
            
            Reader.ReadVInt();//67
            Reader.ReadVInt();//67
            Reader.ReadVInt();//0
            Reader.ReadVInt();//7

            Name = Reader.ReadScString();
            Description = Reader.ReadScString();
            SetPassword = Reader.ReadBoolean();
            ChestPrize = Reader.ReadVInt();
            ShowToClan = Reader.ReadBoolean();
        }

        public override async void Process()
        {
            Console.WriteLine($"Name: {Name}, Description: {Description}, SetPassword: {SetPassword}, Password: {Password}, ChestPrize: {ChestPrize}, ShowToClan: {ShowToClan}");
            /*await new ServerErrorMessage(Device)
            {
                Message = "Not implemented yet."
            }.SendAsync();*/
        }
    }
}