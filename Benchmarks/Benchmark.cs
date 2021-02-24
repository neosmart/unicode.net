using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public class Benchmark
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Utf32Encoder>();
        }
    }
}
