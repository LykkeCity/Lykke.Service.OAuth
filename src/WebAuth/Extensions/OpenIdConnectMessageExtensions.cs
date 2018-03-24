using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using AspNet.Security.OpenIdConnect.Primitives;

namespace WebAuth.Extensions
{
    internal static class OpenIdConnectMessageExtensions
    {
        /// <summary>Serializes an OpenID Connect message.</summary>
        /// <param name="message">The <see cref="T:Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage" /> instance.</param>
        /// <returns>The serialized payload.</returns>
        public static byte[] Export(this OpenIdConnectMessage message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            var paramDict = message
                .GetParameters()
                .ToDictionary(i => i.Key, i => i.Value.Value.ToString());

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(0);
                    binaryWriter.Write(paramDict.Count);
                    foreach (KeyValuePair<string, string> parameter in paramDict)
                    {
                        binaryWriter.Write(parameter.Key);
                        binaryWriter.Write(parameter.Value);
                    }
                }
                return memoryStream.ToArray();
            }
        }

        /// <summary>Deserializes and populates an OpenID Connect message.</summary>
        /// <param name="message">The <see cref="T:Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage" /> instance.</param>
        /// <param name="payload">The payload containing the serialized parameters.</param>
        /// <returns>The <see cref="T:Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage" /> instance.</returns>
        public static OpenIdConnectMessage Import(this OpenIdConnectMessage message, byte[] payload)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (payload == null)
                throw new ArgumentNullException("payload");

            using (MemoryStream memoryStream = new MemoryStream(payload))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    if (binaryReader.ReadInt32() != 0)
                        throw new InvalidOperationException("The OpenID Connect message was serialized using an incompatible version.");
                    int index = binaryReader.ReadInt32();
                    for (int i = 0; i < index; ++i)
                    {
                        string key = binaryReader.ReadString();
                        string val = binaryReader.ReadString();
                        if (!message.HasParameter(key))
                            message.SetParameter(key, val);
                    }
                    return message;
                }
            }
        }
    }
}
