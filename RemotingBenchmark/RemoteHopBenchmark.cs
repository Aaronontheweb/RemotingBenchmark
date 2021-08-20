using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;

namespace RemotingBenchmark
{
    /// <summary>
    /// BenchmarkDotNet configuration used for monitored jobs (not for microbenchmarks).
    /// </summary>
    public class MonitoringConfig : ManualConfig
    {
        public MonitoringConfig()
        {
            AddExporter(MarkdownExporter.GitHub);
        }
    }
    
    [Config(typeof(MonitoringConfig))]
    public class RemoteHopBenchmark
    {
        public static Config Config = @"
            akka.actor.provider = cluster
            akka.remote.dot-netty.tcp.port = 0
        ";
        
        private ActorSystem _sys1;
        private ActorSystem _sys2;

        private IActorRef _sys1Local;
        private IActorRef _sys2Local;
        private IActorRef _sys2Remote;
        
        private ShardedMessage _messageToSys2;

        [Params(10000)]
        public int MsgCount;

        [GlobalSetup]
        public async Task Setup()
        {
            _sys1 = ActorSystem.Create("BenchSys", Config);
            _sys2 = ActorSystem.Create("BenchSys", Config);

            var c1 = Akka.Cluster.Cluster.Get(_sys1);
            var c2 = Akka.Cluster.Cluster.Get(_sys2);

            await c1.JoinAsync(c1.SelfAddress);
            await c2.JoinAsync(c1.SelfAddress);

            _sys2Local = _sys2.ActorOf(Props.Create(() => new ParentActor()), "parent");
            await _sys2Local.Ask<string>("hi");

            _sys2Remote = await _sys1.ActorSelection(new RootActorPath(c2.SelfAddress) / "user" / "parent")
                .ResolveOne(TimeSpan.FromSeconds(3));

            _messageToSys2 = new ShardedMessage("C1", 100);
        }
        
        [Benchmark]
        public async Task SingleRequestResponseToLocalEntity()
        {
            for (var i = 0; i < MsgCount; i++)
                await _sys2Remote.Ask<ShardedMessage>(_messageToSys2);
        }
        
        [GlobalCleanup]
        public async Task Cleanup()
        {
            var t1 = _sys1.Terminate();
            var t2 = _sys2.Terminate();
            await Task.WhenAll(t1, t2);
        }
    }
}