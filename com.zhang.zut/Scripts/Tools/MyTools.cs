using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace MyTool.Tools
{
	/// <summary>
	/// 杂项工具类
	/// </summary>
	public static class MyTools
	{
		/// <summary>
		/// Task计时器
		/// </summary>
		/// <param name="ms">计时时间(毫秒)</param>
		/// <param name="over">结束回调</param>
		/// <param name="loop">是否循环执行</param>
		public static async void Timer(int ms, Action over, bool loop = false)
		{
			try
			{
				do
				{
					await Task.Delay(ms);
					if (Application.isPlaying) over?.Invoke();
				} while (loop);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		/// <summary>
		/// DoTween倒计时计时器
		/// </summary>
		/// <param name="TimeCount">倒计时所需的时间</param>
		/// <param name="shortcuts">计时进行时的回调</param>
		/// <param name="overCallback">完成倒计时的回调</param>
		/// <param name="loop">是否循环执行</param>
		/// <returns></returns>
		public static Tween Timer(float TimeCount, ShortcutsCellbackFloat shortcuts, Action overCallback, bool loop = false)
		{
			float StartTimeCount = 0;
			float TargetTimeCount = TimeCount;
			var t = DOTween.To(() => StartTimeCount, a => StartTimeCount = a, TargetTimeCount, TimeCount);
			if (loop) t.SetLoops(-1); ;
			t.OnComplete(delegate { overCallback?.Invoke(); });
			return t;
		}

		/// <summary>
		/// DoTween倒计时计时器
		/// </summary>
		/// <param name="TimeCount">倒计时所需的时间</param>
		/// <param name="action">完成倒计时的回调</param>
		/// <param name="loop">是否循环执行</param>
		public static Tween Timer(float TimeCount, Action action, bool loop = false)
		{
			float StartTimeCount = 0;
			float TargetTimeCount = TimeCount;
			var t = DOTween.To(() => StartTimeCount, a => StartTimeCount = a, TargetTimeCount, TimeCount).OnComplete(delegate { action?.Invoke(); });
			if (loop) t.SetLoops(-1); ;
			return t;
		}

		/// <summary>
		/// 随机获取List中的一个数据
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static T RandomGet<T>(this List<T> list)
		{
			if (list.Count > 0)
			{
				var v = list[UnityEngine.Random.Range(0, list.Count)];
				return v;
			}
			else
			{
				Debug.LogError("List为空无法获取!");
				return default;
			}
		}
		/// <summary>
		/// 返回深复制对象
		/// </summary>
		public static T CreateDeepClone<T>(this T obj)
		{

			if (obj == null) return obj;

			T t;

			using (MemoryStream ms = new MemoryStream())
			{

				BinaryFormatter bf = new BinaryFormatter();

				// 把数据序列化成二进制
				bf.Serialize(ms, obj);

				ms.Position = 0;

				// 把数据从二进制反序列化
				t = (T)bf.Deserialize(ms);

				return t;

			}
		}
	}
}
