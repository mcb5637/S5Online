using System;
using System.Collections.Generic;

namespace S5GameServices
{
    public class Message
    {
        public MessageType Type;
        public MessageCode Code;
        public MessageDataList Data;
        public byte[] Body;
        public byte NumA, NumB;

        public Message(MessageType type, MessageCode code, byte[] data, byte numA, byte numB)
        {
            Type = type;
            Code = code;
            Body = data;
            Data = MessageDataList.Parse(data);
            NumA = numA;
            NumB = numB;
        }

        public Message(MessageType type, MessageCode code, MessageDataList args, byte numA, byte numB)
        {
            Type = type;
            Code = code;
            Data = args;
            NumA = numA;
            NumB = numB;
        }

        public byte[] Serialize(Blowfish blowfishContext = null)
        {
            var msgData = Data.Serialize();

            switch (Type)
            {
                case MessageType.GSMessage:
                    XorCrypt.Encrypt(msgData);
                    break;

                case MessageType.GSEncryptMessage:
                    if (blowfishContext == null)
                        throw new Exception("Missing blowfish key!");

                    blowfishContext.EncipherPadded(ref msgData);
                    break;

                case MessageType.GameMessage:
                    throw new NotImplementedException("Sending clGameMessage, WAT DO?");
            }

            var msgLen = msgData.Length + 6;
            var message = new byte[msgLen];

            message[0] = (byte)(msgLen >> 16);
            message[1] = (byte)(msgLen >> 8);
            message[2] = (byte)msgLen;
            message[3] = (byte)((int)Type << 6);
            message[4] = (byte)Code;
            message[5] = (byte)(NumA << 4 | (NumB & 0x0F));
            Array.Copy(msgData, 0, message, 6, msgData.Length);

            return message;
        }

        public static IEnumerable<Message> ParseIncoming(byte[] data, Blowfish blowfishContext = null)
        {
            for (int pos = 0; pos < data.Length;)
            {
                if (pos + 6 > data.Length)
                    throw new Exception("Incomplete Header received!");

                int packetSize = data[pos + 2] + 256 * data[pos + 1] + 256 * 256 * data[pos + 0];
                MessageType type = (MessageType)(data[pos + 3] >> 6);
                MessageCode code = (MessageCode)(data[pos + 4]);
                byte numA = (byte)(data[pos + 5] >> 4);
                byte numB = (byte)(data[pos + 5] & 0x0F);

                if (pos + packetSize > data.Length)
                    throw new Exception("Incomplete message body received!");

                var argLen = packetSize - 6;
                byte[] msgData = new byte[argLen];

                if (argLen != 0)
                {
                    Array.Copy(data, pos + 6, msgData, 0, argLen);

                    switch (type)
                    {
                        case MessageType.GSMessage:
                            XorCrypt.Decrypt(msgData);
                            break;

                        case MessageType.GSEncryptMessage:
                            if (blowfishContext == null)
                                throw new Exception("Received BF encrypted message, but no key available!");

                            blowfishContext.DecipherPadded(ref msgData);
                            break;

                        case MessageType.GameMessage:
                            throw new NotImplementedException("GameMessage received, WAT DO?");
                    }
                }

                yield return new Message(type, code, msgData, numA, numB);

                pos += packetSize;
            }
            yield break;
        }

        public override string ToString()
        {
            return Code.ToString() + string.Format(" ({0}, {1}, {2}) ", (int)Code, NumA, NumB) + Data.ToString();
        }
    }

    public class CDKeyMessage
    {
        public MessageDataList Data;
        public byte[] BinData;//DBG

        public CDKeyMessage(MessageDataList data)
        {
            Data = data;
        }

        public CDKeyMessage(byte[] data)
        {
            if (data[0] != 0xD3)
                throw new Exception("Unknown packet!");

            int dataLen = data[4];
            var packetData = new byte[dataLen];
            Array.Copy(data, 5, packetData, 0, dataLen);
            Global.CDKeyCrypt.DecipherPadded(ref packetData);

            BinData = packetData;
            Data = MessageDataList.Parse(packetData);
        }

