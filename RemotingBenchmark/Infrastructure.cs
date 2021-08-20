using System.Threading.Tasks;
using Akka.Actor;

namespace RemotingBenchmark
{
    public sealed class ShardedEntityActor : ReceiveActor
    {
        public sealed class Resolve
        {
            public static readonly Resolve Instance = new Resolve();
            private Resolve(){}
        }

        public sealed class ResolveResp
        {
            public ResolveResp(string entityId, Address addr)
            {
                EntityId = entityId;
                Addr = addr;
            }

            public string EntityId { get; }
            
            public Address Addr { get; }
        }
        
        public ShardedEntityActor()
        {
            ReceiveAny(_ => Sender.Tell(_));
        }
    }

    public sealed class ParentActor : ReceiveActor
    {
        private IActorRef _child;

        public ParentActor()
        {
            ReceiveAny(_ => _child.Forward(_));
        }

        protected override void PreStart()
        {
            _child = Context.ActorOf(Props.Create(() => new ShardedEntityActor()));
        }
    }


    public sealed class BulkSendActor : ReceiveActor
    {
        public sealed class BeginSend
        {
            public BeginSend(ShardedMessage msg, IActorRef target, int batchSize)
            {
                Msg = msg;
                Target = target;
                BatchSize = batchSize;
            }

            public ShardedMessage Msg { get; }
            
            public IActorRef Target { get; }
            
            public int BatchSize { get; }
        }
        
        private int _remaining;

        private readonly TaskCompletionSource<bool> _tcs;

        public BulkSendActor(TaskCompletionSource<bool> tcs, int remaining)
        {
            _tcs = tcs;
            _remaining = remaining;

            Receive<BeginSend>(b =>
            {
                for (var i = 0; i < b.BatchSize; i++)
                {
                    b.Target.Tell(b.Msg);
                }
            });

            Receive<ShardedMessage>(s =>
            {
                if (--remaining > 0)
                {
                    Sender.Tell(s);
                }
                else
                {
                    Context.Stop(Self); // shut ourselves down
                    _tcs.TrySetResult(true);
                }
            });
        }
    }
    
    public sealed class ShardedMessage
    {
        public ShardedMessage(string entityId, int message)
        {
            EntityId = entityId;
            Message = message;
        }

        public string EntityId { get; }
            
        public int Message { get; }
    }
}