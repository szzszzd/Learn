﻿using System;
using UnityEngine;

// Token: 0x02000076 RID: 118
public class GlobalWind : MonoBehaviour
{
	// Token: 0x0600058D RID: 1421 RVA: 0x0002B4A8 File Offset: 0x000296A8
	private void Start()
	{
		if (EnvMan.instance == null)
		{
			return;
		}
		this.m_ps = base.GetComponent<ParticleSystem>();
		this.m_cloth = base.GetComponent<Cloth>();
		if (this.m_checkPlayerShelter)
		{
			this.m_player = base.GetComponentInParent<Player>();
		}
		if (this.m_smoothUpdate)
		{
			base.InvokeRepeating("UpdateWind", 0f, 0.01f);
			return;
		}
		base.InvokeRepeating("UpdateWind", UnityEngine.Random.Range(1.5f, 2.5f), 2f);
		this.UpdateWind();
	}

	// Token: 0x0600058E RID: 1422 RVA: 0x0002B534 File Offset: 0x00029734
	private void UpdateWind()
	{
		if (this.m_alignToWindDirection)
		{
			Vector3 windDir = EnvMan.instance.GetWindDir();
			base.transform.rotation = Quaternion.LookRotation(windDir, Vector3.up);
		}
		if (this.m_ps)
		{
			if (!this.m_ps.emission.enabled)
			{
				return;
			}
			Vector3 windForce = EnvMan.instance.GetWindForce();
			if (this.m_particleVelocity)
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = this.m_ps.velocityOverLifetime;
				velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
				velocityOverLifetime.x = windForce.x * this.m_multiplier;
				velocityOverLifetime.z = windForce.z * this.m_multiplier;
			}
			if (this.m_particleForce)
			{
				ParticleSystem.ForceOverLifetimeModule forceOverLifetime = this.m_ps.forceOverLifetime;
				forceOverLifetime.space = ParticleSystemSimulationSpace.World;
				forceOverLifetime.x = windForce.x * this.m_multiplier;
				forceOverLifetime.z = windForce.z * this.m_multiplier;
			}
			if (this.m_particleEmission)
			{
				this.m_ps.emission.rateOverTimeMultiplier = Mathf.Lerp((float)this.m_particleEmissionMin, (float)this.m_particleEmissionMax, EnvMan.instance.GetWindIntensity());
			}
		}
		if (this.m_cloth)
		{
			Vector3 a = EnvMan.instance.GetWindForce();
			if (this.m_checkPlayerShelter && this.m_player != null && this.m_player.InShelter())
			{
				a = Vector3.zero;
			}
			this.m_cloth.externalAcceleration = a * this.m_multiplier;
			this.m_cloth.randomAcceleration = a * this.m_multiplier * this.m_clothRandomAccelerationFactor;
		}
	}

	// Token: 0x04000690 RID: 1680
	public float m_multiplier = 1f;

	// Token: 0x04000691 RID: 1681
	public bool m_smoothUpdate;

	// Token: 0x04000692 RID: 1682
	public bool m_alignToWindDirection;

	// Token: 0x04000693 RID: 1683
	[Header("Particles")]
	public bool m_particleVelocity = true;

	// Token: 0x04000694 RID: 1684
	public bool m_particleForce;

	// Token: 0x04000695 RID: 1685
	public bool m_particleEmission;

	// Token: 0x04000696 RID: 1686
	public int m_particleEmissionMin;

	// Token: 0x04000697 RID: 1687
	public int m_particleEmissionMax = 1;

	// Token: 0x04000698 RID: 1688
	[Header("Cloth")]
	public float m_clothRandomAccelerationFactor = 0.5f;

	// Token: 0x04000699 RID: 1689
	public bool m_checkPlayerShelter;

	// Token: 0x0400069A RID: 1690
	private ParticleSystem m_ps;

	// Token: 0x0400069B RID: 1691
	private Cloth m_cloth;

	// Token: 0x0400069C RID: 1692
	private Player m_player;
}