        public byte[] Serialize()
        {
            var msgData = Data.Serialize();
            Global.CDKeyCrypt.EncipherPadded(ref msgData);

            var msgLen = msgData.Length + 5;
            var message = new byte[msgLen];

            message[0] = 0xD3;
            message[4] = (byte)msgData.Length;
            Array.Copy(msgData, 0, message, 5, msgData.Length);

            return message;
        }
    }

    public interface IMessageDataElement
    {
        bool IsList { get; }

        void Serialize(List<byte> dataBlock);

        //now this is a crap solution but its faster than casts and less of an eyesore
        //throws execeptions if you dont get a compatible type though

        IMessageDataElement this[int i] { get; }
        string AsString { get; set; }
        int AsInt { get; set; }
        byte[] AsBinary { get; set; }
    }

    public class MessageDataList : List<IMessageDataElement>, IMessageDataElement
    {
        public bool IsList
        { get { return true; } }

        public int AsInt
        { get { throw new Exception(); } set { throw new Exception(); } }
        public string AsString
        { get { throw new Exception(); } set { throw new Exception(); } }

        public byte[] AsBinary
        { get { throw new Exception(); } set { throw new Exception(); } }

        public static MessageDataList Parse(byte[] data)
        {
            int pos = 0;
            return new MessageDataList(data, ref pos);
        }

        public MessageDataList()
        { }

        protected MessageDataList(byte[] data, ref int pos)
        {
            for (; pos < data.Length;)
            {
                byte type = data[pos];
                pos++;

                switch (type)
                {
                    case (byte)'s':
                        int endPos;
                        for (endPos = pos; endPos < data.Length && data[endPos] != 0; endPos++) ;
                        Add(new MessageDataString(Global.ServerEncoding.GetString(data, pos, endPos - pos)));
                        pos = endPos + 1;
                        break;

                    case (byte)'b':
                        int binLen = (data[pos] << 24) + (data[pos + 1] << 16) + (data[pos + 2] << 8) + data[pos + 3];

                        if (pos + 4 + binLen > data.Length)
                            throw new Exception("Binary DataElement too large!");

                        var binData = new byte[binLen];
                        Array.Copy(data, pos + 4, binData, 0, binLen);

                        Add(new MessageDataBinary(binData));
                        pos += 4 + binLen;
                        break;

                    case (byte)'[':
                        var subList = new MessageDataList(data, ref pos);
                        if (data[pos - 1] != (byte)']')
                            throw new Exception("List not finished!");
                        Add(subList);
                        break;

                    case (byte)']':
                        return;

                    default:
                        throw new Exception("Unknown Element in DataList!");
                }
            }
        }

        public byte[] Serialize()
        {
            var dataBlock = new List<byte>();
            Serialize(dataBlock);
            return dataBlock.ToArray();
        }

        public void Serialize(List<byte> dataBlock)
        {
            foreach (var elm in this)
            {
                if (elm.IsList)
                {
                    dataBlock.Add((byte)'[');
                    elm.Serialize(dataBlock);
                    dataBlock.Add((byte)']');
                }
                else
                    elm.Serialize(dataBlock);
            }
        }

        public override string ToString()
        {
            var str = "[";
            foreach (var v in this)
                str += " " + v.ToString();
            return str + "]";
        }
    }

    public class MessageDataString : IMessageDataElement
    {
        public bool IsList
        { get { return false; } }

        public string Text;
        public int AsInt
        { get { return int.Parse(Text); } set { Text = value.ToString(); } }

        public string AsString
        { get { return Text; } set { Text = value; } }

        public byte[] AsBinary
        { get { throw new Exception(); } set { throw new Exception(); } }

        public IMessageDataElement this[int n]
        { get { throw new Exception("NOPE, its a string!"); } }

