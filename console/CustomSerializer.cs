using System;
using System.Text;
using Akka.Actor;
using Akka.Serialization;
using Newtonsoft.Json;

namespace default_serializer_programmatic_override
{
    public class CustomSerializer : Serializer
    {
        public override int Identifier { get; } = 395;
        public override bool IncludeManifest { get; } = true;

        public CustomSerializer(ExtendedActorSystem system)
            : base(system)
        {
        }

        public override byte[] ToBinary(object obj)
        {
            system.Log.Info($"ToBinary({obj?.GetType().FullName})");
            var json = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(json);
            system.Log.Info($"ToBinary({obj?.GetType().FullName}): {json}");
            return bytes;
        }

        public override object FromBinary(byte[] bytes, Type type)
        {
            system.Log.Info($"FromBinary(byte[{bytes.Length}], {type.FullName})");
            var json = Encoding.UTF8.GetString(bytes);
            var obj = JsonConvert.DeserializeObject(json, type);
            system.Log.Info($"FromBinary(byte[{bytes.Length}], {type.FullName}): {json}");
            return obj;
        }

    }
}