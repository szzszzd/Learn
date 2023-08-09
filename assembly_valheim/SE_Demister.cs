using System;
using UnityEngine;

// Token: 0x02000059 RID: 89
public class SE_Demister : StatusEffect
{
	// Token: 0x060004DC RID: 1244 RVA: 0x00027848 File Offset: 0x00025A48
	public override void Setup(Character character)
	{
		base.Setup(character);
		if (this.m_coverRayMask == 0)
		{
			this.m_coverRayMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain"
			});
		}
	}

	// Token: 0x060004DD RID: 1245 RVA: 0x000278A0 File Offset: 0x00025AA0
	private bool IsUnderRoof()
	{
		RaycastHit raycastHit;
		return Physics.Raycast(this.m_character.GetCenterPoint(), Vector3.up, out raycastHit, 4f, this.m_coverRayMask);
	}

	// Token: 0x060004DE RID: 1246 RVA: 0x000278D4 File Offset: 0x00025AD4
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (!this.m_ballInstance)
		{
			Vector3 position = this.m_character.GetCenterPoint() + this.m_character.transform.forward * 0.5f;
			this.m_ballInstance = UnityEngine.Object.Instantiate<GameObject>(this.m_ballPrefab, position, Quaternion.identity);
			return;
		}
		Character character = this.m_character;
		bool flag = this.IsUnderRoof();
		Vector3 position2 = this.m_character.transform.position;
		Vector3 vector = this.m_ballInstance.transform.position;
		Vector3 vector2 = flag ? this.m_offsetInterior : this.m_offset;
		float d = flag ? this.m_noiseDistanceInterior : this.m_noiseDistance;
		Vector3 vector3 = position2 + this.m_character.transform.TransformVector(vector2);
		float num = Time.time * this.m_noiseSpeed;
		vector3 += new Vector3(Mathf.Sin(num * 4f), Mathf.Sin(num * 2f) * this.m_noiseDistanceYScale, Mathf.Cos(num * 5f)) * d;
		float num2 = Vector3.Distance(vector3, vector);
		if (num2 > this.m_maxDistance * 2f)
		{
			vector = vector3;
		}
		else if (num2 > this.m_maxDistance)
		{
			Vector3 normalized = (vector - vector3).normalized;
			vector = vector3 + normalized * this.m_maxDistance;
		}
		Vector3 normalized2 = (vector3 - vector).normalized;
		this.m_ballVel += normalized2 * this.m_ballAcceleration * dt;
		if (this.m_ballVel.magnitude > this.m_ballMaxSpeed)
		{
			this.m_ballVel = this.m_ballVel.normalized * this.m_ballMaxSpeed;
		}
		if (!flag)
		{
			Vector3 velocity = this.m_character.GetVelocity();
			this.m_ballVel += velocity * this.m_characterVelocityFactor * dt;
		}
		this.m_ballVel -= this.m_ballVel * this.m_ballFriction;
		Vector3 position3 = vector + this.m_ballVel * dt;
		this.m_ballInstance.transform.position = position3;
		Quaternion quaternion = this.m_ballInstance.transform.rotation;
		quaternion *= Quaternion.Euler(this.m_rotationSpeed, 0f, this.m_rotationSpeed * 0.5321f);
		this.m_ballInstance.transform.rotation = quaternion;
	}

	// Token: 0x060004DF RID: 1247 RVA: 0x00027B74 File Offset: 0x00025D74
	private void RemoveEffects()
	{
		if (this.m_ballInstance != null)
		{
			ZNetView component = this.m_ballInstance.GetComponent<ZNetView>();
			if (component.IsValid())
			{
				component.ClaimOwnership();
				component.Destroy();
			}
		}
	}

	// Token: 0x060004E0 RID: 1248 RVA: 0x00027BAF File Offset: 0x00025DAF
	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
		this.m_ballInstance = null;
	}

	// Token: 0x060004E1 RID: 1249 RVA: 0x00027BBE File Offset: 0x00025DBE
	public override void Stop()
	{
		base.Stop();
		this.RemoveEffects();
	}

	// Token: 0x060004E2 RID: 1250 RVA: 0x00027BCC File Offset: 0x00025DCC
	public override void OnDestroy()
	{
		base.OnDestroy();
		this.RemoveEffects();
	}

	// Token: 0x0400059E RID: 1438
	[Header("SE_Demister")]
	public GameObject m_ballPrefab;

	// Token: 0x0400059F RID: 1439
	public Vector3 m_offset = new Vector3(0f, 2f, 0f);

	// Token: 0x040005A0 RID: 1440
	public Vector3 m_offsetInterior = new Vector3(0.5f, 1.8f, 0f);

	// Token: 0x040005A1 RID: 1441
	public float m_maxDistance = 50f;

	// Token: 0x040005A2 RID: 1442
	public float m_ballAcceleration = 4f;

	// Token: 0x040005A3 RID: 1443
	public float m_ballMaxSpeed = 10f;

	// Token: 0x040005A4 RID: 1444
	public float m_ballFriction = 0.1f;

	// Token: 0x040005A5 RID: 1445
	public float m_noiseDistance = 1f;

	// Token: 0x040005A6 RID: 1446
	public float m_noiseDistanceInterior = 0.2f;

	// Token: 0x040005A7 RID: 1447
	public float m_noiseDistanceYScale = 1f;

	// Token: 0x040005A8 RID: 1448
	public float m_noiseSpeed = 1f;

	// Token: 0x040005A9 RID: 1449
	public float m_characterVelocityFactor = 1f;

	// Token: 0x040005AA RID: 1450
	public float m_rotationSpeed = 1f;

	// Token: 0x040005AB RID: 1451
	private int m_coverRayMask;

	// Token: 0x040005AC RID: 1452
	private GameObject m_ballInstance;

	// Token: 0x040005AD RID: 1453
	private Vector3 m_ballVel = new Vector3(0f, 0f, 0f);
}
