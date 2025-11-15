using ClashRoyale;
using ClashRoyale.Logic;
using ClashRoyale.Logic.Battle;
using ClashRoyale.Core;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol.Commands.Client
{
    public class StartMatchmakeCommand : LogicCommand
    {
        public StartMatchmakeCommand(Device device, IByteBuffer buffer) : base(device, buffer)
        {
        }

        public bool Is2V2 { get; set; }

        public override void Decode()
        {
            base.Decode();

            Reader.ReadVInt();
            Reader.ReadVInt();

            Is2V2 = Reader.ReadBoolean();
        }

        public override void Process()
        {
            if (Is2V2)
            {
                //await new MatchmakeFailedMessage(Device).SendAsync();
                //await new CancelMatchmakeDoneMessage(Device).SendAsync();

                var players = Resources.DuoBattles.Dequeue;
                if (players != null)
                {
                    players.Add(Device.Player);

                    var battle = new LogicBattle(false,Device.Player.Home.Arena.CurrentArena + 1, players);

                    Resources.DuoBattles.Add(battle);

                    foreach (var player in players)
                    {
                        player.Battle = battle;
                    }

                    battle.Start();
                }
                else
                {
                    Resources.DuoBattles.Enqueue(Device.Player);
                }
            }
            else
            {
                var enemy = Resources.Battles.Dequeue;
                // player can't attack an opponent from any arena
                if (Resources.Configuration.CantAttackAnyArena)
                {
                    if (enemy != null && enemy.Home.Arena.CurrentArena == Device.Player.Home.Arena.CurrentArena)
                    {
                        var battle = new LogicBattle(false, enemy.Home.Arena.CurrentArena + 1)
                        {
                            Device.Player, enemy
                        };

                        Resources.Battles.Add(battle);

                        Device.Player.Battle = battle;
                        enemy.Battle = battle;

                        battle.Start();
                    }
                    else
                    {
                        Resources.Battles.Enqueue(Device.Player);
                    }
                }
                else
                // player can attack an opponent from any arena
                {
                    if (enemy != null)
                    {
                        var battle = new LogicBattle(false, enemy.Home.Arena.CurrentArena + 1)
                        {
                            Device.Player, enemy
                        };

                        Resources.Battles.Add(battle);

                        Device.Player.Battle = battle;
                        enemy.Battle = battle;

                        battle.Start();
                    }
                    else
                    {
                        Resources.Battles.Enqueue(Device.Player);
                    }
                }
            }
        }
    }
}