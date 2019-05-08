using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Linq;

namespace Hoard
{    
    /// <summary>
    /// Whisper interface
    /// </summary>
    public class WhisperService
    {
        private static readonly byte[] HexNibble = new byte[]
        {
            0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7,
            0x8, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string str)
        {
            int byteCount = str.Length >> 1;
            byte[] result = new byte[byteCount + (str.Length & 1)];
            for (int i = 0; i < byteCount; i++)
                result[i] = (byte)(HexNibble[str[i << 1] - 48] << 4 | HexNibble[str[(i << 1) + 1] - 48]);
            if ((str.Length & 1) != 0)
                result[byteCount] = (byte)HexNibble[str[str.Length - 1] - 48];
            return result;
        }

        /// <summary>
        /// Criteria
        /// </summary>
        public class SubscriptionCriteria
        {
            /// ID of symmetric key for message decryption
            public string symKeyID;

            /// ID of private (asymmetric) key for message decryption
            public string privateKeyID;

            /// (optional) Public key of the signature
            public string sig;

            /// (optional) Minimal PoW requirement for incoming messages
            public float minPow;

            /// (optional) Array of possible topics (or partial topics)
            public string[] topics;

            /// (optional) Indicates if this filter allows processing of direct peer-to-peer messages (which are not to be forwarded any further, because they might be expired). 
            /// This might be the case in some very rare cases, e.g. if you intend to communicate to MailServers, etc
            public bool allowP2P;

            /// <summary>
            /// Constructor
            /// </summary>
            public SubscriptionCriteria(string symKeyID, string privateKeyID, string sig, float minPow, string[] topics, bool allowP2P)
            {
                this.symKeyID = symKeyID;
                this.privateKeyID = privateKeyID;
                this.sig = sig;
                this.minPow = minPow;
                this.topics = topics;
                this.allowP2P = allowP2P;
            }
        }

        /// <summary>
        /// Message
        /// Either symKeyID or pubKey must be present. Can not be both
        /// </summary>
        public class MessageDesc
        {
            /// ID of symmetric key for message decryption
            public string symKeyID;

            /// public key for message encryption
            public string pubKey;

            /// (optional) ID of the signing key
            public string sig;

            /// Time-to-live in seconds
            public int ttl;

            /// 4 Bytes (mandatory when key is symmetric): Message topic
            public string topic;

            /// Message data to be encrypted (hex string)
            public string payload;

            /// (optional): Optional padding (byte array of arbitrary length)
            public string padding;

            /// Maximal time in seconds to be spent on proof of work
            public int powTime;

            /// Minimal PoW target required for this message
            public float powTarget;

            /// (optional): Optional peer ID (for peer-to-peer message only)
            public string targetPeer;

            /// <summary>
            /// Constructor
            /// </summary>
            public MessageDesc(string symKeyID, string pubKey, string sig, int ttl, string topic, byte[] data, string padding, int powTime, float powTarget, string targetPeer)
            {
                this.symKeyID = symKeyID;
                this.pubKey = pubKey;
                this.sig = sig;
                this.ttl = ttl;
                this.topic = topic;
                this.payload = "0x" + BitConverter.ToString(data).Replace("-", string.Empty);
                this.padding = padding;
                this.powTime = powTime;
                this.powTarget = powTarget;
                this.targetPeer = targetPeer;
            }
        }

        /// <summary>
        /// ReceivedData
        /// Either symKeyID or pubKey must be present. Can not be both
        /// </summary>
        public class ReceivedData
        {
            /// Public key who signed this message.
            public string sig;

            /// Time-to-live in seconds
            public int ttl;

            /// Unix timestamp of the message generation
            public float timestamp;

            /// 4 Bytes: Message topic
            public string topic;

            /// Decrypted payload (hex string)
            public string payload;

            /// Optional padding (byte array of arbitrary length)
            public string padding;

            /// Proof of work value
            public float pow;

            /// Hash of the enveloped message
            public string hash;

            /// Recipient public key
            public string recipientPublicKey;

            static ReceivedData()
            {
                new ReceivedData();//to force code generators create proper code for reflection (JSON deserialize)
            }

            /// Returns decoded message
            public byte[] GetDecodedMessage()
            {
                return HexStringToByteArray(payload.Substring(2));
            }
        }

