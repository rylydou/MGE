using System;

namespace MGE
{
	public class Event
	{
		Action action = () => { };

		public void Sub(Action action) => this.action += () => action.Invoke();
		public void Unsub(Action action) => this.action -= () => action.Invoke();
		public void UnsubAll() => action = () => { };

		public void Invoke() => action.Invoke();
	}

	public class Event<T1>
	{
		Action<T1> action = (p1) => { };

		public void Sub(Action<T1> action) => this.action += (p1) => action.Invoke(p1);
		public void Unsub(Action<T1> action) => this.action -= (p1) => action.Invoke(p1);
		public void UnsubAll() => action = (p1) => { };

		public void Invoke(T1 p1) => action.Invoke(p1);
	}

	public class Event<T1, T2>
	{
		Action<T1, T2> action = (p1, p2) => { };

		public void Sub(Action<T1, T2> action) => this.action += (p1, p2) => action.Invoke(p1, p2);
		public void Unsub(Action<T1, T2> action) => this.action -= (p1, p2) => action.Invoke(p1, p2);
		public void UnsubAll() => action = (p1, p2) => { };

		public void Invoke(T1 p1, T2 p2) => action.Invoke(p1, p2);
	}

	public class Event<T1, T2, T3>
	{
		Action<T1, T2, T3> action = (p1, p2, p3) => { };

		public void Sub(Action<T1, T2, T3> action) => this.action += (p1, p2, p3) => action.Invoke(p1, p2, p3);
		public void Unsub(Action<T1, T2, T3> action) => this.action -= (p1, p2, p3) => action.Invoke(p1, p2, p3);
		public void UnsubAll() => action = (p1, p2, p3) => { };

		public void Invoke(T1 p1, T2 p2, T3 p3) => action.Invoke(p1, p2, p3);
	}

	public class Event<T1, T2, T3, T4>
	{
		Action<T1, T2, T3, T4> action = (p1, p2, p3, p4) => { };

		public void Sub(Action<T1, T2, T3, T4> action) => this.action += (p1, p2, p3, p4) => action.Invoke(p1, p2, p3, p4);
		public void Unsub(Action<T1, T2, T3, T4> action) => this.action -= (p1, p2, p3, p4) => action.Invoke(p1, p2, p3, p4);
		public void UnsubAll() => action = (p1, p2, p3, p4) => { };

		public void Invoke(T1 p1, T2 p2, T3 p3, T4 p4) => action.Invoke(p1, p2, p3, p4);
	}
}