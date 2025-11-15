using ClashRoyale.Logic;
using ClashRoyale.Utilities.Netty;

namespace ClashRoyale.Protocol.Messages.Server
{
    public class AvatarLocalRankingListMessage : PiranhaMessage
    {
        public AvatarLocalRankingListMessage(Device device) : base(device)
        {
            Id = 24404;
        }

        public override void Encode()
        {
            var players = Resources.Leaderboard.LocalPlayerRanking[Device.Player.Home.PreferredDeviceLanguage];
            var count = players.Count;

            Writer.WriteVInt(count);

            for (var i = 0; i < count; i++)
            {
                var player = players[i];

                Writer.WriteVInt(player.Home.HighId);
                Writer.WriteVInt(player.Home.LowId);
                Writer.WriteScString(player.Home.Name);

                Writer.WriteVInt(count + 1);
                Writer.WriteVInt(player.Home.Arena.Trophies);
                Writer.WriteVInt(200);

                Writer.WriteVInt(0);
                Writer.WriteVInt(0);
                Writer.WriteVInt(0);

                player.RankingEntry(Writer);
            }
        }
    }
}

/*using ClashRoyale.Logic;
using ClashRoyale.Utilities.Netty;

namespace ClashRoyale.Protocol.Messages.Server
{
    public class AvatarLocalRankingListMessage : PiranhaMessage
    {
        public AvatarLocalRankingListMessage(Device device) : base(device)
        {
            Id = 24404;
        }

        public override void Encode()
        {
            var players = Resources.Leaderboard.GlobalPlayerRanking;
            var count = players.Count;

            Writer.WriteVInt(count);

            for (var i = 0; i < count; i++)
            {
                var player = players[i];

                Writer.WriteVInt(player.Home.HighId);
                Writer.WriteVInt(player.Home.LowId);
                Writer.WriteScString(player.Home.Name);

                Writer.WriteVInt(count + 1);
                Writer.WriteVLong((long) player.Home.Arena.Trophies);
                Writer.WriteVInt(200);

                Writer.WriteVInt(0);
                Writer.WriteVInt(0);
                Writer.WriteVInt(0);

                player.RankingEntry(Writer);
            }
/*
            var alliances = Resources.Leaderboard.GlobalAllianceRanking;
            var alliances_count = alliances.Count;

            Writer.WriteVInt(count);

            for (var i = 0; i < count; i++)
            {
                var alliance = alliances[i];

                Writer.WriteVInt(alliance.HighId);
                Writer.WriteVInt(alliance.LowId);
                Writer.WriteScString(alliance.Name);

                Writer.WriteVInt(count + 1);
                Writer.WriteVLong((long) alliance.Score);
                Writer.WriteVInt(200);

                Writer.WriteVInt(0);
                Writer.WriteVInt(0);
                Writer.WriteVInt(0);

                alliance.AllianceRankingEntry(Writer);
            } *
        }
    }
}*/