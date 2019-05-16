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
using System.Diagnostics;

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
        /// Called when new subscription message is translated
        /// </summary>
        public event Action<SubscriptionResponse> OnSubscriptionMessage;

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
        public class SubscriptionResponse
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

            static SubscriptionResponse()
            {
                new SubscriptionResponse();//to force code generators create proper code for reflection (JSON deserialize)
            }

            /// Returns decoded message
            public byte[] GetDecodedMessage()
            {
                return HexStringToByteArray(payload.Substring(2));
            }
        }

        private IWebSocketProvider WebSocketProvider = null;
        private string Error;

        /// <summary>
        /// Returns true if it is connected to host
        /// </summary>
        public bool IsConnected { get { return (WebSocketProvider != null) && WebSocketProvider.IsConnectionOpen(); } }        
        
        private ConcurrentQueue<SubscriptionResponse> SubscriptionMessagesQueue = new ConcurrentQueue<SubscriptionResponse>();

        static readonly private string JsonVersion = "2.0";
        private int JsonId = 0;

        /// <summary>
        /// Message timeout
        /// </summary>
        static readonly public int MAX_WAIT_TIME_IN_MS = 120000;

        /// <summary>
        /// Constructor
        /// </summary>
        public WhisperService(IWebSocketProvider webSocketProvider)
        {
            WebSocketProvider = webSocketProvider;
            Debug.Assert(WebSocketProvider != null);
        }

        /// <summary>
        /// Establishes connection
        /// </summary>
        public async Task<bool> Connect(CancellationToken token)
        {
            return await WebSocketProvider.Connect(token);
        }

        /// <summary>
        /// Closes connection
        /// </summary>
        public async Task Close()
        {
            await WebSocketProvider.Close();
        }

        private async Task<SubscriptionResponse> ReceiveSubscriptionResponse(CancellationToken token)
        {
            if (IsConnected == false)
            {
                ErrorCallbackProvider.ReportError("Whisper connection error!");
                throw new WebException("Whisper connection error!");
            }

            while (true)
            {
                byte[] msgBytes = await WebSocketProvider.Receive(token);

                //we are skipping all messages that are not subscriptions
                if (msgBytes != null)
                {
                    string jsonMsg = Encoding.UTF8.GetString(msgBytes);
                    ErrorCallbackProvider.ReportInfo(jsonMsg);

                    JToken message = null;
                    JObject json = JObject.Parse(jsonMsg);
                    json.TryGetValue("error", out message);
                    if (message != null)
                    {
                        Error = message.ToString();
                        ErrorCallbackProvider.ReportError("Received error from Whisper service: " + Error);
                    }
                    else
                    {
                        json.TryGetValue("method", out JToken method);
                        if ((method != null) && (method.ToString() == "shh_subscription"))
                        {
                            JToken prms = null;
                            json.TryGetValue("params", out prms);
                            if (prms != null)
                            {
                                JObject jsonParams = prms.ToObject<JObject>();
                                jsonParams.TryGetValue("result", out message);
                                if (message != null)
                                {
                                    return JsonConvert.DeserializeObject<SubscriptionResponse>(message.ToString());
                                }
                            }
                        }
                        else
                        {
                            ErrorCallbackProvider.ReportWarning("Expected notification but got: "+jsonMsg);
                        }
                    }
                }
            }
        }

        private async Task<T> ReceiveResponse<T>(int reqId, T defaultValue, CancellationToken ctoken)
        {
            if (IsConnected == false)
            {
                ErrorCallbackProvider.ReportError("Whisper connection error!");
                throw new WebException("Whisper connection error!");
            }

            //wait for requested response (or timeout)
            while (true)
            {
                byte[] msgBytes = await WebSocketProvider.Receive(ctoken);
                
                if (msgBytes != null)
                {
                    string jsonMsg = Encoding.UTF8.GetString(msgBytes);
                    ErrorCallbackProvider.ReportInfo(jsonMsg);

                    JObject json = JObject.Parse(jsonMsg);
                    json.TryGetValue("error", out JToken error);
                    if (error != null)
                    {
                        Error = error.ToString();
                        ErrorCallbackProvider.ReportError("Received error from Whisper service: " + Error);
                        return defaultValue;
                    }
                    else
                    {
                        json.TryGetValue("result", out JToken result);
                        if (result != null)
                        {
                            json.TryGetValue("id", out JToken respId);
                            if (respId.Value<int>() == reqId)
                            {
                                if (result.Type == JTokenType.Array || result.Type == JTokenType.Object)
                                    return JsonConvert.DeserializeObject<T>(result.ToString());
                                return result.Value<T>();
                            }
                            else
                            {
                                ErrorCallbackProvider.ReportError("Waiting for response with ID: " + reqId + " but got " + respId.Value<int>());
                            }
                        }
                        else
                        {
                            json.TryGetValue("method", out JToken method);
                            if ((method != null) && (method.ToString() == "shh_subscription"))
                            {
                                json.TryGetValue("params", out JToken prms);
                                if (prms != null)
                                {
                                    JObject jsonParams = prms.ToObject<JObject>();
                                    jsonParams.TryGetValue("result", out JToken message);
                                    if (message != null)
                                    {
                                        OnSubscriptionMessage?.Invoke(JsonConvert.DeserializeObject<SubscriptionResponse>(message.ToString()));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    ErrorCallbackProvider.ReportWarning("Conection was closed!");
                    return defaultValue;
                }
            }
        }

        private async Task<T> BuildAndSendRequest<T>(string function, JArray additionalParams, T defaultValue, CancellationToken ctoken)
        {
            try
            {
                if (IsConnected == false)
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
                jobj.Add("id", ++JsonId);
                await WebSocketProvider.Send(Encoding.UTF8.GetBytes(jobj.ToString()), ctoken);
                return await ReceiveResponse<T>(JsonId, defaultValue, ctoken);
            }
            catch (WebSocketException ex)
            {
                ErrorCallbackProvider.ReportError("Whisper unknown exception:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                ErrorCallbackProvider.ReportError("Unknown exception:\n" + ex.Message);
            }
            return defaultValue;
        }

        /// <summary>
        /// Check connection
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckConnection(CancellationToken ctoken)
        {
            return !string.IsNullOrEmpty(await BuildAndSendRequest<string>("shh_info", null, string.Empty, ctoken));
        }

        /// <summary>
        /// Returns the current version number
        /// </summary>
        /// <returns>version number</returns>
        public async Task<string> CheckVersion(CancellationToken ctoken)
        {
            return await BuildAndSendRequest<string>("shh_version", null, string.Empty, ctoken);
        }

        /// <summary>
        /// Sets the maximal message size allowed by this node. Incoming and outgoing messages with a larger size will be rejected. 
        /// Whisper message size can never exceed the limit imposed by the underlying P2P protocol (10 Mb)
        /// </summary>
        /// <param name="maxMessageSize">Message size in bytes</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> SetMaxMessageSize(int maxMessageSize, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(maxMessageSize);
            return await BuildAndSendRequest<bool>("shh_setMaxMessageSize", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Sets the minimal PoW required by this node
        /// </summary>
        /// <param name="minPov">The new PoW requirement</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> SetMinPov(float minPov, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(minPov);
            return await BuildAndSendRequest<bool>("shh_setMinPoW", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Marks specific peer trusted, which will allow it to send historic (expired) messages
        /// </summary>
        /// <param name="enode">Enode of the trusted peer</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> MarkTrustedPeer(string enode, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(enode);
            return await BuildAndSendRequest<bool>("shh_markTrustedPeer", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Generates a new public and private key pair for message decryption and encryption
        /// </summary>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<string> GenerateKeyPair(CancellationToken ctoken)
        {
            return await BuildAndSendRequest<string>("shh_newKeyPair", null, string.Empty, ctoken);
        }

        /// <summary>
        /// Stores the key pair, and returns its ID
        /// </summary>
        /// <param name="privKey">private key as HEX bytes</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> AddPrivateKey(string privKey, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(privKey);
            return !string.IsNullOrEmpty(await BuildAndSendRequest("shh_addPrivateKey", jarrayObj, string.Empty, ctoken));
        }

        /// <summary>
        /// Deletes the specifies key if it exists
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> DeleteKeyPair(string keyPairId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_deleteKeyPair", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Checks if the whisper node has a private key of a key pair matching the given ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> HasKeyPair(string keyPairId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_hasKeyPair", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Returns the public key for identity ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>public key</returns>
        public async Task<string> GetPublicKey(string keyPairId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_getPublicKey", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Returns the private key for identity ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>private key</returns>
        public async Task<string> GetPrivateKey(string keyPairId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            return await BuildAndSendRequest("shh_getPrivateKey", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Generates a random symmetric key and stores it under an ID, which is then returned. 
        /// Can be used encrypting and decrypting messages where the key is known to both parties
        /// </summary>
        /// <returns>Key ID</returns>
        public async Task<string> GenerateSymetricKey(CancellationToken ctoken)
        {
            return await BuildAndSendRequest("shh_newSymKey", null, string.Empty, ctoken);
        }

        /// <summary>
        /// Stores the key, and returns its ID
        /// </summary>
        /// <param name="rawSymKey">The raw key for symmetric encryption as HEX bytes</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>Key ID</returns>
        public async Task<string> AddSymetricKey(string rawSymKey, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(rawSymKey);
            return await BuildAndSendRequest("shh_addSymKey", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Generates the key from password, stores it, and returns its ID
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>Key ID</returns>
        public async Task<string> GenerateSymetricKeyFromPassword(string password, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(password);
            return await BuildAndSendRequest("shh_generateSymKeyFromPassword", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Returns true if there is a key associated with the name string. Otherwise, returns false
        /// </summary>
        /// <param name="keyId">key ID</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> HasSymetricKey(string keyId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            return await BuildAndSendRequest("shh_hasSymKey", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Returns the symmetric key associated with the given ID
        /// </summary>
        /// <param name="keyId">key ID</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>symetric key</returns>
        public async Task<string> GetSymetricKey(string keyId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            return await BuildAndSendRequest("shh_getSymKey", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Deletes the key associated with the name string if it exists
        /// </summary>
        /// <param name="keyId">key Id</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> DeleteSymetricKey(string keyId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            return await BuildAndSendRequest("shh_deleteSymKey", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Creates and registers a new subscription to receive notifications for inbound whisper messages. 
        /// Returns the ID of the newly created subscription.
        /// Either symKeyID or privateKeyID must be present. Can not be both
        /// </summary>
        /// <param name="filters">message filters</param>
        /// <param name="id">identifier of function call. In case of Whisper must contain the value "messages"
        /// This might be the case in some very rare cases, e.g. if you intend to communicate to MailServers, etc</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>subscription id</returns>
        public async Task<string> Subscribe(SubscriptionCriteria filters, CancellationToken ctoken, string id = "messages")
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(id);
            var outObject = (JObject)JToken.FromObject(filters);
            jarrayObj.Add(outObject);
            return await BuildAndSendRequest("shh_subscribe", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Cancels and removes an existing subscription
        /// </summary>
        /// <param name="subscriptionId">subscription ID</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> Unsubscribe(string subscriptionId, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(subscriptionId);
            return await BuildAndSendRequest("shh_unsubscribe", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Create a new filter within the node. This filter can be used to poll for new messages that match the set of criteria
        /// </summary>
        /// <param name="filters">Message filters</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>filter identifier</returns>
        public async Task<string> CreateNewMessageFilter(SubscriptionCriteria filters, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            var outObject = (JObject)JToken.FromObject(filters);
            jarrayObj.Add(outObject);
            return await BuildAndSendRequest("shh_newMessageFilter", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Uninstall a message filter in the node
        /// </summary>
        /// <param name="filterIdentifier">filter identifier as returned when the filter was created</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> DeleteMessageFilter(string filterIdentifier, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(filterIdentifier);
            return await BuildAndSendRequest("shh_deleteMessageFilter", jarrayObj, false, ctoken);
        }

        /// <summary>
        /// Retrieve messages that match the filter criteria and are received between the last time this function was called and now
        /// </summary>
        /// <param name="filterIdentifier">ID of filter that was created with shh_newMessageFilter</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>Array of messages</returns>
        public async Task<List<SubscriptionResponse>> ReceiveMessage(string filterIdentifier, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(filterIdentifier);
            return await BuildAndSendRequest<List<SubscriptionResponse>>("shh_getFilterMessages", jarrayObj, null, ctoken);
        }

        /// <summary>
        /// Creates a whisper message and injects it into the network for distribution
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>message hash</returns>
        public async Task<string> SendMessage(MessageDesc msg, CancellationToken ctoken)
        {
            JArray jarrayObj = new JArray();
            var outObject = (JObject)JToken.FromObject(msg);
            jarrayObj.Add(outObject);
            return await BuildAndSendRequest("shh_post", jarrayObj, string.Empty, ctoken);
        }

        /// <summary>
        /// Returns one SubscriptionResponse message sent from Whisper
        /// </summary>
        /// <param name="ctoken">cancellation token</param>
        public async Task<SubscriptionResponse> ReceiveMessages(CancellationToken ctoken)
        {
            return await ReceiveSubscriptionResponse(ctoken);
        }
    }
}
