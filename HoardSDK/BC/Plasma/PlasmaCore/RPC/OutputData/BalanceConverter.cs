using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PlasmaCore.RPC.OutputData.Balance
{
    internal class BalanceConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);

            BalanceData result = null;
            if (jObject["amount"] != null)
            {
                if (jObject["amount"].Type == JTokenType.Array)
                {
                    result = new NFCBalanceData();
                }
                else if (jObject["amount"].Type == JTokenType.Integer)
                {
                    result = new FCBalanceData();
                }
                else
                {
                    //TODO not supported balance format
                    throw new NotSupportedException();
                }
            }
            else
            {
                //TODO not supported balance format
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
