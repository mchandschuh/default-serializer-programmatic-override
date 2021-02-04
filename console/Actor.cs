using Akka.Actor;
using static default_serializer_programmatic_override.EpicProtocol;

namespace default_serializer_programmatic_override
{
    public class Actor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case Hello m:
                    Sender.Tell(new World(m.Message));
                    break;

                case World m:
                    Sender.Tell(new Hello(m.Message));
                    break;

                default:
                    Unhandled(message);
                    break;
            }
        }
    }
}