        public MessageDataString(int number)
        {
            Text = number.ToString();
        }

        public MessageDataString(string text)
        {
            Text = text;
        }

        public void Serialize(List<byte> dataBlock)
        {
            dataBlock.Add((byte)'s');
            dataBlock.AddRange(Global.ServerEncoding.GetBytes(Text));
            dataBlock.Add(0);
        }

        public override string ToString()
        {
            return "\"" + Text + "\"";
        }
    }

    public class MessageDataBinary : IMessageDataElement
    {
        public bool IsList
        { get { return false; } }

        public int AsInt
        {
            get { throw new NotImplementedException("would make sense to have this"); }
            set { throw new NotImplementedException("maybe"); }
        }

        public string AsString
        {
            get { throw new NotImplementedException("would make sense to have this"); }
            set { throw new NotImplementedException("maybe"); }
        }

        public byte[] AsBinary
        { get { return Data; } set { Data = value; } }

        public IMessageDataElement this[int n]
        { get { throw new Exception("NOPE, its a binary!"); } }

        public byte[] Data;

        public MessageDataBinary(byte[] data)
        {
            Data = data;
        }

        public void Serialize(List<byte> dataBlock)
        {
            dataBlock.Add((byte)'b');
            dataBlock.Add((byte)(Data.Length >> 24));
            dataBlock.Add((byte)(Data.Length >> 16));
            dataBlock.Add((byte)(Data.Length >> 8));
            dataBlock.Add((byte)(Data.Length));
            dataBlock.AddRange(Data);
        }

        public override string ToString()
        {
            return "Bin{" + BitConverter.ToString(Data).Replace("-", " ") + "}";
        }
    }

    public enum MessageType : byte
    { GSMessage = 0, GameMessage = 1, GSEncryptMessage = 2 }

