using ManiaplanetXMLRPC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManiaplanetXMLRPC.Structures
{
	public class RegisterEvent : IRegisterEvent<object>
	{
		#region Public Properties

		private Dictionary<ICallback, MethodInfo> i_listeners = new Dictionary<ICallback, MethodInfo>();

		#endregion Public Properties

		#region Public Methods

		public void CheckNulls()
		{
			foreach (var listener in i_listeners)
				if (listener.Key == null)
					i_listeners.Remove(listener.Key);
		}

		public void RegisterListener<Know>(ResultEvent onAction) where Know : ICallback
		{
			CheckNulls();
			// TODO: IMPLEMENT THIS
		}

		public void RegisterListener<Interface, OBJ>(OBJ op)
			where Interface : ICallback
			where OBJ : class
		{
			CheckNulls();

			i_listeners[(Interface)(object)op] = op.GetType().GetInterfaces().Where(t => t == typeof(Interface)).First().GetMethods()[0];
			// TODO: IMPLEMENT THIS CORRECTLY
		}

		public void RegisterListener<Interface>(object op)
			where Interface : ICallback
		{
			RegisterListener<Interface, object>(op);
		}

		/// <summary>
		/// Trigger the listeners
		/// </summary>
		/// <typeparam name="Interface">Callback name to use</typeparam>
		/// <param name="caller">The caller</param>
		/// <param name="op">The parameters</param>
		/// <remarks>
		/// Prefer using GetListener'Interface'().Trigger();
		/// </remarks>
		public void TriggerListeners<Interface>(object caller, params object[] op)
			where Interface : ICallback
		{
			foreach (var listener in i_listeners)
			{
				try
				{
					if (listener.Key.GetType().GetInterfaces().Contains(typeof(Interface)))
					{
						listener.Value?.Invoke(listener.Key, op);
					}
				}
				catch { }
			}
		}

		public void TriggerListeners<Interface>(object caller, Action<Interface> toDo)
			where Interface : ICallback
		{
			List<Interface> list = new List<Interface>();
			foreach (var listener in i_listeners)
			{
				try
				{
                    if (listener.Key.GetType().GetInterfaces().Contains(typeof(Interface)))
                        list.Add((Interface)listener.Key);
				}
				catch { }
			}

			foreach (var i in list)
				toDo(i);
		}

		#endregion Public Methods
	}
}