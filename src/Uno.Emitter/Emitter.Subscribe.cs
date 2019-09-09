using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Uno.Emitter.Messages;
using Uno.Emitter.Utility;

namespace Uno.Emitter
{
    public partial class Connection
    {
        #region Subscribe
        public event MessageHandler DefaultMessageHandler;

        /// <summary>
        /// Asynchronously subscribes to a particular channel of emitter.io service. Uses the default
        /// key that should be specified in the constructor.
        /// </summary>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <param name="optionalHandler">The callback to be invoked every time the message is received.</param>
        /// <param name="options">The options of the channel. Ex: Options.WithLast(10)</param>;
        /// <returns>The message identifier for this operation.</returns>
        public ushort Subscribe(string channel, MessageHandler optionalHandler=null, params string[] options)
        {
            if (this.DefaultKey == null)
                throw EmitterException.NoDefaultKey;
            return this.Subscribe(this.DefaultKey, channel, optionalHandler, options);
        }

        /// <summary>
        /// Asynchronously subscribes to a particular channel of emitter.io service.
        /// </summary>
        /// <param name="key">The key to use for this subscription request.</param>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <param name="optionalHandler">The callback to be invoked every time the message is received.</param>
        /// <param name="options">The options of the channel. Ex: Options.WithLast(10)</param>;
        /// <returns>The message identifier for this operation.</returns>
        public ushort Subscribe(string key, string channel, MessageHandler optionalHandler=null, params string[] options)
        {
            // Register the handler
            if (optionalHandler != null)
                this.Trie.RegisterHandler(channel, optionalHandler);

            // Subscribe
            this.Client.SubscribeAsync(new TopicFilterBuilder().WithTopic(FormatChannel(key, channel, options)).Build());
            return 1;
        }

        /// <summary>
        /// Asynchronously subscribes to a particular share group for a channel of emitter.io service.
        /// </summary>
        /// <param name="key">The key to use for this subscription request.</param>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <param name="shareGroup">The share group to join.</param>
        /// <param name="optionalHandler">The callback to be invoked every time the message is received.</param>
        /// <param name="options">The options of the channel. Ex: Options.WithLast(10)</param>;
        /// <returns>The message identifier for this operation.</returns>
        public ushort SubscribeWithGroup(string key, string channel, string shareGroup, MessageHandler optionalHandler=null, params string[] options)
        {
            // Register the handler
            if (optionalHandler != null)
                this.Trie.RegisterHandler(channel, optionalHandler);

            // Subscribe
            this.Client.SubscribeAsync(
                new TopicFilterBuilder()
                .WithTopic(FormatChannelShare(key, channel, shareGroup, options))
                .Build());
            return 2;
        }


        #endregion Subscribe

        #region Unsubscribe
        /// <summary>
        /// Asynchonously unsubscribes from a particular channel of emitter.io service. Uses the default
        /// key that should be specified in the constructor.
        /// </summary>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <returns>The message identifier for this operation.</returns>
        public ushort Unsubscribe(string channel)
        {
            if (this.DefaultKey == null)
                throw EmitterException.NoDefaultKey;
            return this.Unsubscribe(this.DefaultKey, channel);
        }

        /// <summary>
        /// Asynchonously unsubscribes from a particular channel of emitter.io service.
        /// </summary>
        /// <param name="key">The key to use for this unsubscription request.</param>
        /// <param name="channel">The channel to subscribe to.</param>
        /// <returns>The message identifier for this operation.</returns>
        public ushort Unsubscribe(string key, string channel)
        {
            // Unregister the handler
            this.Trie.UnregisterHandler(key);

            // Unsubscribe
            this.Client.UnsubscribeAsync();
            return 1;
        }

        #endregion Unsubscribe
    }
}
