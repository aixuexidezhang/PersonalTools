using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyTool.Tools
{
	/// <summary>
	/// 快捷操作指令
	/// </summary>
	[Serializable]
	public class ShortcutsCellback : UnityEvent { }
	/// <summary>
	/// 快捷指令float
	/// </summary>
	[Serializable]
	public class ShortcutsCellbackFloat : UnityEvent<float> { }

	[Serializable]
	public class ShortcutsCellbackBoolean : UnityEvent<bool> { }

	[Serializable]
	public class ShortcutsCellbackGameObject : UnityEvent<GameObject> { }

}