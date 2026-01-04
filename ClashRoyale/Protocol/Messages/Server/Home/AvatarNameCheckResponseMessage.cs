using ClashRoyale.Logic;
using ClashRoyale.Utilities.Netty;

namespace ClashRoyale.Protocol.Messages.Server
{
    public class AvatarNameCheckResponseMessage : PiranhaMessage
    {
        public AvatarNameCheckResponseMessage(Device device) : base(device)
        {
            Id = 20300;
        }

        public string Name { get; set; }
        public int ErrorCode { get; set; }
        public bool IsValid { get; set; }

        // Errorcodes:
        // 1 = invalid
        // 2 = too short
        // 3 = already changed
        // 4 = invalid mirror
        // 5 = low level

        public override void Encode()
        {
            Writer.WriteBoolean(IsValid); // IsValid
            Writer.WriteInt(ErrorCode); // ErrorCode
            Writer.WriteScString(Name);
        }
    }
}