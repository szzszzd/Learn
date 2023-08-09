using System;
using TMPro;
using UnityEngine;

namespace Fishlabs
{
	// Token: 0x020002E5 RID: 741
	public class GamepadMapLabel : MonoBehaviour
	{
		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06001BF0 RID: 7152 RVA: 0x000B99A9 File Offset: 0x000B7BA9
		public TextMeshProUGUI Label
		{
			get
			{
				return this.label;
			}
		}

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x06001BF1 RID: 7153 RVA: 0x000B99B1 File Offset: 0x000B7BB1
		public TextMeshProUGUI Button
		{
			get
			{
				return this.button;
			}
		}

		// Token: 0x04001E20 RID: 7712
		[SerializeField]
		private TextMeshProUGUI label;

		// Token: 0x04001E21 RID: 7713
		[SerializeField]
		private TextMeshProUGUI button;
	}
}
