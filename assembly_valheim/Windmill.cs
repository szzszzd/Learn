using System;
using UnityEngine;

// Token: 0x020002C5 RID: 709
public class Windmill : MonoBehaviour
{
	// Token: 0x06001AD2 RID: 6866 RVA: 0x000B2868 File Offset: 0x000B0A68
	private void Start()
	{
		this.m_smelter = base.GetComponent<Smelter>();
		base.InvokeRepeating("CheckCover", 0.1f, 5f);
	}

	// Token: 0x06001AD3 RID: 6867 RVA: 0x000B288C File Offset: 0x000B0A8C
	private void Update()
	{
		Quaternion to = Quaternion.LookRotation(-EnvMan.instance.GetWindDir());
		float powerOutput = this.GetPowerOutput();
		this.m_bom.rotation = Quaternion.RotateTowards(this.m_bom.rotation, to, this.m_bomRotationSpeed * powerOutput * Time.deltaTime);
		float num = powerOutput * this.m_propellerRotationSpeed;
		this.m_propAngle += num * Time.deltaTime;
		this.m_propeller.localRotation = Quaternion.Euler(0f, 0f, this.m_propAngle);
		if (this.m_smelter == null || this.m_smelter.IsActive())
		{
			this.m_grindStoneAngle += powerOutput * this.m_grindstoneRotationSpeed * Time.deltaTime;
		}
		this.m_grindstone.localRotation = Quaternion.Euler(0f, this.m_grindStoneAngle, 0f);
		this.m_propellerAOE.SetActive(Mathf.Abs(num) > this.m_minAOEPropellerSpeed);
		this.UpdateAudio(Time.deltaTime);
	}

	// Token: 0x06001AD4 RID: 6868 RVA: 0x000B2998 File Offset: 0x000B0B98
	public float GetPowerOutput()
	{
		float num = Utils.LerpStep(this.m_minWindSpeed, 1f, EnvMan.instance.GetWindIntensity());
		return (1f - this.m_cover) * num;
	}

	// Token: 0x06001AD5 RID: 6869 RVA: 0x000B29D0 File Offset: 0x000B0BD0
	private void CheckCover()
	{
		bool flag;
		Cover.GetCoverForPoint(this.m_propeller.transform.position, out this.m_cover, out flag, 0.5f);
	}

	// Token: 0x06001AD6 RID: 6870 RVA: 0x000B2A00 File Offset: 0x000B0C00
	private void UpdateAudio(float dt)
	{
		float powerOutput = this.GetPowerOutput();
		float target = Mathf.Lerp(this.m_minPitch, this.m_maxPitch, Mathf.Clamp01(powerOutput / this.m_maxPitchVel));
		float target2 = this.m_maxVol * Mathf.Clamp01(powerOutput / this.m_maxVolVel);
		foreach (AudioSource audioSource in this.m_sfxLoops)
		{
			audioSource.volume = Mathf.MoveTowards(audioSource.volume, target2, this.m_audioChangeSpeed * dt);
			audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, target, this.m_audioChangeSpeed * dt);
		}
	}

	// Token: 0x04001CFB RID: 7419
	public Transform m_propeller;

	// Token: 0x04001CFC RID: 7420
	public Transform m_grindstone;

	// Token: 0x04001CFD RID: 7421
	public Transform m_bom;

	// Token: 0x04001CFE RID: 7422
	public AudioSource[] m_sfxLoops;

	// Token: 0x04001CFF RID: 7423
	public GameObject m_propellerAOE;

	// Token: 0x04001D00 RID: 7424
	public float m_minAOEPropellerSpeed = 5f;

	// Token: 0x04001D01 RID: 7425
	public float m_bomRotationSpeed = 10f;

	// Token: 0x04001D02 RID: 7426
	public float m_propellerRotationSpeed = 10f;

	// Token: 0x04001D03 RID: 7427
	public float m_grindstoneRotationSpeed = 10f;

	// Token: 0x04001D04 RID: 7428
	public float m_minWindSpeed = 0.1f;

	// Token: 0x04001D05 RID: 7429
	public float m_minPitch = 1f;

	// Token: 0x04001D06 RID: 7430
	public float m_maxPitch = 1.5f;

	// Token: 0x04001D07 RID: 7431
	public float m_maxPitchVel = 10f;

	// Token: 0x04001D08 RID: 7432
	public float m_maxVol = 1f;

	// Token: 0x04001D09 RID: 7433
	public float m_maxVolVel = 10f;

	// Token: 0x04001D0A RID: 7434
	public float m_audioChangeSpeed = 2f;

	// Token: 0x04001D0B RID: 7435
	private float m_cover;

	// Token: 0x04001D0C RID: 7436
	private float m_propAngle;

	// Token: 0x04001D0D RID: 7437
	private float m_grindStoneAngle;

	// Token: 0x04001D0E RID: 7438
	private Smelter m_smelter;
}
