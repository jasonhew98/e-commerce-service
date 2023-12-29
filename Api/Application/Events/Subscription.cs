using System;

namespace Api.Application.Events
{
	public class Subscription
	{
		public Type EventType { get; private set; }
		public Type HandlerType { get; private set; }

		public Subscription(Type eventType, Type handlerType)
		{
			EventType = eventType;
			HandlerType = handlerType;
		}
	}
}
