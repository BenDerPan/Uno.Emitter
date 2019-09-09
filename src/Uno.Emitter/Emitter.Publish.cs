using MQTTnet;
using MQTTnet.Client;
using System.Text;
using Uno.Emitter.Utility;

namespace Uno.Emitter
{
    public partial class Connection
    {
        #region Publish Members

        /// <summary>
        /// Asynchonously publishes a message to the emitter.io service. Uses the default
        /// key that should be specified in the constructor.
        /// </summary>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <returns>The message identifier for this operation.</returns>
        public ushort Publish(string channel, byte[] message)
        {
            if (this.DefaultKey == null)
                throw EmitterException.NoDefaultKey;
            return this.Publish(this.DefaultKey, channel, message);
        }

        /// <summary>
        /// Asynchonously publishes a message to the emitter.io service. Uses the default
        /// key that should be specified in the constructor.
        /// </summary>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <returns>The message identifier for this operation.</returns>
        public ushort Publish(string channel, string message)
        {
            if (this.DefaultKey == null)
                throw EmitterException.NoDefaultKey;
            return this.Publish(this.DefaultKey, channel, Encoding.UTF8.GetBytes(message));
        }

        /// <summary>
        /// Publishes a message to the emitter.io service asynchronously.
        /// </summary>
        /// <param name="key">The key to use for this publish request.</param>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <returns>The message identifier.</returns>
        public ushort Publish(string key, string channel, string message)
        {
            //return this.Client.Publish(FormatChannel(key, channel), Encoding.UTF8.GetBytes(message));
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(FormatChannel(key, channel))
                .WithPayload(Encoding.UTF8.GetBytes(message))
                .Build();
            this.Client.PublishAsync(msg);
            return 1;
        }

        /// <summary>
        /// Publishes a message to the emitter.io service asynchronously.
        /// </summary>
        /// <param name="key">The key to use for this publish request.</param>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <returns>The message identifier.</returns>
        public ushort Publish(string key, string channel, byte[] message)
        {
            //return this.Client.Publish(FormatChannel(key, channel), message);
            var msg = new MqttApplicationMessageBuilder()
               .WithTopic(FormatChannel(key, channel))
               .WithPayload(message)
               .Build();
            this.Client.PublishAsync(msg);
            return 1;


        }

        /// <summary>
        /// Publishes a message to the emitter.io service asynchronously.
        /// </summary>
        /// <param name="key">The key to use for this publish request.</param>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <param name="ttl">The time to live of the message.</param>
        /// <returns>The message identifier.</returns>
        public ushort Publish(string key, string channel, string message, int ttl)
        {
            //return this.Client.Publish(FormatChannel(key, channel, Options.WithTTL(ttl)),
            //    Encoding.UTF8.GetBytes(message));
            return Publish(key, channel, message);
        }

        /// <summary>
        /// Publishes a message to the emitter.io service asynchronously.
        /// </summary>
        /// <param name="key">The key to use for this publish request.</param>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <param name="options">The options associated with the message. Ex: Options.WithLast(5).</param>
        /// <returns>The message identifier.</returns>
        public ushort Publish(string key, string channel, string message, params string[] options)
        {
            //GetHeader(options, out var retain, out var qos);
            //return this.Client.Publish(FormatChannel(key, channel, options), Encoding.UTF8.GetBytes(message), qos,
            //    retain);

            var msg = new MqttApplicationMessageBuilder()
               .WithTopic(FormatChannel(key, channel))
               .WithPayload(message)
               .WithExactlyOnceQoS()
               .Build();
            this.Client.PublishAsync(msg);
            return 1;
        }

        /// <summary>
        /// Publishes a message to the emitter.io service asynchronously.
        /// </summary>
        /// <param name="key">The key to use for this publish request.</param>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <param name="ttl">The time to live of the message.</param>
        /// <returns>The message identifier.</returns>
        public ushort Publish(string key, string channel, byte[] message, int ttl)
        {
            //return this.Client.Publish(FormatChannel(key, channel, Options.WithTTL(ttl)), message);
            return 10;
        }

        /// <summary>
        /// Publishes a message to the emitter.io service asynchronously.
        /// </summary>
        /// <param name="key">The key to use for this publish request.</param>
        /// <param name="channel">The channel to publish to.</param>
        /// <param name="message">The message body to send.</param>
        /// <param name="options">The options associated with the message. Ex: Options.WithoutEcho().</param>
        /// <returns>The message identifier.</returns>
        public ushort Publish(string key, string channel, byte[] message, params string[] options)
        {
            //GetHeader(options, out var retain, out var qos);
            //return this.Client.Publish(FormatChannel(key, channel, options), message, qos, retain);
            return 9;
        }

        #endregion Publish Members
    }
}