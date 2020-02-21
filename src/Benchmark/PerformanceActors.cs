﻿using Akka;
using Akka.Actor;
using Akka.Persistence;

namespace Benchmark
{
    public sealed class Init
    {
        public static readonly Init Instance = new Init();
        private Init() { }
    }

    public sealed class Finish
    {
        public static readonly Finish Instance = new Finish();
        private Finish() { }
    }

    public sealed class Done
    {
        public static readonly Done Instance = new Done();
        private Done() { }
    }

    public sealed class Finished
    {
        public readonly long State;

        public Finished(long state)
        {
            State = state;
        }
    }

    public sealed class Store
    {
        public readonly int Value;

        public Store(int value)
        {
            Value = value;
        }
    }

    public sealed class Stored
    {
        public readonly int Value;

        public Stored(int value)
        {
            Value = value;
        }
    }

    public class PerformanceTestActor : PersistentActor
    {
        private long state;

        public PerformanceTestActor(string persistenceId)
        {
            PersistenceId = persistenceId;
        }

        public sealed override string PersistenceId { get; }

        protected override bool ReceiveRecover(object message) => message.Match()
            .With<Stored>(s => state += s.Value)
            .WasHandled;

        protected override bool ReceiveCommand(object message) => message.Match()
            .With<Store>(store =>
            {
                Persist(new Stored(store.Value), s =>
                {
                    state += s.Value;
                });
            })
            .With<Init>(_ =>
            {
                var sender = Sender;
                Persist(new Stored(0), s =>
                {
                    state += s.Value;
                    sender.Tell(Done.Instance);
                    Sender.Tell(Done.Instance);
                });
            })
            .With<Finish>(_ => Sender.Tell(new Finished(state)))
            .WasHandled;
    }
}