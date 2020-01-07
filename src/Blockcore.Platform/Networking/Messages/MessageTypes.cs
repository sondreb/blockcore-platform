namespace Blockcore.Platform.Networking.Messages
{
    // TODO: Migrate to use a fixed-length string based message header.
    public class MessageTypes
    {
        public static ushort ACK = 0;
        public static ushort CHAT = 1;
        public static ushort HUBINFO = 2;
        public static ushort KEEPALIVE = 3;
        public static ushort NOTIFY = 4;
        public static ushort CONNECT_REQUEST = 5;
        public static ushort CONNECT_APPROVAL = 6;
        public static ushort TEST = 999;
        public static ushort NOT_FOUND = 404;
    }
}
