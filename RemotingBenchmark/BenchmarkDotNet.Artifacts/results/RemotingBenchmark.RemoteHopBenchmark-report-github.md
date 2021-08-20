``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19041.1165 (2004/May2020Update/20H1)
AMD Ryzen 7 1700, 1 CPU, 16 logical and 8 physical cores
.NET SDK=5.0.302
  [Host]     : .NET Core 3.1.17 (CoreCLR 4.700.21.31506, CoreFX 4.700.21.31502), X64 RyuJIT
  DefaultJob : .NET Core 3.1.17 (CoreCLR 4.700.21.31506, CoreFX 4.700.21.31502), X64 RyuJIT


```
|                             Method | MsgCount |    Mean |    Error |   StdDev |
|----------------------------------- |--------- |--------:|---------:|---------:|
| SingleRequestResponseToLocalEntity |    10000 | 3.575 s | 0.0681 s | 0.0569 s |
