using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace VSL.UnitTests.BinaryTools
{
    [TestClass]
    public class PacketBufferTest
    {
        [TestMethod]
        public void TestDynamicReadBoundry()
        {
            using (PacketBuffer buffer = PacketBuffer.CreateDynamic(new byte[16]))
            {
                TestReadBoundry(buffer);
            }
        }

        [TestMethod]
        public void TestStaticReadBoundry()
        {
            using (PacketBuffer buffer = PacketBuffer.CreateStatic(new byte[16]))
            {
                TestReadBoundry(buffer);
            } 
        }

        private void TestReadBoundry(PacketBuffer buffer)
        {
            buffer.ReadLong();
            buffer.ReadSingle();
            buffer.ReadUShort();
            Assert.ThrowsException<ArgumentException>(() => buffer.ReadInt());
            Assert.ThrowsException<ArgumentException>(() => buffer.ReadString());
            Assert.ThrowsException<ArgumentException>(() => buffer.ReadByteArray());
            Assert.ThrowsException<ArgumentException>(() => buffer.ReadByteArray(13));
        }

        [TestMethod]
        public void TestStaticWriteBoundry()
        {
            using (PacketBuffer buffer = PacketBuffer.CreateStatic(16))
            {
                buffer.WriteLong(0x0475f9172a17abc9);
                buffer.WriteSingle(0.1f);
                buffer.WriteUShort(ushort.MaxValue);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.WriteInt(-1));
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.WriteString("Hello world!"));
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => buffer.WriteByteArray(new byte[3], false));
            }
        }
    }
}
