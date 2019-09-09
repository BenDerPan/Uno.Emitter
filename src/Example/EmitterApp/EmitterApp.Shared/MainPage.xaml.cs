using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Uno.Emitter;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmitterApp
{
    public class MqttHandler : IMqttClientConnectedHandler
    {
        public MainPage Parent { get; private set; }
        public MqttHandler(MainPage parent = null)
        {
            Parent = parent;
        }
        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            await Task.Run(async () =>
            {
                Console.WriteLine($"================>连接成功：{eventArgs.AuthenticateResult.ToString()}");
                if (Parent != null)
                {
                    await Parent.ShowLog($"================>连接成功：{eventArgs.AuthenticateResult.ToString()}");
                }
            });

        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public async Task ShowLog(string msg)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.RunAsync(
              Windows.UI.Core.CoreDispatcherPriority.Normal,
                  () => tbOutput.Text = $"{Environment.NewLine}{msg}{Environment.NewLine}");
            });
        }

        private async void BtnSendMsg_Click(object sender, RoutedEventArgs e)
        {
            //channel: test /#/
            //key    : BaDC_v4dIYai2krr0qMS0AMV9rUVYrOi

            Uno.Emitter.Connection emitter = new Connection("BaDC_v4dIYai2krr0qMS0AMV9rUVYrOi", "localhost", 8080, false);
            emitter.Connect();
            await Task.Delay(3000);
            emitter.Subscribe("test/");
            await Task.Delay(3000);
            emitter.Publish("test/", "HHHHHH");
            var options = new MqttClientOptionsBuilder().WithWebSocketServer("localhost:8080").Build();
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            mqttClient.ConnectedHandler = new MqttHandler(this);
            CancellationTokenSource cancellation = new CancellationTokenSource();
            await Dispatcher.RunAsync(
            Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => tbOutput.Text = $"{Environment.NewLine}=============================================={Environment.NewLine}正在连接Emitter ...{Environment.NewLine}");
            await mqttClient.ConnectAsync(options, cancellation.Token);

            return;

        }
    }
}
