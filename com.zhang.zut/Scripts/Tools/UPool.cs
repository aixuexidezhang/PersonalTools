using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MyTool.Tools
{
	/// <summary>
	/// 对象池
	/// </summary>
	[System.Serializable]
	public class UPool<T> where T : Component
	{
		[SerializeField]
		private T Prefab;
		[SerializeField]
		int PoolSize;
		public Queue<T> Pools => Queues;
		Queue<T> Queues;
		Transform Parent;
		bool IsCopy = false;
		/// <summary>
		/// 创建池
		/// </summary>
		/// <param name="prefab">对应类型的预制体</param>
		/// <param name="p">父级</param>
		/// <param name="PoolSize">初始化生成的数量</param>
		/// <param name="isCopy">是否开启复制,如果不开启就会在池完的时候把第一个返回,如果开启就会返回一个新的</param>
		public UPool(T prefab, int PoolSize = 20, Transform p = null, bool isCopy = true)
		{
			IsCopy = isCopy;
			Prefab = prefab;
			Parent = p;
			Queues = new Queue<T>();
			this.PoolSize = PoolSize;
			Create();
		}
		/// <summary>
		/// 创建池,并且给对象添加对应的脚本
		/// </summary>
		/// <param name="prefab">对应类型的预制体</param>
		/// <param name="p">父级</param>
		/// <param name="PoolSize">初始化生成的数量</param>
		/// <param name="isCopy">是否开启复制,如果不开启就会在池完的时候把第一个返回,如果开启就会返回一个新的</param>
		public UPool(GameObject prefab, int PoolSize = 20, Transform p = null, bool isCopy = true)
		{
			IsCopy = isCopy;
			Prefab = prefab.GetComponent<T>();
            if (Prefab == null)
            {
                Debug.LogError($"对象错误,没有获取到:{typeof(T).Name}");
				return;
            }
			Parent = p;
			Queues = new Queue<T>();
			this.PoolSize = PoolSize;
			Create();

		}
		void Create()
		{
			for (int i = 0; i < PoolSize; i++)
			{
				Queues.Enqueue(Copy());
			}
		}

		T Copy()
		{
			if (Parent == null)
			{
				GameObject pls = GameObject.Find("Pools") == null ? new GameObject("Pools") : GameObject.Find("Pools");

				GameObject p = GameObject.Find(Prefab.name) == null ? new GameObject(Prefab.name) : GameObject.Find(Prefab.name);

				p.transform.SetParent(pls.transform);
				Parent = p.transform;
			}
			var obj = Object.Instantiate(Prefab, Parent);
			obj.gameObject.SetActive(false);
			obj.gameObject.name = string.Concat("Pool", Queues.Count.ToString());
			return obj.GetComponent<T>();
		}

		public GameObject GetObj()
		{
			T obj = null;

			GameObject p = Queues.Peek().gameObject;
			if (Queues.Count > 0 && !p.activeSelf)
			{
				obj = Queues.Dequeue();
			}
			else
			{
				obj = Copy();
			}
			Queues.Enqueue(obj);
			return obj.gameObject;
		}

		public T Get()
		{
			T obj = null;
			GameObject p = Queues.Peek().gameObject;
			if (Queues.Count > 0 && !p.activeSelf)
			{
				obj = Queues.Dequeue();
			}
			else if (IsCopy)
			{
				obj = Copy();
				obj.name = string.Concat(obj.name, "新添加");
			}
			else
			{
				obj = Queues.Dequeue();
				obj.transform.SetSiblingIndex(obj.transform.parent.childCount - 1);
			}
			Queues.Enqueue(obj);
			obj.gameObject.SetActive(true);
			return obj;
		}
		public T Get(Vector3 pos)
		{
			var obj = Get();
			obj.transform.position = pos;
			obj.gameObject.SetActive(true);
			return obj;
		}
		public T1 Get<T1>() where T1 : Component
		{
			var obj = Get().GetComponent<T1>();
			obj.gameObject.SetActive(true);
			return obj;
		}
		public T Find(string name)
		{
			return Queues.ToList().Find(f => f.name.Equals(name));
		}
		/// <summary>
		/// 把全部对象设置为false
		/// </summary>
		public void CloseAll()
		{
			foreach (var item in Queues)
			{
				item.gameObject.SetActive(false);
			}
		}
	}

	public class UPool<T, V> where T : Component
	{
		[SerializeField]
		private T Prefab;
		[SerializeField]
		int PoolSize;
		public Queue<T> Pools => Queues;

		Queue<T> Queues;
		Transform Parent;
		bool IsCopy = false;

		V nowV;
		/// <summary>
		/// 创建池
		/// </summary>
		/// <param name="prefab">对应类型的预制体</param>
		/// <param name="p">父级</param>
		/// <param name="PoolSize">初始化生成的数量</param>
		/// <param name="isCopy">是否开启复制,如果不开启就会在池完的时候把第一个返回,如果开启就会返回一个新的</param>
		public UPool(T prefab, V v, int PoolSize = 20, Transform p = null, bool isCopy = true)
		{
			IsCopy = isCopy;
			Prefab = prefab;
			Parent = p;
			nowV = v;
			Queues = new Queue<T>();
			this.PoolSize = PoolSize;
			Create();
		}
		/// <summary>
		/// 创建池,并且给对象添加对应的脚本
		/// </summary>
		/// <param name="prefab">对应类型的预制体</param>
		/// <param name="p">父级</param>
		/// <param name="PoolSize">初始化生成的数量</param>
		/// <param name="isCopy">是否开启复制,如果不开启就会在池完的时候把第一个返回,如果开启就会返回一个新的</param>
		public UPool(GameObject prefab, V v, int PoolSize = 20, Transform p = null, bool isCopy = true)
		{
			IsCopy = isCopy;
			nowV = v;
			Prefab = prefab.GetComponent<T>();
			if (Prefab == null)
			{
				Debug.LogError($"对象错误,没有获取到:{typeof(T).Name}");
				return;
			}
			Parent = p;
			Queues = new Queue<T>();
			this.PoolSize = PoolSize;
			Create();

		}
		void Create()
		{
			for (int i = 0; i < PoolSize; i++)
			{
				Queues.Enqueue(Copy());
			}
		}

		T Copy()
		{
			if (Parent == null)
			{
				GameObject pls = GameObject.Find("Pools") == null ? new GameObject("Pools") : GameObject.Find("Pools");

				GameObject p = GameObject.Find(Prefab.name) == null ? new GameObject(Prefab.name) : GameObject.Find(Prefab.name);

				p.transform.SetParent(pls.transform);
				Parent = p.transform;
			}
			var obj = Object.Instantiate(Prefab, Parent);
			obj.gameObject.SetActive(false);
			obj.gameObject.name = string.Concat("Pool", Queues.Count.ToString());
			obj.GetComponent<IPoolValue>().Init(nowV);
			return obj.GetComponent<T>();
		}

		public GameObject GetObj()
		{
			T obj = null;

			GameObject p = Queues.Peek().gameObject;
			if (Queues.Count > 0 && !p.activeSelf)
			{
				obj = Queues.Dequeue();
			}
			else
			{
				obj = Copy();
			}
			Queues.Enqueue(obj);
			return obj.gameObject;
		}

		public T Get()
		{
			T obj = null;
			GameObject p = Queues.Peek().gameObject;
			if (Queues.Count > 0 && !p.activeSelf)
			{
				obj = Queues.Dequeue();
			}
			else if (IsCopy)
			{
				obj = Copy();
				obj.name = string.Concat(obj.name, "新添加");
			}
			else
			{
				obj = Queues.Dequeue();
				obj.transform.SetSiblingIndex(obj.transform.parent.childCount - 1);
			}
			Queues.Enqueue(obj);
			obj.gameObject.SetActive(true);
			return obj;
		}
		public T Get(Vector3 pos)
		{
			var obj = Get();
			obj.transform.position = pos;
			obj.gameObject.SetActive(true);
			return obj;
		}
		public T1 Get<T1>() where T1 : Component
		{
			var obj = Get().GetComponent<T1>();
			obj.gameObject.SetActive(true);
			return obj;
		}
		public T Find(string name)
		{
			return Queues.ToList().Find(f => f.name.Equals(name));
		}
		/// <summary>
		/// 把全部对象设置为false
		/// </summary>
		public void CloseAll()
		{
			foreach (var item in Queues)
			{
				item.gameObject.SetActive(false);
			}
		}
	}

	/// <summary> 对象池对象传参接口 </summary>
	public interface IPoolValue
	{
		/// <summary>
		/// 给对象传参
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="t"></param>
		void Init<T>(T t);
	}
}
