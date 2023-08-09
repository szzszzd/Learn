using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Fishlabs
{
	// Token: 0x020002E2 RID: 738
	public class Selector : MonoBehaviour
	{
		// Token: 0x06001BDF RID: 7135 RVA: 0x000B936D File Offset: 0x000B756D
		public void SetText(string text)
		{
			if (this.label != null)
			{
				this.label.text = text;
			}
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x000B9389 File Offset: 0x000B7589
		public void OnLeftButtonClicked()
		{
			UnityEvent onLeftButtonClickedEvent = this.OnLeftButtonClickedEvent;
			if (onLeftButtonClickedEvent == null)
			{
				return;
			}
			onLeftButtonClickedEvent.Invoke();
		}

		// Token: 0x06001BE1 RID: 7137 RVA: 0x000B939B File Offset: 0x000B759B
		public void OnRightButtonClicked()
		{
			UnityEvent onRightButtonClickedEvent = this.OnRightButtonClickedEvent;
			if (onRightButtonClickedEvent == null)
			{
				return;
			}
			onRightButtonClickedEvent.Invoke();
		}

		// Token: 0x04001DF8 RID: 7672
		[SerializeField]
		private TextMeshProUGUI label;

		// Token: 0x04001DF9 RID: 7673
		public UnityEvent OnLeftButtonClickedEvent;

		// Token: 0x04001DFA RID: 7674
		public UnityEvent OnRightButtonClickedEvent;
	}
}
