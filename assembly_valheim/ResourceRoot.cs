using System;
using UnityEngine;

// Token: 0x02000286 RID: 646
public class ResourceRoot : MonoBehaviour, Hoverable
{
	// Token: 0x060018AE RID: 6318 RVA: 0x000A4AB8 File Offset: 0x000A2CB8
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<float>("RPC_Drain", new Action<long, float>(this.RPC_Drain));
		base.InvokeRepeating("UpdateTick", UnityEngine.Random.Range(0f, 10f), 10f);
	}

	// Token: 0x060018AF RID: 6319 RVA: 0x000A4B1C File Offset: 0x000A2D1C
	public string GetHoverText()
	{
		float level = this.GetLevel();
		string text;
		if (level > this.m_highThreshold)
		{
			text = this.m_statusHigh;
		}
		else if (level > this.m_emptyTreshold)
		{
			text = this.m_statusLow;
		}
		else
		{
			text = this.m_statusEmpty;
		}
		return Localization.instance.Localize(text);
	}

	// Token: 0x060018B0 RID: 6320 RVA: 0x000A4B66 File Offset: 0x000A2D66
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060018B1 RID: 6321 RVA: 0x000A4B6E File Offset: 0x000A2D6E
	public bool CanDrain(float amount)
	{
		return this.GetLevel() > amount;
	}

	// Token: 0x060018B2 RID: 6322 RVA: 0x000A4B79 File Offset: 0x000A2D79
	public bool Drain(float amount)
	{
		if (!this.CanDrain(amount))
		{
			return false;
		}
		this.m_nview.InvokeRPC("RPC_Drain", new object[]
		{
			amount
		});
		return true;
	}

	// Token: 0x060018B3 RID: 6323 RVA: 0x000A4BA6 File Offset: 0x000A2DA6
	private void RPC_Drain(long caller, float amount)
	{
		if (this.GetLevel() > amount)
		{
			this.ModifyLevel(-amount);
		}
	}

	// Token: 0x060018B4 RID: 6324 RVA: 0x000A4BBC File Offset: 0x000A2DBC
	private double GetTimeSinceLastUpdate()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong(ZDOVars.s_lastTime, time.Ticks));
		TimeSpan timeSpan = time - d;
		this.m_nview.GetZDO().Set(ZDOVars.s_lastTime, time.Ticks);
		double num = timeSpan.TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return num;
	}

	// Token: 0x060018B5 RID: 6325 RVA: 0x000A4C3C File Offset: 0x000A2E3C
	private void ModifyLevel(float mod)
	{
		float num = this.GetLevel();
		num += mod;
		num = Mathf.Clamp(num, 0f, this.m_maxLevel);
		this.m_nview.GetZDO().Set(ZDOVars.s_level, num);
	}

	// Token: 0x060018B6 RID: 6326 RVA: 0x000A4C7C File Offset: 0x000A2E7C
	public float GetLevel()
	{
		return this.m_nview.GetZDO().GetFloat(ZDOVars.s_level, this.m_maxLevel);
	}

	// Token: 0x060018B7 RID: 6327 RVA: 0x000A4C9C File Offset: 0x000A2E9C
	private void UpdateTick()
	{
		if (this.m_nview.IsOwner())
		{
			double timeSinceLastUpdate = this.GetTimeSinceLastUpdate();
			float mod = (float)((double)this.m_regenPerSec * timeSinceLastUpdate);
			this.ModifyLevel(mod);
		}
		float level = this.GetLevel();
		if (level < this.m_emptyTreshold || this.m_wasModified)
		{
			this.m_wasModified = true;
			float t = Utils.LerpStep(this.m_emptyTreshold, this.m_highThreshold, level);
			Color value = Color.Lerp(this.m_emptyColor, this.m_fullColor, t);
			MeshRenderer[] meshes = this.m_meshes;
			for (int i = 0; i < meshes.Length; i++)
			{
				Material[] materials = meshes[i].materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].SetColor("_EmissiveColor", value);
				}
			}
		}
	}

	// Token: 0x060018B8 RID: 6328 RVA: 0x000A4D60 File Offset: 0x000A2F60
	public bool IsLevelLow()
	{
		return this.GetLevel() < this.m_emptyTreshold;
	}

	// Token: 0x04001A99 RID: 6809
	public string m_name = "$item_ancientroot";

	// Token: 0x04001A9A RID: 6810
	public string m_statusHigh = "$item_ancientroot_full";

	// Token: 0x04001A9B RID: 6811
	public string m_statusLow = "$item_ancientroot_half";

	// Token: 0x04001A9C RID: 6812
	public string m_statusEmpty = "$item_ancientroot_empty";

	// Token: 0x04001A9D RID: 6813
	public float m_maxLevel = 100f;

	// Token: 0x04001A9E RID: 6814
	public float m_highThreshold = 50f;

	// Token: 0x04001A9F RID: 6815
	public float m_emptyTreshold = 10f;

	// Token: 0x04001AA0 RID: 6816
	public float m_regenPerSec = 1f;

	// Token: 0x04001AA1 RID: 6817
	public Color m_fullColor = Color.white;

	// Token: 0x04001AA2 RID: 6818
	public Color m_emptyColor = Color.black;

	// Token: 0x04001AA3 RID: 6819
	public MeshRenderer[] m_meshes;

	// Token: 0x04001AA4 RID: 6820
	private ZNetView m_nview;

	// Token: 0x04001AA5 RID: 6821
	private bool m_wasModified;
}
