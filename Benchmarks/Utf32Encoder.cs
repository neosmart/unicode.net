using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Net;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using NeoSmart.Unicode;

namespace Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class Utf32Encoder
    {
        static Codepoint Input = new Codepoint(@"U+1F604");
        static Consumer Consumer = new Consumer();

        [GlobalSetup]
        public void InitializeGlobals()
        {
            Consumer.Consume(true);
            Consumer.Consume(Input);
        }

        [Benchmark]
        public void ViaYield()
        {
            static IEnumerable<byte> benchmark()
            {
                var utf32 = Input.AsUtf32();

                // From highest to lowest
                var b1 = (byte)(utf32 >> 24);
                yield return b1;
                var b2 = (byte)((utf32 & 0x00FFFFFF) >> 16);
                yield return b2;
                var b3 = (byte)(((UInt16)utf32) >> 8);
                yield return b3;
                var b4 = (byte)utf32;
                yield return b4;
            }

            benchmark().Consume(Consumer);
        }

        [Benchmark]
        public void ViaArray()
        {
            static byte[] benchmark()
            {
                var bytes = new byte[4];

                var utf32 = Input.AsUtf32();
                bytes[0] = (byte)(utf32 >> 24);
                bytes[1] = (byte)((utf32 & 0x00FFFFFF) >> 16);
                bytes[2] = (byte)(((UInt16)utf32) >> 8);
                bytes[3] = (byte)utf32;

                return bytes;
            }

            benchmark().Consume(Consumer);
        }

        [Benchmark]
        public void ArrayViaStackalloc()
        {
            static IEnumerable<byte> benchmark()
            {
                Span<byte> bytes = stackalloc byte[4];

                var utf32 = Input.AsUtf32();
                bytes[0] = (byte)(utf32 >> 24);
                bytes[1] = (byte)((utf32 & 0x00FFFFFF) >> 16);
                bytes[2] = (byte)(((UInt16)utf32) >> 8);
                bytes[3] = (byte)utf32;

                return bytes.ToArray();
            }

            benchmark().Consume(Consumer);
        }

        [Benchmark]
        public void ViaUtf32Codepoint()
        {
            static IEnumerable<byte> benchmark()
            {
                var utf32 = Input.AsUtf32();
                return new Utf32Codepoint(
                    (byte)(utf32 >> 24),
                    (byte)((utf32 & 0x00FFFFFF) >> 16),
                    (byte)(((UInt16)utf32) >> 8),
                    (byte)utf32);
            }

            benchmark().Consume(Consumer);
        }

        [Benchmark]
        public void AsUtf32Codepoint()
        {
            static Utf32Codepoint benchmark()
            {
                var utf32 = Input.AsUtf32();
                return new Utf32Codepoint(
                    (byte)(utf32 >> 24),
                    (byte)((utf32 & 0x00FFFFFF) >> 16),
                    (byte)(((UInt16)utf32) >> 8),
                    (byte)utf32);
            }

            var x = benchmark();
            Consumer.Consume(x.Byte0);
        }

        readonly struct Utf32Codepoint : IEnumerable<byte>
        {
            public readonly byte Byte0;
            public readonly byte Byte1;
            public readonly byte Byte2;
            public readonly byte Byte3;

            public Utf32Codepoint(byte b0, byte b1, byte b2, byte b3)
            {
                Byte0 = b0;
                Byte1 = b1;
                Byte2 = b2;
                Byte3 = b3;
            }

            public IEnumerator<byte> GetEnumerator()
            {
                //static IEnumerable<byte> Enumerate(in Utf32Codepoint cp)
                //{
                    yield return Byte0;
                    yield return Byte1;
                    yield return Byte2;
                    yield return Byte3;
                //}

                //Span<byte> span = stackalloc byte[4];
                //span[0] = Byte0;
                //span[1] = Byte1;
                //span[2] = Byte2;
                //span[3] = Byte3;

                //return Enumerate(this).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
