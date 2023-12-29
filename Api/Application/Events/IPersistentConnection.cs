using RabbitMQ.Client;
using System;

namespace Api.Application.Events
{
	public interface IPersistentConnection
	{
		event EventHandler OnReconnectedAfterConnectionFailure;
		bool IsConnected { get; }

		bool TryConnect();
		IModel CreateModel();
	}
}
