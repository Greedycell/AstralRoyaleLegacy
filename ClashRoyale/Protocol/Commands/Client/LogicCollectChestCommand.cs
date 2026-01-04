using System;
using ClashRoyale.Files.CsvLogic;
using ClashRoyale.Files;
using ClashRoyale.Logic;
using ClashRoyale.Logic.Home.Chests.Items;
using ClashRoyale.Protocol.Commands.Server;
using ClashRoyale.Protocol.Messages.Server;
using DotNetty.Buffers;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;
using ClashRoyale.Logic.Home.Chests;
using System.Diagnostics;

namespace ClashRoyale.Protocol.Commands.Client
{   
    public class LogicCollectChestCommand : LogicCommand
    {
        public LogicCollectChestCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
        }

        public int InstanceId { get; set; }

        public override void Decode()
        {
            var chestID = 8;
            base.Decode();
            Reader.ReadVInt();
            chestID = Reader.ReadVInt();
            //Console.WriteLine($"[Debug] [C] Slot Chest opened by {Device.Player.Home.Name}, {Device.Player.Home.Id} ");
        }
        public override async void Process()
        {
            var chest = Device.Player.Home.Chests.BuyChest(InstanceId, Chest.ChestType.Slot);

            await new AvailableServerCommand(Device)
            {
                Command = new ChestDataCommand(Device)
                {
                    Chest = chest
                 
                } 
            }.SendAsync(); 
        }
    }
}

/*using System;
using System.Linq;
using ClashRoyale.Files;
using ClashRoyale.Files.CsvLogic;
using ClashRoyale.Logic;
using ClashRoyale.Logic.Home.Chests.Items;
using ClashRoyale.Protocol.Commands.Server;
using ClashRoyale.Protocol.Messages.Server;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol.Commands.Client
{
    public class LogicCollectChestCommand : LogicChestCommand
    {
        public LogicCollectChestCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
        }

        public bool UseGems { get; set; }

        public override void Decode()
        {
            base.Decode(); 
            UseGems = Reader.ReadBoolean(); 
        }

        public override async void Process()
        {
            var home = Device.Player.Home;
            var chestToOpen = home.PlayerChests.FirstOrDefault(c => c.Slot == SlotId);

            if (chestToOpen == null) return;

            bool isReadyToOpen = chestToOpen.UnlockTime > DateTime.MinValue;

            if (UseGems)
            {
               isReadyToOpen = true; 
            }

            if (!isReadyToOpen)
            {
                Console.WriteLine($"Player {home.Name} tried to collect a chest in slot {SlotId} that was not unlocked.");
                return;
            }

            var chestWithRewards = home.Chests.BuyChest(chestToOpen.ChestId, Chest.ChestType.Slot);
            if (chestWithRewards != null)
            {
                await new AvailableServerCommand(Device)
                {
                    Command = new ChestDataCommand(Device)
                    {
                        Chest = chestWithRewards
                    }
                }.SendAsync();

                home.PlayerChests.Remove(chestToOpen);
                Console.WriteLine($"Chest in slot {SlotId} opened and REMOVED for player {home.Name}.");

                Device.Player.Save();
            }
        }
    }
}*/