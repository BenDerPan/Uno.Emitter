using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using Uno.MQTTnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.MQTTnet
{
    public class WasmMqttFactory:IMqttFactory
    {
        private IMqttClientAdapterFactory _clientAdapterFactory = new WasmMqttClientAdapterFactory();

        public WasmMqttFactory() : this(new MqttNetLogger())
        {
        }

        public WasmMqttFactory(IMqttNetLogger logger)
        {
            DefaultLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IMqttNetLogger DefaultLogger { get; }

        public IMqttFactory UseClientAdapterFactory(IMqttClientAdapterFactory clientAdapterFactory)
        {
            _clientAdapterFactory = clientAdapterFactory ?? throw new ArgumentNullException(nameof(clientAdapterFactory));
            return this;
        }

        public IMqttClient CreateMqttClient()
        {
            return CreateMqttClient(DefaultLogger);
        }

        public IMqttClient CreateMqttClient(IMqttNetLogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            return new MqttClient(_clientAdapterFactory, logger);
        }

        public IMqttClient CreateMqttClient(IMqttClientAdapterFactory clientAdapterFactory)
        {
            if (clientAdapterFactory == null) throw new ArgumentNullException(nameof(clientAdapterFactory));

            return new MqttClient(clientAdapterFactory, DefaultLogger);
        }

        public IMqttClient CreateMqttClient(IMqttNetLogger logger, IMqttClientAdapterFactory clientAdapterFactory)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (clientAdapterFactory == null) throw new ArgumentNullException(nameof(clientAdapterFactory));

            return new MqttClient(clientAdapterFactory, logger);
        }

        public IMqttServer CreateMqttServer()
        {
            throw new NotImplementedException();
        }

        public IMqttServer CreateMqttServer(IMqttNetLogger logger)
        {
            throw new NotImplementedException();
        }

        public IMqttServer CreateMqttServer(IEnumerable<IMqttServerAdapter> serverAdapters, IMqttNetLogger logger)
        {
            throw new NotImplementedException();
        }

        public IMqttServer CreateMqttServer(IEnumerable<IMqttServerAdapter> serverAdapters)
        {
            throw new NotImplementedException();
        }


    }
}
