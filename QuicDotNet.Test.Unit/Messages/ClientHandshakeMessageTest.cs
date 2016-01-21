﻿using System;
using System.Linq;

namespace QuicDotNet.Test.Unit.Messages
{
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using QuicDotNet.Messages;

    [TestClass]
    public class ClientHandshakeMessageTest
    {
        internal static readonly Lazy<ClientHandshakeMessage> ClientInchoateGoogleFreshParametersClientMessageFactory = new Lazy<ClientHandshakeMessage>(() => new ClientHandshakeMessage
                                                                                                                                                               {
                                                                                                                                                                   { MessageTags.PAD, Enumerable.Repeat((byte)0x2d, 1053).ToArray() },
                                                                                                                                                                   { MessageTags.SNI, "clients2.google.com" },
                                                                                                                                                                   { MessageTags.VER, "Q025" },
                                                                                                                                                                   { MessageTags.CCS, new byte[] { 0x7b, 0x26, 0xe9, 0xe7, 0xe4, 0x5c, 0x71, 0xff, 0x01, 0xe8, 0x81, 0x60, 0x92, 0x92, 0x1a, 0xe8 } },
                                                                                                                                                                   { MessageTags.MSPC, 100 },
                                                                                                                                                                   { MessageTags.UAID, "Chrome/49.0.2623.0 Windows NT 6.1; WOW64" },
                                                                                                                                                                   { MessageTags.TCID, 0 },
                                                                                                                                                                   { MessageTags.PDMD, "X509" },
                                                                                                                                                                   { MessageTags.SRBF, 1048576 },
                                                                                                                                                                   { MessageTags.ICSL, 30 },
                                                                                                                                                                   { MessageTags.SCLS, 1 },
                                                                                                                                                                   { MessageTags.COPT, 1146636614 },
                                                                                                                                                                   { MessageTags.IRTT, 9248 },
                                                                                                                                                                   { MessageTags.CFCW, 15728640 },
                                                                                                                                                                   { MessageTags.SFCW, 6291456 }
                                                                                                                                                               });

