using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventBus
{
	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class HandlesEvent : System.Attribute
	{
		public HandlesEvent(){}
	}

	/**
	 * Based on Square Inc.'s Otto event bus library for Android
	 */
	public class Dispatcher : MonoBehaviour
	{
		private static readonly object INSTANCE_LOCK = new object();

		private static bool sInstanceCreated = false;

		private static Dispatcher sInstance;

		public static Dispatcher Instance
		{
			get 
			{
				if (!sInstanceCreated) {
					lock (INSTANCE_LOCK)
					{
						if (!sInstanceCreated) {
							sInstance = GetInstance();
							sInstanceCreated = true;
						}
					}
				}
				return sInstance;
			}
		}

		private static Dispatcher GetInstance()
		{
			try 
			{
				GameObject o = new GameObject("EventBus.Dispatcher");
				Dispatcher dispatcher = o.AddComponent<Dispatcher>();
				return dispatcher;
			}
			catch (ArgumentException ae)
			{
				throw new ArgumentException("Event Dispatcher can only be created on the Unity main thread." +
				                            "Instance must be invoked at least once from Start(), Awake(), Update(), FixedUpdate(), etc.");
			}
		}

		private class MyEventHandler
		{
			private object mInstance;

			private MethodInfo mMethod;

			private readonly int mHashCode;

			public MyEventHandler(object instance, MethodInfo method)
			{
				if (instance == null || method == null) {
					throw new ArgumentNullException("Cannot instantiate null event handler" +
					                                "with null instance or method");
				}
				mInstance = instance;
				mMethod = method;
				mHashCode = (33 + instance.GetHashCode()) * (33 + method.GetHashCode());
			}

			public override int GetHashCode()
			{
				return mHashCode;
			}

			public override bool Equals(object obj)
			{
				if (obj == null) {
					return false;
				}

				if (this == obj) {
					return true;
				}

				MyEventHandler that = (MyEventHandler) obj;
				return mInstance == that.mInstance && mMethod == that.mMethod;
			}

			public void HandleEvent(object myEvent)
			{
				mMethod.Invoke(mInstance, new object[] { myEvent });
			}
		}

		private class QueuedEvent
		{
			public readonly object myEvent;

			public readonly MyEventHandler myEventHandler;

			public QueuedEvent(object myEvent, MyEventHandler myEventHandler)
			{
				this.myEvent = myEvent;
				this.myEventHandler = myEventHandler;
			}
		}

		// Caches the verified methods with the HandlesEvent attribute for the given subscriber type
		private static Dictionary<Type, Dictionary<Type, MethodInfo>> sMethodCache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();


		// Caches the listeners for the given event type
		private static Dictionary<Type, HashSet<MyEventHandler>> sSubscribers = new Dictionary<Type, HashSet<MyEventHandler>>();

		// Keeps track of events to be carried out on the Unity game thread
		private List<QueuedEvent> mEventQueue;

		// Dispatches events while leaving the real event queue unlocked
		private List<QueuedEvent> mDispatchQueue;

		void Awake()
		{
			mEventQueue = new List<QueuedEvent>();
			mDispatchQueue = new List<QueuedEvent>();
		}

		void Update()
		{
			lock (mEventQueue)
			{
				mDispatchQueue.Clear();
				mDispatchQueue.AddRange(mEventQueue);
				mEventQueue.Clear();
			}
			foreach (QueuedEvent queuedEvent in mDispatchQueue) {
				
				Debug.Log("EventBus.Dispatcher: " + "dispatching event of type " + queuedEvent.myEvent.GetType().ToString());
				
				queuedEvent.myEventHandler.HandleEvent(queuedEvent.myEvent);
			}
		}

		void OnDestroy()
		{
			lock (INSTANCE_LOCK)
			{
				sInstanceCreated = false;
			}
		}

		public void Register(object subscriber)
		{
			Debug.Log("EventBus.Dispatcher: " + "Regster(), " + subscriber.ToString());
			if (subscriber == null) {
				throw new ArgumentNullException("Object to register must not be null");
			}

			Type subscriberType = subscriber.GetType();

			Dictionary<Type, MethodInfo> handlerMethods;
			if (sMethodCache.ContainsKey(subscriberType)) {
				Debug.Log("EventBus.Dispatcher: " + "method cache hit ");
				handlerMethods = sMethodCache[subscriberType];
			} else {
				Debug.Log("EventBus.Dispatcher: " + "method cache miss ");
				handlerMethods = GetVerifiedHandlerMethods(subscriberType);
				if (handlerMethods == null) {
					return;
				}
				sMethodCache.Add(subscriberType, handlerMethods);
			}

			foreach (KeyValuePair<Type, MethodInfo> pair in handlerMethods) {
				Type eventType = pair.Key;
				MethodInfo handlerMethod = pair.Value;
				
				Debug.Log("EventBus.Dispatcher: " + "method " + handlerMethod.ToString() + " can handle event " + eventType.ToString());

				HashSet<MyEventHandler> handlersForEvent;

				if (sSubscribers.ContainsKey(eventType)) {
					handlersForEvent = sSubscribers[eventType];
				} else {
					handlersForEvent = new HashSet<MyEventHandler>();
					sSubscribers.Add(eventType, handlersForEvent);
				}

				handlersForEvent.Add(new MyEventHandler(subscriber, handlerMethod));
			}
		}

		public void Unregister(object subscriber)
		{
			if (subscriber == null) {
				throw new ArgumentNullException("Object to unregister must not be null");
			}

			Type subscriberType = subscriber.GetType();
			
			Dictionary<Type, MethodInfo> handlerMethods;
			if (sMethodCache.ContainsKey(subscriberType)) {
				handlerMethods = sMethodCache[subscriberType];
			} else {
				handlerMethods = GetVerifiedHandlerMethods(subscriberType);
				sMethodCache.Add(subscriberType, handlerMethods);
			}

			foreach (KeyValuePair<Type, MethodInfo> pair in handlerMethods) {
				Type eventType = pair.Key;
				MethodInfo handlerMethod = pair.Value;
				
				HashSet<MyEventHandler> handlersForEvent;
				
				if (sSubscribers.ContainsKey(eventType)) {
					handlersForEvent = sSubscribers[eventType];
				} else {
					handlersForEvent = new HashSet<MyEventHandler>();
					sSubscribers.Add(eventType, handlersForEvent);
				}
				
				MyEventHandler eventHandler = 
					new MyEventHandler(subscriber, handlerMethod);

				if (handlersForEvent.Contains(eventHandler)) {
					handlersForEvent.Remove(eventHandler);
				}
			}
		}

		public void Post(object myEvent)
		{
			Type eventType = myEvent.GetType();

			Debug.Log("EventBus.Dispatcher: " + "posting event of type " + eventType.ToString());

			if (sSubscribers.ContainsKey(eventType)) {
				HashSet<MyEventHandler> handlers = sSubscribers[eventType];

				lock (mEventQueue)
				{
					foreach (MyEventHandler handler in handlers) {
						mEventQueue.Add(new QueuedEvent(myEvent, handler));
					}
				}
			} else {
				Debug.Log("EventBus.Dispatcher: " + "No handlers found for type " + eventType.ToString());
			}
		}

		private Dictionary<Type, MethodInfo> GetVerifiedHandlerMethods(Type type)
		{
			Dictionary<Type, MethodInfo> handlers = null;

			MethodInfo[] methods = type.GetMethods();

			if (methods.Length > 0) {
				foreach (MethodInfo method in methods) {
					if (Attribute.GetCustomAttribute(method, typeof(HandlesEvent)) != null) {
						ParameterInfo[] parameters = method.GetParameters();
						if (parameters.Length != 1) {
							throw new Exception("Method " + method.ToString() + " has the HandlesEvent attribute, " +
												"but requires " + parameters.Length + 
							                    " arguments. Must only require a single argument.");
						}
						Type eventType = parameters[0].ParameterType;
						if (eventType.IsInterface) {
							throw new Exception("Method " + method.ToString() + " has the HandlesEvent attribute, " +
							                    "but " + eventType.ToString() + " is an interface. " +
							                    " Events must be concrete classes.");
						}
						if (!method.IsPublic) {
							throw new Exception("Method " + method.ToString() + " has the HandlesEvent attribute " +
							                    "and has the correct signature, but it is not available publicly.");
						}
						if (handlers == null) {
							handlers = new Dictionary<Type, MethodInfo>();
						}
						handlers.Add(eventType, method);
					}
				}
			}

			return handlers;
		}
	}
}
