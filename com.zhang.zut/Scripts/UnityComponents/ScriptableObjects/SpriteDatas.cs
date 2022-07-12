using System.Collections.Generic;
using UnityEngine;

namespace MyTool.UnityComponents
{
	[CreateAssetMenu(fileName = "SpriteDatas", menuName = "Data/Sprite库")]
	public class SpriteDatas : Database<Sprite>
	{
		/// <summary>
		/// 获取对象
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Sprite Get(string name)
		{
			if (Dic == null)
			{
				Dic = new Dictionary<string, Sprite>();
				for (int i = 0; i < Datas.Count; i++)
				{
					if (Datas[i] != null)
					{
						Dic[Datas[i].name] = Datas[i];
					}
					else
					{
						Debug.LogError("出错请刷新" + this);
						continue;
					}
				}
			}
			if (Dic.ContainsKey(name))
			{
				return Dic[name];
			}
			else
			{
				Debug.LogError("请检查预制体名称是否正确");
				return null;
			}
		}
	}
}
