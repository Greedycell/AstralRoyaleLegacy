using System;
using System.IO;
using System.Linq;
using ClashRoyale.Logic;
using ClashRoyale.Protocol.Commands.Server;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol.Messages.Client.Home
{
    public class ChangeAvatarNameMessage : PiranhaMessage
    {
        public ChangeAvatarNameMessage(Device device, IByteBuffer buffer) : base(device, buffer)
        {
            Id = 10212;
        }

        public string Name { get; set; }
        private static readonly string[] bannedWords;

        static ChangeAvatarNameMessage()
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filterPath = Path.Combine(currentDirectory, "filter.json");

                if (File.Exists(filterPath))
                {
                    bannedWords = File.ReadAllLines(filterPath);
                }
                else
                {
                    bannedWords = Array.Empty<string>();
                }
            }
            catch
            {
                bannedWords = Array.Empty<string>();
            }
        }

        // Check if name contains banned words
        private bool ContainsBannedWords(string message)
        {
            if (string.IsNullOrEmpty(message)) return false;
            return bannedWords.Any(word => message.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public override void Decode()
        {
            Name = Reader.ReadScString();
        }

        public override async void Process()
        {
            if (string.IsNullOrEmpty(Name)) return;
            if (Name.Length < 2 || Name.Length > 15) return;
            if (Device?.Player == null) return;

            // Block inappropriate names
            if (ContainsBannedWords(Name))
            {
                await new ServerErrorMessage(Device)
                { 
                    Message = "Invalid name! Please try another one." 
                }.SendAsync();
                return; // Do not change the name
            }

            var home = Device.Player.Home;
            if (home == null) return;
            if (home.NameSet >= 2) return;

            home.Name = Name;

            var info = home.AllianceInfo;
            if (info != null && info.HasAlliance)
            {
                var alliance = await Resources.Alliances.GetAllianceAsync(info.Id);
                if (alliance != null)
                {
                    var member = alliance.GetMember(home.Id);
                    if (member != null)
                    {
                        member.Name = Name;
                        alliance.Save();
                    }
                }
            }

            await new AvailableServerCommand(Device)
            {
                Command = new LogicChangeNameCommand(Device)
                {
                    NameSet = home.NameSet
                }
            }.SendAsync();

            home.NameSet++;
            Device.Player.Save();
        }
    }
}