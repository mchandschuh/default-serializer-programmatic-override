using System;
using System.Collections.Immutable;
using System.Threading;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.Serialization;
using static default_serializer_programmatic_override.EpicProtocol;

namespace default_serializer_programmatic_override
{
    public class Program
    {
        static void Main(string[] args)
        {
            // setup declaring our custom serializer to override the default typeof(object) newtonsoft registration
            var serializationSetup = SerializationSetup.Create(system =>
                ImmutableHashSet<SerializerDetails>.Empty.Add(
                    SerializerDetails.Create("json-custom", new CustomSerializer(system),
                        ImmutableHashSet<Type>.Empty
                            // this should make all my messages use CustomSerializer
                            .Add(typeof(object))
                            // this is provided to show CustomSerializer gets called,
                            // i.e, everything appears to be configured correctly
                            .Add(typeof(World))
                    )
                )
            );

            // we just want to force serialization so we don't need to set up remote
            // the goal of the programmatic setup is so apps don't need to register the serializer
            var hocon = ConfigurationFactory.ParseString(@"
akka {
    ""loglevel"": ""DEBUG"",
    ""stdout-loglevel"": ""DEBUG"",
    actor {
        ""serialize-messages"": ""on"",
        ""debug"": {
            ""receive"": ""on"",
            ""lifecycle"": ""on"",
            ""unhandled"": ""on"",
            ""event-stream"": ""on""
        }
    }
}
");
            // I'm not sure why 'serialize-messages' isn't triggering my serializer, not even for
            // my 'World' type. Inspecting system.Serialization._serializerMap reveals the following:
            // [object] = NewtonsoftJsonSerializer
            // [World]  = CustomSerializer
            // [byte[]] = ByteArraySerializer
            // * I thought it might have been dependent on the ordering of ActorSystemSetup.Create(params Setup[] setups)
            //   but I observe the same behavior regardless of the setup ordering.
            var bootstrapSetup = BootstrapSetup.Create().WithConfig(hocon);
            var systemSetup = ActorSystemSetup.Create(serializationSetup, bootstrapSetup);
            var system = ActorSystem.Create("system", systemSetup);

            // uncommenting this line and the custom serializer is invoked
            // really bizarre since there is a mapping for typeof(World) but w/out this next line it's not invoked on that type either.
            //system.Serialization.AddSerializationMap(typeof(object), new CustomSerializer(system.Serialization.System));

            var actor = system.ActorOf(Props.Create(() => new Actor()), "actor");
            var response = actor.Ask<World>(new Hello("world!")).GetAwaiter().GetResult();
            system.Log.Info($"Response: {response.Message}");

            Thread.Sleep(TimeSpan.FromSeconds(20));

            system.Dispose();
        }
    }
}
