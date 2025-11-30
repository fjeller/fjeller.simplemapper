using BenchmarkDotNet.Running;
using Benchmarks.Fjeller.SimpleMapper;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

BenchmarkRunner.Run<SimpleMapperBenchmarks>();
BenchmarkRunner.Run<ManualMappingComparison>();