        private ClientWebSocket WhisperClient = null;
        private TimeSpan TimeOut = TimeSpan.FromSeconds(3);
        private string Url = null;
        private string Error;

        static readonly private string JsonVersion = "2.0";
        static readonly private int JsonId = 1;

        /// <summary>
        /// Message timeout
        /// </summary>
        static readonly public int MAX_WAIT_TIME_IN_MS = 120000;

        /// <summary>
        /// Constructor
        /// </summary>
        public WhisperService(string url)
        {
            Url = url;
            WhisperClient = new ClientWebSocket();
        }

        /// <summary>
        /// Establishes connection
        /// </summary>
        public async Task<bool> Connect()
        {
            using (var cts = new CancellationTokenSource(TimeOut))
            {
                await WhisperClient.ConnectAsync(new Uri(Url), cts.Token);
            }

            if (WhisperClient.State != WebSocketState.Open)
            {
                ErrorCallbackProvider.ReportError("Cannot connect to destination host: "+Url);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Closes connection
        /// </summary>
        public async Task Close()
        {
            await WhisperClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        private async Task<T> BuildAndSendRequest<T>(string function, JArray additionalParams, T defaultValue)
        {
            try
            {
                if (WhisperClient.State != WebSocketState.Open)
                {
                    ErrorCallbackProvider.ReportError("Whisper connection error!");
                    throw new WebException("Whisper connection error!");
                }

                JObject jobj = new JObject();
                jobj.Add("jsonrpc", JsonVersion);
                jobj.Add("method", function);
                if (additionalParams != null)
                {
                    jobj.Add("params", additionalParams);
                }
                jobj.Add("id", JsonId);
                using (var cts = new CancellationTokenSource(TimeOut))
                {
                    await WhisperClient.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(jobj.ToString())), WebSocketMessageType.Text, true, cts.Token);

                    //read in 1K chunks
                    WebSocketReceiveResult rcvResult = null;
                    System.IO.MemoryStream msgBytes = new System.IO.MemoryStream(1024);
                    do
                    {
                        var rcvBytes = new byte[1024];
                        var rcvBuffer = new ArraySegment<byte>(rcvBytes);

                        rcvResult = await WhisperClient.ReceiveAsync(rcvBuffer, cts.Token);

                        msgBytes.Write(rcvBuffer.Array, rcvBuffer.Offset, rcvResult.Count);
                    } while (!rcvResult.EndOfMessage);

                    if (rcvResult.MessageType == WebSocketMessageType.Binary)
                    {
                        ErrorCallbackProvider.ReportError("Expected text, received binary data: " + msgBytes);
                        return defaultValue;
                    }
                    else if (rcvResult.MessageType == WebSocketMessageType.Text)
                    {
                        string jsonMsg = Encoding.UTF8.GetString(msgBytes.ToArray());

                        JToken message = null;
                        JObject json = JObject.Parse(jsonMsg);
                        json.TryGetValue("error", out message);
                        if (message != null)
                        {
                            Error = message.ToString();
                        }
                        else
                        {
                            JToken method = null;
                            json.TryGetValue("method", out method);
                            if ((method != null) && (method.ToString() == "shh_subscription"))
                            {
                                JToken prms = null;
                                json.TryGetValue("params", out prms);
                                if (prms != null)
                                {
                                    JObject jsonParams = prms.ToObject<JObject>();
                                    JToken result = "";
                                    jsonParams.TryGetValue("result", out result);
                                    if (result != null)
                                    {
                                        if (message.Type == JTokenType.Array || message.Type == JTokenType.Object)
                                            return JsonConvert.DeserializeObject<T>(result.ToString());
                                        return result.Value<T>();
                                    }
                                }
                            }
                            else
                            {
                                json.TryGetValue("result", out message);
                                if (message != null)
                                {
                                    if (message.Type == JTokenType.Array || message.Type == JTokenType.Object)
                                        return JsonConvert.DeserializeObject<T>(message.ToString());
                                    return message.Value<T>();
                                }
                            }
                        }
                    }
                    ErrorCallbackProvider.ReportError("Conection was closed");
                    return defaultValue;
                };
            }
            catch (WebSocketException ex)
            {
                ErrorCallbackProvider.ReportError("Whisper unknown exception:\n" + ex.Message);
                return defaultValue;
            }
            catch (Exception ex)
            {
                ErrorCallbackProvider.ReportError("Unknown exception:\n" + ex.Message);
                return defaultValue;
            }
        }

            /// <summary>
            /// Check connection
            /// </summary>
            /// <returns></returns>
            public async Task<bool> CheckConnection()
        {
            return !string.IsNullOrEmpty(await BuildAndSendRequest<string>("shh_info", null, string.Empty));
        }

        /// <summary>
        /// Returns the current version number
        /// </summary>
        /// <returns>version number</returns>
        public async Task<string> CheckVersion()
        {
            return await BuildAndSendRequest<string>("shh_version", null, string.Empty);
        }

        /// <summary>
        /// Sets the maximal message size allowed by this node. Incoming and outgoing messages with a larger size will be rejected. 
        /// Whisper message size can never exceed the limit imposed by the underlying P2P protocol (10 Mb)
        /// </summary>
        /// <param name="maxMessageSize">Message size in bytes</param>
        /// <returns></returns>
        public async Task<bool> SetMaxMessageSize(int maxMessageSize)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(maxMessageSize);
            return await BuildAndSendRequest<bool>("shh_setMaxMessageSize", jarrayObj, false);
        }

