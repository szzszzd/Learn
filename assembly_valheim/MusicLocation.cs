using System;
using UnityEngine;

// Token: 0x0200026F RID: 623
public class MusicLocation : MonoBehaviour
{
	// Token: 0x060017EF RID: 6127 RVA: 0x0009F4C4 File Offset: 0x0009D6C4
	private void Awake()
	{
		this.m_audioSource = base.GetComponent<AudioSource>();
		this.m_baseVolume = this.m_audioSource.volume;
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview)
		{
			this.m_nview.Register("SetPlayed", new Action<long>(this.SetPlayed));
		}
		if (this.m_addRadiusFromLocation)
		{
			Location componentInParent = base.GetComponentInParent<Location>();
			if (componentInParent != null)
			{
				this.m_radius += componentInParent.GetMaxRadius();
			}
		}
	}

	// Token: 0x060017F0 RID: 6128 RVA: 0x0009F548 File Offset: 0x0009D748
	private void Update()
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		float p_X = Vector3.Distance(base.transform.position, Player.m_localPlayer.transform.position);
		float target = 1f - Utils.SmoothStep(this.m_radius * 0.5f, this.m_radius, p_X);
		this.volume = Mathf.MoveTowards(this.volume, target, Time.deltaTime);
		float num = this.volume * this.m_baseVolume * MusicMan.m_masterMusicVolume;
		if (this.volume > 0f && !this.m_audioSource.isPlaying && !this.m_blockLoopAndFade)
		{
			if (this.m_oneTime && this.HasPlayed())
			{
				return;
			}
			if (this.m_notIfEnemies && BaseAI.HaveEnemyInRange(Player.m_localPlayer, base.transform.position, this.m_radius))
			{
				return;
			}
			this.m_audioSource.time = 0f;
			this.m_audioSource.Play();
		}
		if (!Settings.ContinousMusic && this.m_audioSource.loop)
		{
			this.m_audioSource.loop = false;
			this.m_blockLoopAndFade = true;
		}
		if (this.m_blockLoopAndFade || this.m_forceFade)
		{
			float num2 = this.m_audioSource.time - this.m_audioSource.clip.length + 1.5f;
			if (num2 > 0f)
			{
				num *= 1f - num2 / 1.5f;
			}
			if (Terminal.m_showTests)
			{
				Terminal.m_testList["Music location fade"] = num2.ToString() + " " + (1f - num2 / 1.5f).ToString();
			}
		}
		this.m_audioSource.volume = num;
		if (this.m_blockLoopAndFade && this.volume <= 0f)
		{
			this.m_blockLoopAndFade = false;
			this.m_audioSource.loop = true;
		}
		if (Terminal.m_showTests && this.m_audioSource.isPlaying)
		{
			Terminal.m_testList["Music location current"] = this.m_audioSource.name;
			Terminal.m_testList["Music location vol / volume"] = num.ToString() + " / " + this.volume.ToString();
			if (Input.GetKeyDown(KeyCode.N) && Input.GetKey(KeyCode.LeftShift))
			{
				this.m_audioSource.time = this.m_audioSource.clip.length - 4f;
			}
		}
		if (this.m_oneTime && this.volume > 0f && this.m_audioSource.time > this.m_audioSource.clip.length * 0.75f && !this.HasPlayed())
		{
			this.SetPlayed();
		}
	}

	// Token: 0x060017F1 RID: 6129 RVA: 0x0009F7FC File Offset: 0x0009D9FC
	private void SetPlayed()
	{
		this.m_nview.InvokeRPC("SetPlayed", Array.Empty<object>());
	}

	// Token: 0x060017F2 RID: 6130 RVA: 0x0009F813 File Offset: 0x0009DA13
	private void SetPlayed(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().Set(ZDOVars.s_played, true);
		ZLog.Log("Setting location music as played");
	}

	// Token: 0x060017F3 RID: 6131 RVA: 0x0009F843 File Offset: 0x0009DA43
	private bool HasPlayed()
	{
		return this.m_nview.GetZDO().GetBool(ZDOVars.s_played, false);
	}

	// Token: 0x060017F4 RID: 6132 RVA: 0x0009F85B File Offset: 0x0009DA5B
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.6f, 0.8f, 0.8f, 0.5f);
		Gizmos.DrawWireSphere(base.transform.position, this.m_radius);
	}

	// Token: 0x04001969 RID: 6505
	private float volume;

	// Token: 0x0400196A RID: 6506
	public bool m_addRadiusFromLocation = true;

	// Token: 0x0400196B RID: 6507
	public float m_radius = 10f;

	// Token: 0x0400196C RID: 6508
	public bool m_oneTime = true;

	// Token: 0x0400196D RID: 6509
	public bool m_notIfEnemies = true;

	// Token: 0x0400196E RID: 6510
	public bool m_forceFade;

	// Token: 0x0400196F RID: 6511
	private ZNetView m_nview;

	// Token: 0x04001970 RID: 6512
	private AudioSource m_audioSource;

	// Token: 0x04001971 RID: 6513
	private float m_baseVolume;

	// Token: 0x04001972 RID: 6514
	private bool m_blockLoopAndFade;
}
