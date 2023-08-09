using System;
using UnityEngine;
using UnityEngine.UI;

namespace Valheim.UI
{
	// Token: 0x020002D5 RID: 725
	public static class ScrollRectExtensions
	{
		// Token: 0x06001B69 RID: 7017 RVA: 0x000B799C File Offset: 0x000B5B9C
		public static void SnapToChild(this ScrollRect scrollRect, RectTransform child)
		{
			Vector2 vector = scrollRect.viewport.transform.InverseTransformPoint(child.position);
			float height = scrollRect.viewport.rect.height;
			bool flag = vector.y > 0f;
			bool flag2 = -vector.y + child.rect.height > height;
			float num = flag ? (-vector.y) : (flag2 ? (-vector.y + child.rect.height - height) : 0f);
			scrollRect.content.anchoredPosition = new Vector2(0f, scrollRect.content.anchoredPosition.y + num);
		}

		// Token: 0x06001B6A RID: 7018 RVA: 0x000B7A58 File Offset: 0x000B5C58
		public static bool IsVisible(this ScrollRect scrollRect, RectTransform child)
		{
			float height = scrollRect.viewport.rect.height;
			Vector2 vector = scrollRect.viewport.transform.InverseTransformPoint(child.position);
			return vector.y < 0f && -vector.y + child.rect.height < height;
		}
	}
}
