using System;
using System.Threading.Tasks;
using ClashRoyale.Logic;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Logic.Home;
using ClashRoyale.Utilities.Netty;
using ClashRoyale.Files;
using ClashRoyale.Files.CsvLogic;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol
{
    internal class ClaimAchievementsCommand : LogicCommand
    {
        public ClaimAchievementsCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
        }

        public int AchievementId { get; set; }

        public override void Decode()
        {
            AchievementId = Reader.ReadVInt();
            Reader.ReadVInt();
        }

        public override void Process()
        {
            var achievement = (ClashRoyale.Files.CsvLogic.Achievements)
                Csv.Tables.Get(Csv.Files.Achievements).GetDataWithId(AchievementId);

            Device.Player.Home.Achievements.Add(new Achievement
            {
                Id = AchievementId,
                Data = achievement.ActionCount
            });

            Device.Player.Home.Diamonds += (achievement.DiamondReward);
            Device.Player.Home.AddExpPoints(achievement.ExpReward);

            Console.WriteLine($"[Achivement] AchievementId: {AchievementId}, DiamondReward: {achievement.DiamondReward}, ExpPoints: {achievement.ExpReward}");
        }
    }
}