        internal static readonly Lazy<ClientHandshakeMessage> ClientInchoateGoogleCachedServerParametersClientMessageFactory = new Lazy<ClientHandshakeMessage>(() => new ClientHandshakeMessage
                       {
                           { MessageTags.PAD, Enumerable.Repeat((byte)0x2d, 384).ToArray() },
                           { MessageTags.SNI, "www.google.com" },
                           { MessageTags.STK, new byte[] { 0xf7, 0x86, 0xe9, 0xf9, 0x18, 0xfd, 0x8e, 0xcf, 0x75, 0x9d, 0x6b, 0x5c, 0xf6, 0x44, 0x8c, 0xf9, 0xc7, 0x76, 0x14, 0x18, 0xf1, 0x45, 0x0a, 0x57, 0xee, 0x5b, 0xe9, 0x0c, 0x3a, 0x61, 0xce, 0xf1, 0x07, 0x9b, 0x62, 0x9f, 0x81, 0x5c, 0xa0, 0x50, 0xec, 0xb2, 0xfa, 0x92, 0x1d, 0x91, 0xf2, 0x2c, 0x49, 0x5b, 0xff, 0x67, 0x94, 0xc7, 0x66, 0xfb, 0x47, 0x5a } },
                           { MessageTags.VER, "Q025" },
                           { MessageTags.CCS, new byte[] { 0x7b, 0x26, 0xe9, 0xe7, 0xe4, 0x5c, 0x71, 0xff, 0x01, 0xe8, 0x81, 0x60, 0x92, 0x92, 0x1a, 0xe8 } },
                           { MessageTags.NONC, new byte[] { 0x56, 0x98, 0x91, 0x51, 0x6f, 0xbe, 0xfd, 0xe9, 0x4c, 0x1c, 0xcd, 0x85, 0x86, 0x79, 0xbc, 0x2d, 0x82, 0x5f, 0x4b, 0x27, 0x77, 0xdd, 0x4a, 0x27, 0xea, 0xcc, 0x63, 0xb1, 0x9a, 0xe2, 0xdc, 0x98 } },
                           { MessageTags.MSPC, 100 },
                           { MessageTags.AEAD, "AESG" },
                           { MessageTags.UAID, "Chrome/49.0.2623.0 Windows NT 6.1; WOW64" },
                           { MessageTags.SCID, new byte[] { 0x9d, 0xbc, 0xc3, 0x51, 0x1f, 0x2d, 0x6c, 0x17, 0xe4, 0x74, 0x85, 0xad, 0x89, 0xf4, 0x5e, 0x8b } },
                           { MessageTags.TCID, 0 },
                           { MessageTags.PDMD, "X509" },
                           { MessageTags.SRBF, 1048576 },
                           { MessageTags.ICSL, 30 },
                           { MessageTags.PUBS, new byte[] { 0x33, 0xa9, 0x56, 0x42, 0x9e, 0xcb, 0x12, 0xea, 0xa1, 0x3a, 0xad, 0x63, 0xe4, 0x96, 0xf3, 0x6a, 0x9f, 0xd4, 0xd6, 0x51, 0xfe, 0xb5, 0x9f, 0x71, 0x9b, 0xbb, 0x80, 0x62, 0xbb, 0x74, 0xd7, 0x1f} },
                           { MessageTags.SCLS, 1 },
                           { MessageTags.KEXS, "C255" },
                           { MessageTags.COPT, 1146636614 },
                           { MessageTags.CCRT, new byte[] { 0x45, 0x10, 0x46, 0x71, 0xc1, 0xe0, 0x9b, 0xf0, 0xe2, 0x63, 0x1a, 0x82, 0x7d, 0x71, 0x85, 0x5f, 0x40, 0x0b, 0x7b, 0x90, 0xa9, 0xae, 0x79, 0xeb } },
                           { MessageTags.IRTT, 24233 },
                           { MessageTags.CETV, new byte[] { 0xed, 0xfd, 0x02, 0x21, 0x0d, 0x2d, 0x7b, 0x6b, 0x19, 0xaf, 0x4f, 0xb0, 0xee, 0x83, 0x02, 0xcd, 0x4e, 0x9c, 0xe1, 0x1d, 0xbe, 0x1a, 0xed, 0xb8, 0xcd, 0x50, 0xe8, 0x7e, 0xcf, 0xb4, 0xb1, 0xce, 0xe2, 0xc9, 0xf1, 0x09, 0x70, 0x2d, 0x53, 0xce, 0x94, 0x58, 0xce, 0x08, 0xc6, 0xba, 0xc6, 0x33, 0xc3, 0xd7, 0xc9, 0x31, 0x52, 0x95, 0xcb, 0x8e, 0x55, 0xdb, 0xa6, 0xdb, 0x07, 0xc2, 0x7e, 0x0a, 0xc5, 0x07, 0x07, 0xf9, 0x10, 0xe5, 0xf7, 0xaf, 0x2b, 0xef, 0xa2, 0x15, 0xa9, 0x6f, 0x96, 0x53, 0xcc, 0xb0, 0x64, 0x2f, 0x77, 0x02, 0xe5, 0x49, 0xc0, 0xea, 0xa6, 0xea, 0xd5, 0x3b, 0xaf, 0x64, 0xaa, 0x70, 0xeb, 0x45, 0xe6, 0x7f, 0xa1, 0x0f, 0x3b, 0x07, 0xe6, 0xc7, 0x08, 0x99, 0xca, 0x76, 0xb5, 0x49, 0xa4, 0xc7, 0x7a, 0xe2, 0xa5, 0x72, 0x68, 0x00, 0x9d, 0xbd, 0xbd, 0xff, 0x76, 0xda, 0x8f, 0xf0, 0x50, 0xb4, 0xb3, 0x9e, 0x77, 0x64, 0xb6, 0x94, 0xf1, 0x4c, 0x66, 0xcb, 0x90, 0x06, 0xeb, 0x37, 0x06, 0x07, 0x37, 0xef, 0x2b, 0xf8, 0x13, 0x72, 0x9f, 0xa4, 0x33, 0x26, 0x14, 0x0a, 0x8c, 0x01, 0x9e, 0x41} },
                           { MessageTags.CFCW, 15728640 },
                           { MessageTags.SFCW, 6291456 }
                       });

        [TestMethod]
        public void ClientInchoateGoogleCachedServerParametersClientMessage()
        {
            var message = ClientInchoateGoogleCachedServerParametersClientMessageFactory.Value;

            var messageBytes = message.ToByteArray();
            Assert.IsNotNull(messageBytes);
            Debug.WriteLine(messageBytes.GenerateHexDumpWithASCII());

            Assert.AreEqual(messageBytes.Length, MessageLibrary.ClientInchoateGoogleCachedServerParametersClientMessageSubset.Length);

            // Soft warn
            for (var i = 0; i < messageBytes.Length; i++)
            {
                if (messageBytes[i] != MessageLibrary.ClientInchoateGoogleCachedServerParametersClientMessageSubset[i])
                    Debug.WriteLine($"Byte difference at position {i}: generated byte is {messageBytes[i]:x2} but reference byte was {MessageLibrary.ClientInchoateGoogleCachedServerParametersClientMessageSubset[i]:x2}");
            }

            // Hard test fail
            for (var i = 0; i < messageBytes.Length; i++)
            {
                Assert.AreEqual(messageBytes[i], MessageLibrary.ClientInchoateGoogleCachedServerParametersClientMessageSubset[i], $"Byte difference at position {i}: generated byte is {messageBytes[i]:x2} but reference byte was {MessageLibrary.ClientInchoateGoogleCachedServerParametersClientMessageSubset[i]:x2}");
            }
        }
    }
}
