/*
Copyright (c) 2016 Roman Atachiants

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution.

The Eclipse Public License:  http://www.eclipse.org/legal/epl-v10.html
The Eclipse Distribution License: http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Roman Atachiants - integrating with emitter.io
*/

using Uno.Emitter.Utility;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Disconnecting;
using Uno.Emitter.Messages;
using System.Text;
using System.Threading;

namespace Uno.Emitter
{
    public class MqttConnectedHandler : IMqttClientConnectedHandler
    {
        public MqttConnectedHandler()
        {

        }
        public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                Console.WriteLine("连接成功!");
            });
        }
    }

    public class MqttDisconnectedHandler : IMqttClientDisconnectedHandler
    {
        public MqttDisconnectedHandler()
        {
        }
        public Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            return Task.Run(() =>
            {
                Console.WriteLine("连接断开!!!!");
            });
        }
    }
    
        
    


    /// <summary>
    /// Represents a message handler callback.
    /// </summary>
    public delegate void MessageHandler(string channel, byte[] message);

    /// <summary>
    /// Delegate that defines event handler for client/peer disconnection
    /// </summary>
    public delegate void DisconnectHandler(object sender, EventArgs e);

    /// <summary>
    /// Delegate that defines an event handler for an error.
    /// </summary>
    public delegate void ErrorHandler(object sender, Exception e);

    /// <summary>
    /// Represents emitter.io MQTT-based client.
    /// </summary>
    public partial class Connection : IDisposable
    {
        #region Constructors
        private readonly IMqttClient Client;
        private readonly ReverseTrie<MessageHandler> Trie = new ReverseTrie<MessageHandler>(-1);
        private string DefaultKey = null;

        private readonly ConcurrentDictionary<long, TaskCompletionSource<byte[]>> WaitingRequests = new ConcurrentDictionary<long, TaskCompletionSource<byte[]>>();
        private readonly ConcurrentDictionary<string, int> RequestNames = new ConcurrentDictionary<string, int>();
        private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        string _defaultKey;
        string _broker;
        int _brokerPort;
        bool _secure = true;
        CancellationTokenSource cancellation;

        /// <summary>
        /// Constructs a new emitter.io connection.
        /// </summary>
        public Connection() : this(null, null, 0, true) { }

        /// <summary>
        /// Constructs a new emitter.io connection.
        /// </summary>
        /// <param name="defaultKey">The default key to use.</param>
        public Connection(string defaultKey) : this(defaultKey, null, 0, true) { }

        /// <summary>
        /// Constructs a new emitter.io connection.
        /// </summary>
        /// <param name="defaultKey">The default key to use.</param>
        /// <param name="broker">The address of the broker.</param>
        /// <param name="secure">Whether the connection has to be secure.</param>
        public Connection(string defaultKey, string broker, bool secure = true) : this(defaultKey, broker, 0, secure) { }

        /// <summary>
        /// Constructs a new emitter.io connection.
        /// </summary>
        /// <param name="defaultKey">The default key to use.</param>
        /// <param name="broker">The address of the broker.</param>
        /// <param name="brokerPort">The port of the broker to use.</param>
        /// <param name="secure">Whether the connection has to be secure.</param>
        public Connection(string defaultKey, string broker, int brokerPort, bool secure = true)
        {
            if (broker == null)
                broker = "api.emitter.io";
            if (brokerPort <= 0)
                brokerPort = secure ? 443 : 8080;

            this.DefaultKey = defaultKey;
            _defaultKey = defaultKey;
            _broker = broker;
            _brokerPort = brokerPort;
            _secure = secure;
            if (secure)
                throw new Exception("暂时不支持！");
            else
            {
                var factory = new MqttFactory();
                this.Client = factory.CreateMqttClient();
            }

            this.Client.ConnectedHandler = new MqttConnectedHandler();
            this.Client.UseApplicationMessageReceivedHandler(OnMessageReceived);
            this.Client.DisconnectedHandler= new MqttDisconnectedHandler();
        }

#endregion Constructors

        #region Error Members

        /// <summary>
        /// Occurs when an error occurs.
        /// </summary>
        public event ErrorHandler Error;

        /// <summary>
        /// Invokes the error handler.
        /// </summary>
        /// <param name="ex"></param>
        private void InvokeError(Exception ex)
        {
            this.Error?.Invoke(this, ex);
        }

        /// <summary>
        /// Invokes the error handler.
        /// </summary>
        /// <param name="status"></param>
        private void InvokeError(int status)
        {
            if (status == 200)
                return;

            InvokeError(EmitterException.FromStatus(status));
        }

        #endregion Error Members

        #region Static Members

        /// <summary>
        /// Establishes a new connection by creating the connection instance and connecting to it.
        /// </summary>
        /// <returns>The connection state.</returns>
        public static Connection Establish()
        {
            return Establish(null, null, 0, true);
        }

        /// <summary>
        /// Establishes a new connection by creating the connection instance and connecting to it.
        /// </summary>
        /// <param name="defaultKey">The default key to use.</param>
        /// <returns>The connection state.</returns>
        public static Connection Establish(string defaultKey)
        {
            return Establish(defaultKey, null, 0, true);
        }

        public static Connection Establish(string defaultKey, string broker, bool secure = true)
        {
            return Establish(defaultKey, broker, 0, secure);
        }

        /// <summary>
        /// Establishes a new connection by creating the connection instance and connecting to it.
        /// </summary>
        /// <param name="brokerHostName">The broker hostname to use.</param>
        /// <param name="defaultKey">The default key to use.</param>
        /// <returns>The connection state.</returns>
        public static Connection Establish(string defaultKey, string broker, int brokerPort, bool secure = true)
        {
            // Create the connection
            var conn = new Connection(defaultKey, broker, brokerPort, secure);

            // Connect
            conn.Connect();

            // Return it
            return conn;
        }

#endregion Static Members

#region Connect / Disconnect Members

        /// <summary>
        /// Occurs when the client was disconnected.
        /// </summary>
        public event DisconnectHandler Disconnected;

        /// <summary>
        /// Gets whether the client is connected or not.
        /// </summary>
        public bool IsConnected
        {
            get { return this.Client.IsConnected; }
        }

        /// <summary>
        /// Occurs when the connection was closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisconnect(object sender, EventArgs e)
        {
            // Forward the event
            this.Disconnected?.Invoke(sender, e);
        }


       
        /// <summary>
        /// Connects the emitter.io service.
        /// </summary>
        public void Connect()
        {
            cancellation = new CancellationTokenSource();
            var options = new MqttClientOptionsBuilder().WithWebSocketServer($"{_broker}:{_brokerPort}").Build();
            var connack = this.Client.ConnectAsync(options, cancellation.Token);
        }

        /// <summary>
        /// Disconnects from emitter.io service.
        /// </summary>
        public void Disconnect()
        {
            this.Client.DisconnectAsync();
        }

#endregion Connect / Disconnect Members

        /// <summary>
        /// Occurs when a message is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!e.ApplicationMessage.Topic.StartsWith("emitter"))
                    {
                        var handlers = this.Trie.Match(e.ApplicationMessage.Topic);
                        // Invoke every handler matching the channel
                        foreach (MessageHandler handler in handlers)
                            handler(e.ApplicationMessage.Topic, e.ApplicationMessage.Payload);

                        if (handlers.Count == 0)
                            DefaultMessageHandler?.Invoke(e.ApplicationMessage.Topic, e.ApplicationMessage.Payload);
                        return;
                    }

                    // Did we receive a keygen response?
                    if (e.ApplicationMessage.Topic == "emitter/keygen/")
                    {
                        // Deserialize the response
                        var response = KeygenResponse.FromBinary(e.ApplicationMessage.Payload);
                        if (response == null || response.Status != 200)
                        {
                            this.InvokeError(response.Status);
                            return;
                        }

                        // Call the response handler we have registered previously
                        // TODO: get rid of the handler afterwards, or refac keygen
                        if (this.KeygenHandlers.ContainsKey(response.Channel))
                            ((KeygenHandler)this.KeygenHandlers[response.Channel])(response);
                        return;
                    }

                    if (e.ApplicationMessage.Topic == "emitter/presence/")
                    {
                        var presenceEvent = PresenceEvent.FromBinary(e.ApplicationMessage.Payload);

                        var handlers = this.PresenceTrie.Match(presenceEvent.Channel);
                        // Invoke every handler matching the channel
                        foreach (PresenceHandler handler in handlers)
                            handler(presenceEvent);

                        if (handlers.Count == 0)
                            DefaultPresenceHandler?.Invoke(presenceEvent);

                        return;
                    }

                    if (e.ApplicationMessage.Topic == "emitter/error/")
                    {
                        var errorEvent = ErrorEvent.FromBinary(e.ApplicationMessage.Payload);
                        var emitterException = new EmitterException((EmitterEventCode)errorEvent.Status, errorEvent.Message);

                        InvokeError(emitterException);
                        return;
                    }

                    if (e.ApplicationMessage.Topic == "emitter/me/")
                    {
                        var meResponse = MeResponse.FromBinary(e.ApplicationMessage.Payload);
                        Me?.Invoke(meResponse);

                        return;
                    }
                    return;
                }
                catch (Exception ex)
                {
                    this.InvokeError(ex);
                    return;
                }
            });
        }