    public enum MessageCode : byte
    {
        NEWUSERREQUEST = 0x1,
        CONNECTIONREQUEST = 0x2,
        PLAYERNEW = 0x3,
        DISCONNECTION = 0x4,
        PLAYERREMOVED = 0x5,
        NEWS = 0x7,
        SEARCHPLAYER = 0x8,
        REMOVEACCOUNT = 0x9,
        SERVERSLIST = 0xB,
        SESSIONLIST = 0xD,
        PLAYERLIST = 0xF,
        GETGROUPINFO = 0x10,
        GROUPINFO = 0x11,
        GETPLAYERINFO = 0x12,
        PLAYERINFO = 0x13,
        CHATALL = 0x14,
        CHATLIST = 0x15,
        CHATSESSION = 0x16,
        CHAT = 0x18,
        CREATESESSION = 0x1A,
        SESSIONNEW = 0x1B,
        JOINSESSION = 0x1C,
        JOINNEW = 0x1F,
        LEAVESESSION = 0x20,
        JOINLEAVE = 0x21,
        SESSIONREMOVE = 0x22,
        GSSUCCESS = 0x26,
        GSFAIL = 0x27,
        BEGINGAME = 0x28,
        UPDATEPLAYERINFO = 0x2D,
        MASTERCHANGED = 0x30,
        UPDATESESSIONSTATE = 0x33,
        URGENTMESSAGE = 0x34,
        NEWWAITMODULE = 0x36,
        KILLMODULE = 0x37,
        STILLALIVE = 0x3A,
        PING = 0x3B,
        PLAYERKICK = 0x3C,
        PLAYERMUTE = 0x3D,
        ALLOWGAME = 0x3E,
        FORBIDGAME = 0x3F,
        GAMELIST = 0x40,
        UPDATEADVERTISMEMENTS = 0x41,
        UPDATENEWS = 0x42,
        VERSIONLIST = 0x43,
        UPDATEVERSIONS = 0x44,
        UPDATEDISTANTROUTERS = 0x46,
        ADMINLOGIN = 0x47,
        STAT_PLAYER = 0x48,
        STAT_GAME = 0x49,
        UPDATEFRIEND = 0x4A,
        ADDFRIEND = 0x4B,
        DELFRIEND = 0x4C,
        LOGINWAITMODULE = 0x4D,
        LOGINFRIENDS = 0x4E,
        ADDIGNOREFRIEND = 0x4F,
        DELIGNOREFRIEND = 0x50,
        STATUSCHANGE = 0x51,
        JOINARENA = 0x52,
        LEAVEARENA = 0x53,
        IGNORELIST = 0x54,
        IGNOREFRIEND = 0x55,
        GETARENA = 0x56,
        GETSESSION = 0x57,
        PAGEPLAYER = 0x58,
        FRIENDLIST = 0x59,
        PEERMSG = 0x5A,
        PEERPLAYER = 0x5B,
        DISCONNECTFRIENDS = 0x5C,
        JOINWAITMODULE = 0x5D,
        LOGINSESSION = 0x5E,
        DISCONNECTSESSION = 0x5F,
        PLAYERDISCONNECT = 0x60,
        ADVERTISEMENT = 0x61,
        MODIFYUSER = 0x62,
        STARTGAME = 0x63,
        CHANGEVERSION = 0x64,
        PAGER = 0x65,
        LOGIN = 0x66,
        PHOTO = 0x67,
        LOGINARENA = 0x68,
        SQLCREATE = 0x6A,
        SQLSELECT = 0x6B,
        SQLDELETE = 0x6C,
        SQLSET = 0x6D,
        SQLSTAT = 0x6E,
        SQLQUERY = 0x6F,
        ROUTEURLIST = 0x7F,
        DISTANCEVECTOR = 0x83,
        WRAPPEDMESSAGE = 0x84,
        CHANGEFRIEND = 0x85,
        NEWRELFRIEND = 0x86,
        DELRELFRIEND = 0x87,
        NEWIGNOREFRIEND = 0x88,
        DELETEIGNOREFRIEND = 0x89,
        ARENACONNECTION = 0x8A,
        ARENADISCONNECTION = 0x8B,
        ARENAWAITMODULE = 0x8C,
        ARENANEW = 0x8D,
        NEWBASICGROUP = 0x8F,
        ARENAREMOVED = 0x90,
        DELETEBASICGROUP = 0x91,
        SESSIONSBEGIN = 0x92,
        GROUPDATA = 0x94,
        ARENA_MESSAGE = 0x97,
        ARENALISTREQUEST = 0x9D,
        ROUTERPLAYERNEW = 0x9E,
        BASEGROUPREQUEST = 0x9F,
        UPDATEPLAYERPING = 0xA6,
        UPDATEGROUPSIZE = 0xA9,
        SLEEP = 0xB3,
        WAKEUP = 0xB4,
        SYSTEMPAGE = 0xB5,
        SESSIONOPEN = 0xBD,
        SESSIONCLOSE = 0xBE,
        LOGINCLANMANAGER = 0xC0,
        DISCONNECTCLANMANAGER = 0xC1,
        CLANMANAGERPAGE = 0xC2,
        UPDATECLANPLAYER = 0xC3,
        PLAYERCLANS = 0xC4,
        GETPERSISTANTGROUPINFO = 0xC7,
        UPDATEGROUPPING = 0xCA,
        DEFERREDGAMESTARTED = 0xCB,
        QQQ_ = 0xCC,
        BEGINCLIENTHOSTGAME = 0xCD,
        LOBBY_MSG = 0xD1,
        LOBBYSERVERLOGIN = 0xD2,
        SETGROUPSZDATA = 0xD3,
        GROUPSZDATA = 0xD4,
        UPDATEPLAYERGROUPPING = 0xD8,
        RSAEXCHANGE_ = 0xDB,
        ROUTERGLOBALSTATS = 0xDC,
        GETMOTD_ = 0xDE,
    }
}