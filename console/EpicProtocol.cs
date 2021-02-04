namespace default_serializer_programmatic_override
{
    public static class EpicProtocol
    {
        public interface IEpic { }

        public sealed class Hello
        {
            public string Message { get; }
            public Hello(string messag) => Message = messag;
        }

        public sealed class World
        {
            public string Message { get; }
            public World(string message) => Message = message;
        }
    }
}