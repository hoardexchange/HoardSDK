using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PlasmaCore.UTXO
{
    internal class UTXOConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);

            UTXOData result = null;
            if (jObject["amount"] != null)
                result = new FCUTXOData();
            else
            {
                //TODO not supported utxo format
                throw new NotSupportedException();
            }

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
