using MyTool.Tools;
using UnityEngine;
namespace MyTool.UnityComponents
{
	public class ColliderTrigger : MonoBehaviour
	{
		public ShortcutsCellback EnterCollider;
		public ShortcutsCellback ExitCollider;
		private void OnCollisionEnter(Collision collision)
		{
			EnterCollider?.Invoke();
		}

		private void OnCollisionExit(Collision collision)
		{
			ExitCollider?.Invoke();
		}
		private void OnTriggerEnter(Collider other)
		{
			EnterCollider?.Invoke();
		}
		private void OnTriggerExit(Collider other)
		{
			ExitCollider?.Invoke();
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			EnterCollider?.Invoke();
		}
		private void OnCollisionExit2D(Collision2D collision)
		{
			ExitCollider?.Invoke();
		}
		private void OnTriggerEnter2D(Collider2D collision)
		{
			EnterCollider?.Invoke();
		}
		private void OnTriggerExit2D(Collider2D collision)
		{
			ExitCollider?.Invoke();
		}
	}
}