#region Private Members

        private string FormatOptions(string[] options)
        {
            string formatted = "";
            if (options != null && options.Length > 0)
            {
                formatted += "?";
                for (int i = 0; i < options.Length; ++i)
                {
                    if (options[i][0] == '+')
                        continue;

                    formatted += options[i];
                    if (i + 1 < options.Length)
                        formatted += "&";
                }
            }

            return formatted;
        }

        /// <summary>
        /// Formats the channel.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="channel">The channel name.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        private string FormatChannel(string key, string channel, params string[] options)
        {
            string k = key.Trim('/');
            string c = channel.Trim('/');
            string o = FormatOptions(options);

            var formatted = string.Format("{0}/{1}/{2}", k, c, o);

            return formatted;
        }

        private string FormatChannelLink(string channel, params string[] options)
        {
            string c = channel.Trim('/');
            string o = FormatOptions(options);

            var formatted = string.Format("{0}/{1}", c, o);

            return formatted;
        }

        private string FormatChannelShare(string key, string channel, string shareGroup, params string[] options)
        {
            string k = key.Trim('/');
            string c = channel.Trim('/');
            string s = shareGroup.Trim('/');
            string o = FormatOptions(options);

            var formatted = string.Format("{0}/$share/{1}/{2}/{3}", k, s, c, o);

            return formatted;
        }

        private void GetHeader(string[] options, out bool retain, out byte qos)
        {
            retain = false;
            qos = 0;
            foreach (string o in options)
            {
                switch (o)
                {
                    case Options.Retain:
                        retain = true;
                        break;
                    case Options.QoS1:
                        qos = 1;
                        break;
                }
            }
        }
#endregion Private Members

#region IDisposable

        /// <summary>
        /// Disposes the connection.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the connection.
        /// </summary>
        /// <param name="disposing">Whether we are disposing or finalizing.</param>
        protected void Dispose(bool disposing)
        {
            try
            {
                this.Disconnect();
            }
            catch { }
        }

        /// <summary>
        /// Finalizes the connection.
        /// </summary>
        ~Connection()
        {
            this.Dispose(false);
        }

#endregion IDisposable
    }
}