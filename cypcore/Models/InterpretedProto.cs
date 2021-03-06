﻿// CYPCore by Matthew Hellyer is licensed under CC BY-NC-ND 4.0.
// To view a copy of this license, visit https://creativecommons.org/licenses/by-nc-nd/4.0

using System.Text;

using Newtonsoft.Json;

using ProtoBuf;
using CYPCore.Extentions;

namespace CYPCore.Models
{
    [ProtoContract]
    public class InterpretedProto : IInterpretedProto
    {
        private const string hexUpper = "0123456789ABCDEF";

        [ProtoMember(1)]
        public string Hash { get; set; }
        [ProtoMember(2)]
        public ulong Node { get; set; }
        [ProtoMember(3)]
        public ulong Round { get; set; }
        [ProtoMember(4)]
        public TransactionProto Transaction { get; set; }
        [ProtoMember(5)]
        public string PublicKey { get; set; }
        [ProtoMember(6)]
        public string Signature { get; set; }
        [ProtoMember(7)]
        public string PreviousHash { get; set; }
        [ProtoMember(8)]
        public InterpretedType InterpretedType { get; set; }

        public override string ToString()
        {
            var v = new StringBuilder();
            v.Append(Node);
            v.Append(" | ");
            v.Append(Round);
            if (!string.IsNullOrEmpty(Hash))
            {
                v.Append(" | ");
                for (int i = 6; i < 12; i++)
                {
                    var c = Hash[i];
                    v.Append(new char[] { hexUpper[c >> 4], hexUpper[c & 0x0f] });
                }
            }
            return v.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToIdentifier()
        {
            return ToHash().ByteToHex().ToBytes();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] ToHash()
        {
            return Helper.Util.SHA384ManagedHash(Stream());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] Stream()
        {
            byte[] stream;
            using (var ts = new Helper.TangramStream())
            {
                ts
                .Append(Hash)
                .Append(Node)
                .Append(PreviousHash ?? string.Empty)
                .Append(Round)
                .Append(PublicKey ?? string.Empty)
                .Append(Signature ?? string.Empty)
                .Append(Transaction.Mix)
                .Append(Transaction.Ver);

                foreach (var bp in Transaction.Bp)
                {
                    ts.Append(bp.Proof);
                }

                foreach (var vin in Transaction.Vin)
                {
                    ts.Append(vin.Key.K_Image);
                }

                foreach (var vout in Transaction.Vout)
                {
                    ts.Append(vout.C);
                    ts.Append(vout.E);
                    ts.Append(vout.N);
                    ts.Append(vout.P);
                }

                foreach (var rct in Transaction.Rct)
                {
                    ts.Append(rct.M);
                    ts.Append(rct.P);
                    ts.Append(rct.S);
                }

                stream = ts.ToArray();
            }

            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Cast<T>()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
