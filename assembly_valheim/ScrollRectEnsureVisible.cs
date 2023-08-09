using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000EF RID: 239
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectEnsureVisible : MonoBehaviour
{
	// Token: 0x060009B9 RID: 2489 RVA: 0x00049F0C File Offset: 0x0004810C
	private void Awake()
	{
		if (!this.mInitialized)
		{
			this.Initialize();
		}
	}

	// Token: 0x060009BA RID: 2490 RVA: 0x00049F1C File Offset: 0x0004811C
	private void Initialize()
	{
		this.mScrollRect = base.GetComponent<ScrollRect>();
		this.mScrollTransform = (this.mScrollRect.transform as RectTransform);
		this.mContent = this.mScrollRect.content;
		this.Reset();
		this.mInitialized = true;
	}

	// Token: 0x060009BB RID: 2491 RVA: 0x00049F6C File Offset: 0x0004816C
	public void CenterOnItem(RectTransform target)
	{
		if (!this.mInitialized)
		{
			this.Initialize();
		}
		Vector3 worldPointInWidget = this.GetWorldPointInWidget(this.mScrollTransform, this.GetWidgetWorldPoint(target));
		Vector3 vector = this.GetWorldPointInWidget(this.mScrollTransform, this.GetWidgetWorldPoint(this.maskTransform)) - worldPointInWidget;
		vector.z = 0f;
		if (!this.mScrollRect.horizontal)
		{
			vector.x = 0f;
		}
		if (!this.mScrollRect.vertical)
		{
			vector.y = 0f;
		}
		Vector2 b = new Vector2(vector.x / (this.mContent.rect.size.x - this.mScrollTransform.rect.size.x), vector.y / (this.mContent.rect.size.y - this.mScrollTransform.rect.size.y));
		Vector2 vector2 = this.mScrollRect.normalizedPosition - b;
		if (this.mScrollRect.movementType != ScrollRect.MovementType.Unrestricted)
		{
			vector2.x = Mathf.Clamp01(vector2.x);
			vector2.y = Mathf.Clamp01(vector2.y);
		}
		this.mScrollRect.normalizedPosition = vector2;
	}

	// Token: 0x060009BC RID: 2492 RVA: 0x0004A0C4 File Offset: 0x000482C4
	private void Reset()
	{
		if (this.maskTransform == null)
		{
			Mask componentInChildren = base.GetComponentInChildren<Mask>(true);
			if (componentInChildren)
			{
				this.maskTransform = componentInChildren.rectTransform;
			}
			if (this.maskTransform == null)
			{
				RectMask2D componentInChildren2 = base.GetComponentInChildren<RectMask2D>(true);
				if (componentInChildren2)
				{
					this.maskTransform = componentInChildren2.rectTransform;
				}
			}
		}
	}

	// Token: 0x060009BD RID: 2493 RVA: 0x0004A128 File Offset: 0x00048328
	private Vector3 GetWidgetWorldPoint(RectTransform target)
	{
		Vector3 b = new Vector3((0.5f - target.pivot.x) * target.rect.size.x, (0.5f - target.pivot.y) * target.rect.size.y, 0f);
		Vector3 position = target.localPosition + b;
		return target.parent.TransformPoint(position);
	}

	// Token: 0x060009BE RID: 2494 RVA: 0x0004A1A4 File Offset: 0x000483A4
	private Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
	{
		return target.InverseTransformPoint(worldPoint);
	}

	// Token: 0x04000BBE RID: 3006
	private RectTransform maskTransform;

	// Token: 0x04000BBF RID: 3007
	private ScrollRect mScrollRect;

	// Token: 0x04000BC0 RID: 3008
	private RectTransform mScrollTransform;

	// Token: 0x04000BC1 RID: 3009
	private RectTransform mContent;

	// Token: 0x04000BC2 RID: 3010
	private bool mInitialized;
}
