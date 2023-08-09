using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007B RID: 123
public class LineConnect : MonoBehaviour
{
	// Token: 0x0600059C RID: 1436 RVA: 0x0002BE84 File Offset: 0x0002A084
	private void Awake()
	{
		this.m_lineRenderer = base.GetComponent<LineRenderer>();
		this.m_nview = base.GetComponentInParent<ZNetView>();
		this.m_linePeerID = ZDO.GetHashZDOID(this.m_netViewPrefix + "line_peer");
		this.m_slackHash = (this.m_netViewPrefix + "line_slack").GetStableHashCode();
	}

	// Token: 0x0600059D RID: 1437 RVA: 0x0002BEE0 File Offset: 0x0002A0E0
	private void LateUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			this.m_lineRenderer.enabled = false;
			return;
		}
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID(this.m_linePeerID);
		GameObject gameObject = ZNetScene.instance.FindInstance(zdoid);
		if (gameObject && !string.IsNullOrEmpty(this.m_childObject))
		{
			Transform transform = Utils.FindChild(gameObject.transform, this.m_childObject);
			if (transform)
			{
				gameObject = transform.gameObject;
			}
		}
		if (gameObject != null)
		{
			Vector3 endpoint = gameObject.transform.position;
			if (this.m_centerOfCharacter)
			{
				Character component = gameObject.GetComponent<Character>();
				if (component)
				{
					endpoint = component.GetCenterPoint();
				}
			}
			this.SetEndpoint(endpoint);
			this.m_lineRenderer.enabled = true;
			return;
		}
		if (this.m_hideIfNoConnection)
		{
			this.m_lineRenderer.enabled = false;
			return;
		}
		this.m_lineRenderer.enabled = true;
		this.SetEndpoint(base.transform.position + this.m_noConnectionWorldOffset);
	}

	// Token: 0x0600059E RID: 1438 RVA: 0x0002BFE8 File Offset: 0x0002A1E8
	private void SetEndpoint(Vector3 pos)
	{
		Vector3 vector = base.transform.InverseTransformPoint(pos);
		Vector3 a = base.transform.InverseTransformDirection(Vector3.down);
		if (this.m_dynamicSlack)
		{
			float @float = this.m_nview.GetZDO().GetFloat(this.m_slackHash, this.m_slack);
			Vector3 position = this.m_lineRenderer.GetPosition(0);
			Vector3 b = vector;
			float d = Vector3.Distance(position, b) / 2f;
			for (int i = 1; i < this.m_lineRenderer.positionCount; i++)
			{
				float num = (float)i / (float)(this.m_lineRenderer.positionCount - 1);
				float num2 = Mathf.Abs(0.5f - num) * 2f;
				num2 *= num2;
				num2 = 1f - num2;
				Vector3 vector2 = Vector3.Lerp(position, b, num);
				vector2 += a * d * @float * num2;
				this.m_lineRenderer.SetPosition(i, vector2);
			}
		}
		else
		{
			this.m_lineRenderer.SetPosition(1, vector);
		}
		if (this.m_dynamicThickness)
		{
			float v = Vector3.Distance(base.transform.position, pos);
			float num3 = Utils.LerpStep(this.m_minDistance, this.m_maxDistance, v);
			num3 = Mathf.Pow(num3, this.m_thicknessPower);
			this.m_lineRenderer.widthMultiplier = Mathf.Lerp(this.m_maxThickness, this.m_minThickness, num3);
		}
	}

	// Token: 0x0600059F RID: 1439 RVA: 0x0002C157 File Offset: 0x0002A357
	public void SetPeer(ZNetView other)
	{
		if (other)
		{
			this.SetPeer(other.GetZDO().m_uid);
			return;
		}
		this.SetPeer(ZDOID.None);
	}

	// Token: 0x060005A0 RID: 1440 RVA: 0x0002C17E File Offset: 0x0002A37E
	public void SetPeer(ZDOID zdoid)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().Set(this.m_linePeerID, zdoid);
	}

	// Token: 0x060005A1 RID: 1441 RVA: 0x0002C1B2 File Offset: 0x0002A3B2
	public void SetSlack(float slack)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().Set(this.m_slackHash, slack);
	}

	// Token: 0x040006C0 RID: 1728
	public bool m_centerOfCharacter;

	// Token: 0x040006C1 RID: 1729
	public string m_childObject = "";

	// Token: 0x040006C2 RID: 1730
	public bool m_hideIfNoConnection = true;

	// Token: 0x040006C3 RID: 1731
	public Vector3 m_noConnectionWorldOffset = new Vector3(0f, -1f, 0f);

	// Token: 0x040006C4 RID: 1732
	[Header("Dynamic slack")]
	public bool m_dynamicSlack;

	// Token: 0x040006C5 RID: 1733
	public float m_slack = 0.5f;

	// Token: 0x040006C6 RID: 1734
	[Header("Thickness")]
	public bool m_dynamicThickness = true;

	// Token: 0x040006C7 RID: 1735
	public float m_minDistance = 6f;

	// Token: 0x040006C8 RID: 1736
	public float m_maxDistance = 30f;

	// Token: 0x040006C9 RID: 1737
	public float m_minThickness = 0.2f;

	// Token: 0x040006CA RID: 1738
	public float m_maxThickness = 0.8f;

	// Token: 0x040006CB RID: 1739
	public float m_thicknessPower = 0.2f;

	// Token: 0x040006CC RID: 1740
	public string m_netViewPrefix = "";

	// Token: 0x040006CD RID: 1741
	private LineRenderer m_lineRenderer;

	// Token: 0x040006CE RID: 1742
	private ZNetView m_nview;

	// Token: 0x040006CF RID: 1743
	private KeyValuePair<int, int> m_linePeerID;

	// Token: 0x040006D0 RID: 1744
	private int m_slackHash;
}
