﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FunctionApp1
{
    internal static class RsaUtil
    {
        public static RSAParameters CreateParameter(string base64PrivateKey)
        {
            byte[] der = null;
            der = Convert.FromBase64String(base64PrivateKey);
            return CreateParameter(der);
        }

        private static RSAParameters CreateParameter(byte[] der)
        {
            byte[] sequence = null;
            using (var reader = new BinaryReader(new MemoryStream(der)))
            {
                sequence = Read(reader);
            }

            var parameters = new RSAParameters();
            using (var reader = new BinaryReader(new MemoryStream(sequence)))
            {
                Read(reader); // version
                parameters.Modulus = Read(reader);
                parameters.Exponent = Read(reader);
                parameters.D = Read(reader);
                parameters.P = Read(reader);
                parameters.Q = Read(reader);
                parameters.DP = Read(reader);
                parameters.DQ = Read(reader);
                parameters.InverseQ = Read(reader);
            }

            return parameters;
        }


        private static byte[] Read(BinaryReader reader)
        {
            // tag
            reader.ReadByte();

            // length
            int length = 0;
            byte b = reader.ReadByte();
            if ((b & 0x80) == 0x80) // length が128 octet以上
            {
                int n = b & 0x7F;
                byte[] buf = new byte[] { 0x00, 0x00, 0x00, 0x00 };
                for (var i = n - 1; i >= 0; --i)
                    buf[i] = reader.ReadByte();
                length = BitConverter.ToInt32(buf, 0);
            }
            else // length が 127 octet以下
            {
                length = b;
            }

            // value
            if (length == 0)
                return new byte[0];
            byte first = reader.ReadByte();
            if (first == 0x00) length -= 1; // 最上位byteが0x00の場合は、除いておく
            else reader.BaseStream.Seek(-1, SeekOrigin.Current); // 1byte 読んじゃったので、streamの位置を戻しておく
            return reader.ReadBytes(length);
        }
    }
}
