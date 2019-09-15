using System;
using MQTTnet.Adapter;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;
using MQTTnet.Formatter;

namespace Uno.MQTTnet
{
    public class WasmMqttClientAdapterFactory : IMqttClientAdapterFactory
    {
        public IMqttChannelAdapter CreateClientAdapter(IMqttClientOptions options, IMqttNetChildLogger logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            
            switch (options.ChannelOptions)
            {
                case MqttClientWebSocketOptions webSocketOptions:
                    {
                        return new MqttChannelAdapter(new WasmMqttWebSocketChannel(webSocketOptions), new MqttPacketFormatterAdapter(options.ProtocolVersion), logger);
                    }

                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }
    }
}
