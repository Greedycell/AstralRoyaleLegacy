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
using ClashRoyale.Logic.Home;

public class FriendsListMessage : PiranhaMessage
{
    public List<Friends> Friends { get; set; }

    public FriendsListMessage(Device device) : base(device)
    {
        Id = 20105;

        this.Friends = Device.Player.Home.Friends;

        Encode();
    }


    public async Task Encode()
    {
        var packet = this.Writer;
        packet.WriteInt(this.Friends.Count); // Number of friends
        packet.WriteInt(this.Friends.Count); // Number of requests (same)

        foreach (var Friend in this.Friends)
        {
            try
            {
                var friend_player = await PlayerDb.GetAsync(Friend.PlayerId);

                packet.WriteLong(friend_player.Home.Id);                    // Player ID
                packet.WriteBoolean(true);                            // Is online
                packet.WriteLong(friend_player.Home.Id);                    // Player ID again
                packet.WriteScString(friend_player.Home.Name ?? "Unnamed");              // Name
                packet.WriteScString(Friend.Facebook?.Identifier ?? ""); // Facebook ID
                packet.WriteScString(Friend.Gamecenter?.Identifier ?? ""); // Gamecenter ID
                packet.WriteVInt(friend_player.Home.ExpLevel);                    // Exp Level
                packet.WriteVInt(friend_player.Home.Arena.Trophies);                       // Score
                packet.WriteBoolean(false);                           // HasClan
                packet.WriteVInt(friend_player.Home.Arena.CurrentArena);                       // Arena
                packet.WriteScString(null);                           // Clan Name
                packet.WriteScString(null);                           // Clan Tag
                packet.WriteVInt(-1);                                 // Clan Badge
                packet.WriteInt(0);                                   // Challenge State
                packet.WriteInt(0);                                   // Challenge Wins
            }
            catch (Exception ex)
            {
                Console.WriteLine("Friend list could not load because an error has occured.");
            }
        }
    }
}