using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using WebSocketSharp;

namespace Hoard
{
    /// <summary>
    /// Whisper interface
    /// </summary>
    public class WhisperService
    {
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

            /// Message data to be encrypted
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

            /// Decrypted payload
            public string payload;

            /// Optional padding (byte array of arbitrary length)
            public string padding;

            /// Proof of work value
            public float pow;

            /// Hash of the enveloped message
            public string hash;

            /// recipient public key
            public string recipientPublicKey;
        }

        private string Url = "";

        static private string JsonVersion = "2.0";
        static private int JsonId = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public WhisperService(string url)
        {
            Url = url;
        }
        
        private bool BuildAndSendRequest(string function, JArray additionalParams, out string outMessage)
        {
            try
            {
                HttpWebRequest client = (HttpWebRequest)WebRequest.Create(Url);
                client.ContentType = "application/json";
                client.Method = "POST";
                using (var streamWriter = new StreamWriter(client.GetRequestStream()))
                {
                    JObject jobj = new JObject();
                    jobj.Add("jsonrpc", JsonVersion);
                    jobj.Add("method", function);
                    if (additionalParams != null)
                    {
                        jobj.Add("params", additionalParams);
                    }
                    jobj.Add("id", JsonId);
                    streamWriter.Write(jobj.ToString());
                }

                var httpResponse = (HttpWebResponse)client.GetResponse();
                Debug.Assert(httpResponse.StatusCode == HttpStatusCode.OK);

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    JToken message = null;
                    var result = streamReader.ReadToEnd();
                    JObject json = JObject.Parse(result);
                    json.TryGetValue("error", out message);
                    if (message != null)
                    {
                        outMessage = message.ToString();
                        return false;
                    }
                    else
                    {
                        json.TryGetValue("result", out message);
                        if (message != null)
                        {
                            outMessage = message.ToString();
                            return true;
                        }
                    }
                }
                outMessage = "Unknown error";
                return false;
            }
            catch (Exception e)
            {
                outMessage = "Unknown exception";
                return false;
            }
        }

        /// <summary>
        /// Check connection
        /// </summary>
        /// <returns></returns>
        public bool CheckConnection()
        {
            string outMessage = "";
            return BuildAndSendRequest("shh_info", null, out outMessage);
        }

        /// <summary>
        /// Returns the current version number
        /// </summary>
        /// <returns>version number</returns>
        public string CheckVersion()
        {
            string outMessage = "";
            if (BuildAndSendRequest("shh_version", null, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Sets the maximal message size allowed by this node. Incoming and outgoing messages with a larger size will be rejected. 
        /// Whisper message size can never exceed the limit imposed by the underlying P2P protocol (10 Mb)
        /// </summary>
        /// <param name="maxMessageSize">Message size in bytes</param>
        /// <returns></returns>
        public bool SetMaxMessageSize(int maxMessageSize)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(maxMessageSize);
            string outMessage = "";
            return BuildAndSendRequest("shh_setMaxMessageSize", null, out outMessage);
        }

        /// <summary>
        /// Sets the minimal PoW required by this node
        /// </summary>
        /// <param name="minPov">The new PoW requirement</param>
        /// <returns></returns>
        public bool SetMinPov(float minPov)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(minPov);
            string outMessage = "";
            return BuildAndSendRequest("shh_setMinPoW", null, out outMessage);
        }

        /// <summary>
        /// Marks specific peer trusted, which will allow it to send historic (expired) messages
        /// </summary>
        /// <param name="enode">Enode of the trusted peer</param>
        /// <returns></returns>
        public bool MarkTrustedPeer(string enode)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(enode);
            string outMessage = "";
            return BuildAndSendRequest("shh_markTrustedPeer", null, out outMessage);
        }

        /// <summary>
        /// Generates a new public and private key pair for message decryption and encryption
        /// </summary>
        /// <returns></returns>
        public string GenerateKeyPair()
        {
            string outMessage = "";
            if (BuildAndSendRequest("shh_newKeyPair", null, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Stores the key pair, and returns its ID
        /// </summary>
        /// <param name="privKey">private key as HEX bytes</param>
        /// <returns></returns>
        public bool AddPrivateKey(string privKey)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(privKey);
            string outMessage = "";
            return BuildAndSendRequest("shh_addPrivateKey", null, out outMessage);
        }

        /// <summary>
        /// Deletes the specifies key if it exists
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns></returns>
        public bool DeleteKeyPair(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            string outMessage = "";
            return BuildAndSendRequest("shh_deleteKeyPair", jarrayObj, out outMessage);
        }

        /// <summary>
        /// Checks if the whisper node has a private key of a key pair matching the given ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns></returns>
        public bool HasKeyPair(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            string outMessage = "";
            if (BuildAndSendRequest("shh_hasKeyPair", jarrayObj, out outMessage))
            {
                return (outMessage.ToLower() == "true") ? true : false;
            }
            return false;
        }

        /// <summary>
        /// Returns the public key for identity ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns>public key</returns>
        public string GetPublicKey(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            string outMessage = "";
            if (BuildAndSendRequest("shh_getPublicKey", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Returns the private key for identity ID
        /// </summary>
        /// <param name="keyPairId">ID of key pair</param>
        /// <returns>private key</returns>
        public string GetPrivateKey(string keyPairId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyPairId);
            string outMessage = "";
            if (BuildAndSendRequest("shh_getPrivateKey", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Generates a random symmetric key and stores it under an ID, which is then returned. 
        /// Can be used encrypting and decrypting messages where the key is known to both parties
        /// </summary>
        /// <returns>Key ID</returns>
        public string GenerateSymetricKey()
        {
            string outMessage = "";
            if (BuildAndSendRequest("shh_newSymKey", null, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Stores the key, and returns its ID
        /// </summary>
        /// <param name="rawSymKey">The raw key for symmetric encryption as HEX bytes</param>
        /// <returns>Key ID</returns>
        public string AddSymetricKey(string rawSymKey)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(rawSymKey);
            string outMessage = "";
            if (BuildAndSendRequest("shh_addSymKey", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Generates the key from password, stores it, and returns its ID
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Key ID</returns>
        public string GenerateSymetricKeyFromPassword(string password)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(password);
            string outMessage = "";
            if (BuildAndSendRequest("shh_generateSymKeyFromPassword", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Returns true if there is a key associated with the name string. Otherwise, returns false
        /// </summary>
        /// <param name="keyId">key ID</param>
        /// <returns></returns>
        public bool HasSymetricKey(string keyId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            string outMessage = "";
            if (BuildAndSendRequest("shh_hasSymKey", jarrayObj, out outMessage))
            {
                return (outMessage.ToLower() == "true") ? true : false;
            }
            return false;
        }

        /// <summary>
        /// Returns the symmetric key associated with the given ID
        /// </summary>
        /// <param name="keyId">key ID</param>
        /// <returns>symetric key</returns>
        public string GetSymetricKey(string keyId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            string outMessage = "";
            if (BuildAndSendRequest("shh_getSymKey", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Deletes the key associated with the name string if it exists
        /// </summary>
        /// <param name="keyId">key Id</param>
        /// <returns></returns>
        public bool DeleteSymetricKey(string keyId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(keyId);
            string outMessage = "";
            return BuildAndSendRequest("shh_deleteSymKey", jarrayObj, out outMessage);
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
        public string Subscribe(SubscriptionCriteria filters, string id = "messages")
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(id);
            var outObject = (JObject)JToken.FromObject(filters);
            jarrayObj.Add(outObject);
            string outMessage = "";
            if (BuildAndSendRequest("shh_subscribe", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Cancels and removes an existing subscription
        /// </summary>
        /// <param name="subscriptionId">subscription ID</param>
        /// <returns></returns>
        public bool Unsubscribe(string subscriptionId)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(subscriptionId);
            string outMessage = "";
            return BuildAndSendRequest("shh_unsubscribe", jarrayObj, out outMessage);
        }

        /// <summary>
        /// Create a new filter within the node. This filter can be used to poll for new messages that match the set of criteria
        /// </summary>
        /// <param name="filters">Message filters</param>
        /// <returns>filter identifier</returns>
        public string CreateNewMessageFilter(SubscriptionCriteria filters)
        {
            JArray jarrayObj = new JArray();
            var outObject = (JObject)JToken.FromObject(filters);
            jarrayObj.Add(outObject);
            string outMessage = "";
            if (BuildAndSendRequest("shh_newMessageFilter", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }

        /// <summary>
        /// Uninstall a message filter in the node
        /// </summary>
        /// <param name="filterIdentifier">filter identifier as returned when the filter was created</param>
        /// <returns></returns>
        public bool DeleteMessageFilter(string filterIdentifier)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(filterIdentifier);
            string outMessage = "";
            return BuildAndSendRequest("shh_deleteMessageFilter", jarrayObj, out outMessage);
        }

        /// <summary>
        /// Retrieve messages that match the filter criteria and are received between the last time this function was called and now
        /// </summary>
        /// <param name="filterIdentifier">ID of filter that was created with shh_newMessageFilter</param>
        /// <returns>Array of messages</returns>
        public List<ReceivedData> ReceiveMessage(string filterIdentifier)
        {
            JArray jarrayObj = new JArray();
            jarrayObj.Add(filterIdentifier);
            string outMessage = "";
            if (BuildAndSendRequest("shh_getFilterMessages", jarrayObj, out outMessage))
            {
                return JsonConvert.DeserializeObject< List<ReceivedData>>(outMessage);
            }
            return null;
        }

        /// <summary>
        /// Creates a whisper message and injects it into the network for distribution
        /// </summary>
        /// <param name="msg">Message</param>
        /// <returns>message hash</returns>
        public string SendMessage(MessageDesc msg)
        {
            JArray jarrayObj = new JArray();
            var outObject = (JObject)JToken.FromObject(msg);
            jarrayObj.Add(outObject);
            string outMessage = "";
            if (BuildAndSendRequest("shh_post", jarrayObj, out outMessage))
            {
                return outMessage;
            }
            return "";
        }        
    }
}