        /// <summary>
        /// Sets the minimal PoW required by this node
        /// </summary>
        /// <param name="minPov">The new PoW requirement</param>
        /// <returns></returns>
        public async Task<bool> SetMinPov(float minPov)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(minPov);
            return await BuildAndSendRequest<bool>("shh_setMinPoW", jarrayObj, false);
        }

        /// <summary>
        /// Marks specific peer trusted, which will allow it to send historic (expired) messages
        /// </summary>
        /// <param name="enode">Enode of the trusted peer</param>
        /// <returns></returns>
        public async Task<bool> MarkTrustedPeer(string enode)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(enode);
            return await BuildAndSendRequest<bool>("shh_markTrustedPeer", jarrayObj, false);
        }

        /// <summary>
        /// Generates a new public and private key pair for message decryption and encryption
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateKeyPair()
        {
            return await BuildAndSendRequest<string>("shh_newKeyPair", null, string.Empty);
        }

        /// <summary>
        /// Stores the key pair, and returns its ID
        /// </summary>
        /// <param name="privKey">private key as HEX bytes</param>
        /// <returns></returns>
        public async Task<bool> AddPrivateKey(string privKey)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(privKey);
            return !string.IsNullOrEmpty(await BuildAndSendRequest("shh_addPrivateKey", jarrayObj, string.Empty));
        }

        /// <summary>
        /// Deletes the specifies key if it exists
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns></returns>
        public async Task<bool> DeleteKeyPair(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_deleteKeyPair", jarrayObj, false);
        }

        /// <summary>
        /// Checks if the whisper node has a private key of a key pair matching the given ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns></returns>
        public async Task<bool> HasKeyPair(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_hasKeyPair", jarrayObj, false);
        }

        /// <summary>
        /// Returns the public key for identity ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns>public key</returns>
        public async Task<string> GetPublicKey(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_getPublicKey", jarrayObj, string.Empty);
        }

        /// <summary>
        /// Returns the private key for identity ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns>private key</returns>
        public async Task<string> GetPrivateKey(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_getPrivateKey", jarrayObj, string.Empty);
        }

        /// <summary>
        /// Generates a random symmetric key and stores it under an ID, which is then returned. 
        /// Can be used encrypting and decrypting messages where the key is known to both parties
        /// </summary>
        /// <returns>Key ID</returns>
        public async Task<string> GenerateSymetricKey()
        {
            return await BuildAndSendRequest("shh_newSymKey", null, string.Empty);
        }

        /// <summary>
        /// Stores the key, and returns its ID
        /// </summary>
        /// <param name="rawSymKey">The raw key for symmetric encryption as HEX bytes</param>
        /// <returns>Key ID</returns>
        public async Task<string> AddSymetricKey(string rawSymKey)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(rawSymKey);
            return await BuildAndSendRequest("shh_addSymKey", jarrayObj, string.Empty);
        }

        /// <summary>
        /// Generates the key from password, stores it, and returns its ID
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Key ID</returns>
        public async Task<string> GenerateSymetricKeyFromPassword(string password)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(password);
            return await BuildAndSendRequest("shh_generateSymKeyFromPassword", jarrayObj, string.Empty);
        }

        /// <summary>
        /// Returns true if there is a key associated with the name string. Otherwise, returns false
        /// </summary>
        /// <param name="keyId">key ID</param>
        /// <returns></returns>
        public async Task<bool> HasSymetricKey(string keyId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            return await BuildAndSendRequest("shh_hasSymKey", jarrayObj, false);
        }

        /// <summary>
        /// Returns the symmetric key associated with the given ID
        /// </summary>
        /// <param name="keyId">key ID</param>
        /// <returns>symetric key</returns>
        public async Task<string> GetSymetricKey(string keyId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            return await BuildAndSendRequest("shh_getSymKey", jarrayObj, string.Empty);
        }

        /// <summary>
        /// Deletes the key associated with the name string if it exists
        /// </summary>
        /// <param name="keyId">key Id</param>
        /// <returns></returns>
        public async Task<bool> DeleteSymetricKey(string keyId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            return await BuildAndSendRequest("shh_deleteSymKey", jarrayObj, false);
        }

        /// <summary>
        /// Creates and registers a new subscription to receive notifications for inbound whisper messages. 
        /// Returns the ID of the newly created subscription.
        /// Either symKeyID or privateKeyID must be present. Can not be both
        /// </summary>
        /// <param name="filters">message filters</param>
        /// <param name="id">identifier of function call. In case of Whisper must contain the value "messages"
        /// This might be the case in some very rare cases, e.g. if you intend to communicate to MailServers, etc</param>
        /// <returns>subscription id</returns>
        public async Task<string> Subscribe(SubscriptionCriteria filters, string id = "messages")
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(id);
            var outObject = (JObject)JToken.FromObject(filters);
            jarrayObj.Add(outObject);
            return await BuildAndSendRequest("shh_subscribe", jarrayObj, string.Empty);
        }

        /// <summary>
        /// Cancels and removes an existing subscription
        /// </summary>
        /// <param name="subscriptionId">subscription ID</param>
        /// <returns></returns>
        public async Task<bool> Unsubscribe(string subscriptionId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(subscriptionId);
            return await BuildAndSendRequest("shh_unsubscribe", jarrayObj, false);
        }

        /// <summary>
        /// Create a new filter within the node. This filter can be used to poll for new messages that match the set of criteria
        /// </summary>
        /// <param name="filters">Message filters</param>
        /// <returns>filter identifier</returns>
        public async Task<string> CreateNewMessageFilter(SubscriptionCriteria filters)
        {
            JArray jarrayObj = new JArray();
            var outObject = (JObject)JToken.FromObject(filters);
            jarrayObj.Add(outObject);
            return await BuildAndSendRequest("shh_newMessageFilter", jarrayObj, string.Empty);
        }

        /// <summary>
        /// Uninstall a message filter in the node
        /// </summary>
        /// <param name="filterIdentifier">filter identifier as returned when the filter was created</param>
        /// <returns></returns>
        public async Task<bool> DeleteMessageFilter(string filterIdentifier)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(filterIdentifier);
            return await BuildAndSendRequest("shh_deleteMessageFilter", jarrayObj, false);
        }

        /// <summary>
        /// Retrieve messages that match the filter criteria and are received between the last time this function was called and now
        /// </summary>
        /// <param name="filterIdentifier">ID of filter that was created with shh_newMessageFilter</param>
        /// <returns>Array of messages</returns>
        public async Task<List<ReceivedData>> ReceiveMessage(string filterIdentifier)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(filterIdentifier);
            return await BuildAndSendRequest<List<ReceivedData>>("shh_getFilterMessages", jarrayObj, null);
        }

        /// <summary>
        /// Creates a whisper message and injects it into the network for distribution
        /// </summary>
        /// <param name="msg">Message</param>
        /// <returns>message hash</returns>
        public async Task<string> SendMessage(MessageDesc msg)
        {
            JArray jarrayObj = new JArray();
            var outObject = (JObject)JToken.FromObject(msg);
            jarrayObj.Add(outObject);
            return await BuildAndSendRequest("shh_post", jarrayObj, string.Empty);
        }  
        
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<ReceivedData> GetReceivedMessages()
        {
            return null;
            //WhisperClient.ReceiveAsync()
            //return ReceivedMessagesQueue;
        }
    }
}
