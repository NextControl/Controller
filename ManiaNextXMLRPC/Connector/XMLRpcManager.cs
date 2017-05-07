using ManiaplanetXMLRPC.Attributes;
using ManiaplanetXMLRPC.Connector.Gbx;
using ManiaplanetXMLRPC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManiaplanetXMLRPC.Connector
{
	// TODO: MAKE THIS WORKIN
	public class XmlRPCManager
	{
		#region Public Fields

		public Client Client;
		public OnConnect Event_OnConnect;
		public TcpClient tcpClient = new TcpClient();

		#endregion Public Fields

		#region Internal Fields

		public byte[] Buffer;

		#endregion Internal Fields

		#region Private Fields

		private CancellationTokenSource cts = new CancellationTokenSource();
		private int requests;

		#endregion Private Fields

		#region Public Delegates

		public delegate void OnConnect();

		#endregion Public Delegates

		#region Public Events

		public event ResultEvent OnNewCallback;

		#endregion Public Events

		#region Public Methods

		public Dictionary<int, GbxParam> callbackList = new Dictionary<int, GbxParam>();
		public Dictionary<int, GbxParam> responses = new Dictionary<int, GbxParam>();

		public async Task<GbxParam> AsyncSendCall(GbxParam callToSend, Action<GbxParam> actionOnResult = null)
		{
			//var data = Encoding.UTF8.GetBytes(callToSend.Result);
			//tcpClient.Client.BeginSend(data, 0, data.Length, 0, new AsyncCallback((result) => { Console.WriteLine(callToSend.Result); }), tcpClient.Client);
			callToSend.Handle = --requests;
			int handle = await SendCall(tcpClient, callToSend);

			await Task.Factory.StartNew(() => 
            {
                bool createdNew;
                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, (Guid.NewGuid().ToString() + Guid.NewGuid().ToString()), out createdNew);

                do
                {
                    waitHandle.WaitOne(TimeSpan.FromSeconds(1 / 20f));
                }
                while (!responses.ContainsKey(handle) || responses[handle] == null);
            }).ContinueWith(x => x);

			actionOnResult?.Invoke(responses[handle]);

			return responses[handle];
		}

		public async Task Connect(IPAddress wantedIP, int wantedPort)
		{
			IPEndPoint endPoint = new IPEndPoint(wantedIP, wantedPort);

			tcpClient = new TcpClient(endPoint.AddressFamily);
			tcpClient.Client = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			await tcpClient.ConnectAsync(wantedIP, wantedPort);
			Event_OnConnect?.Invoke();

			HandShake();

			Buffer = new byte[8];

            new Action(async () =>
            {
                await tcpClient.GetStream().ReadAsync(Buffer, 0, Buffer.Length)
                    .ContinueWith(OnReceive);
            })();
		}

		public void SendCall(GbxParam callToSend, Action<GbxParam> actionOnResult = null)
		{
			AsyncSendCall(callToSend, actionOnResult).Wait();
		}

		#endregion Public Methods

		#region Private Methods

		public static async Task<int> SendCall(TcpClient client, GbxParam inCall)
		{
			if (client == null || inCall == null) return 0;
			if (client.Connected)
			{
				try
				{
					// create request body ...
					byte[] body = Encoding.UTF8.GetBytes(inCall.Result);

					// create response header ...
					byte[] bSize = BitConverter.GetBytes(body.Length);
					byte[] bHandle = BitConverter.GetBytes(inCall.Handle);

					// create call data ...
					byte[] call = new byte[bSize.Length + bHandle.Length + body.Length];
					Array.Copy(bSize, 0, call, 0, bSize.Length);
					Array.Copy(bHandle, 0, call, 4, bHandle.Length);
					Array.Copy(body, 0, call, 8, body.Length);

                    // send call ...
                    var stream = client.GetStream();
					await stream.WriteAsync(call, 0, call.Length);
                    await stream.FlushAsync();

					return inCall.Handle;
				}
				catch
				{
					return 0;
				}
			}
			throw new NotConnectedException();
		}

		private async static Task<byte[]> ReceiveRpc(TcpClient client, int inLength)
		{
            var stream = client.GetStream();

			byte[] data = new byte[inLength];
			int offset = 0;
			byte[] buffer;
			while (inLength > 0)
			{
				int read = Math.Min(inLength, 1024);
				buffer = new byte[read];
				int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				Array.Copy(buffer, 0, data, offset, buffer.Length);
				inLength -= bytesRead;
				offset += bytesRead;
			}
			return data;
		}

		private bool HandShake()
		{
			if (tcpClient.Connected)
			{
				// Re-Init buffer
				var buffer = new byte[4];
				tcpClient.Client.Receive(buffer);

				// Get the size
				int size = BitConverter.ToInt32(buffer, 0);
				byte[] handShakeBuffer = new byte[size];

				// Get the handshake.
				tcpClient.Client.Receive(handShakeBuffer);
				string handShake = Encoding.UTF8.GetString(handShakeBuffer);

				return handShake == "GBXRemote 2";
			}
			else
				throw new NotConnectedException();
		}

		private async void OnReceive(IAsyncResult result)
		{
            Console.WriteLine("Receiving something!");

			GbxParam gbx = await ToGbx(tcpClient, Buffer);
            tcpClient.Client.Blocking = true;

			Buffer = new byte[8];

			if (gbx.Type == GbxParam.MessageType.Callback)
			{
				CallbackContext cc = new CallbackContext();
				cc.eventName = gbx.EventName;
				cc.result = gbx.Parameters;
				cc.realCall = true;
				cc.operationNumber = gbx.Handle;
				cc.fromManialink = gbx.EventName == "ManiaPlanet.PlayerManialinkPageAnswer";
				cc.gbx = gbx;
				cc.callerClient = Client;

				OnNewCallback?.Invoke(cc);
			}
			else
			{
				if (callbackList.ContainsKey(gbx.Handle) && callbackList[gbx.Handle] != null)
				{
					// TODO: ((GbxCallCallbackHandler)callbackList[gbx.Handle])?.BeginInvoke(gbx, null, null);
					var origin = (GbxParam)callbackList[gbx.Handle];
					gbx.EventName = origin.EventName;
					callbackList.Remove(gbx.Handle);
				}
				responses[gbx.Handle] = gbx;

				CallbackContext cc = new CallbackContext();
				cc.eventName = gbx.EventName;
				cc.result = gbx.Parameters;
				cc.realCall = true;
				cc.operationNumber = gbx.Handle;
				cc.fromManialink = gbx.EventName == "ManiaPlanet.PlayerManialinkPageAnswer";
				cc.gbx = gbx;
				cc.callerClient = Client;

				OnNewCallback?.Invoke(cc);
			}

            tcpClient.Client.Blocking = false;

            await tcpClient.GetStream().ReadAsync(Buffer, 0, Buffer.Length)
                .ContinueWith(OnReceive);
        }

		private async Task<GbxParam> ToGbx(TcpClient socket, byte[] buffer)
		{
            if (socket.Connected)
            {
                byte[] bSize = new byte[4];
                byte[] bHandle = new byte[4];

                if (buffer == null)
                {
                    Console.WriteLine("Buffer shouldn't be null!");

                    var stream = socket.GetStream();
                    await stream.ReadAsync(bSize, 0, bSize.Length);
                    await stream.ReadAsync(bHandle, 0, bHandle.Length);
                }
                else
                {
                    Array.Copy(buffer, 0, bSize, 0, 4);
                    Array.Copy(buffer, 4, bHandle, 0, 4);
                }
                int size = BitConverter.ToInt32(bSize, 0);
                int handle = BitConverter.ToInt32(bHandle, 0);

                // receive response body ...
                byte[] data = await ReceiveRpc(socket, size);

                // parse the response ...
                GbxParam gbx = GbxParam.ParseResponse(handle, data);

                return gbx;
            }
			throw new NotConnectedException();
		}

		#endregion Private Methods

		#region Public Classes

		public delegate void GbxCallCallbackHandler(GbxParam response);

		public class NotConnectedException : Exception { }

		#endregion Public Classes
	}
}