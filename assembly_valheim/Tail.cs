using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000034 RID: 52
public class Tail : MonoBehaviour
{
	// Token: 0x06000337 RID: 823 RVA: 0x00018984 File Offset: 0x00016B84
	private void Awake()
	{
		foreach (Transform transform in this.m_tailJoints)
		{
			float distance = Vector3.Distance(transform.parent.position, transform.position);
			Vector3 position = transform.position;
			Tail.TailSegment tailSegment = new Tail.TailSegment();
			tailSegment.transform = transform;
			tailSegment.pos = position;
			tailSegment.rot = transform.rotation;
			tailSegment.distance = distance;
			this.m_positions.Add(tailSegment);
		}
	}

	// Token: 0x06000338 RID: 824 RVA: 0x00018A28 File Offset: 0x00016C28
	private void OnEnable()
	{
		Tail.Instances.Add(this);
	}

	// Token: 0x06000339 RID: 825 RVA: 0x00018A35 File Offset: 0x00016C35
	private void OnDisable()
	{
		Tail.Instances.Remove(this);
	}

	// Token: 0x0600033A RID: 826 RVA: 0x00018A44 File Offset: 0x00016C44
	public void CustomLateUpdate(float dt)
	{
		for (int i = 0; i < this.m_positions.Count; i++)
		{
			Tail.TailSegment tailSegment = this.m_positions[i];
			if (this.m_waterSurfaceCheck)
			{
				float liquidLevel = Floating.GetLiquidLevel(tailSegment.pos, 1f, LiquidType.All);
				if (tailSegment.pos.y + this.m_tailRadius > liquidLevel)
				{
					Tail.TailSegment tailSegment2 = tailSegment;
					tailSegment2.pos.y = tailSegment2.pos.y - this.m_gravity * dt;
				}
				else
				{
					Tail.TailSegment tailSegment3 = tailSegment;
					tailSegment3.pos.y = tailSegment3.pos.y - this.m_gravityInWater * dt;
				}
			}
			else
			{
				Tail.TailSegment tailSegment4 = tailSegment;
				tailSegment4.pos.y = tailSegment4.pos.y - this.m_gravity * dt;
			}
			Vector3 vector = tailSegment.transform.parent.position + tailSegment.transform.parent.up * tailSegment.distance * 0.5f;
			Vector3 vector2 = Vector3.Normalize(vector - tailSegment.pos);
			vector2 = Vector3.RotateTowards(-tailSegment.transform.parent.up, vector2, 0.017453292f * this.m_maxAngle, 1f);
			Vector3 vector3 = vector - vector2 * tailSegment.distance * 0.5f;
			if (this.m_groundCheck)
			{
				float groundHeight = ZoneSystem.instance.GetGroundHeight(vector3);
				if (vector3.y - this.m_tailRadius < groundHeight)
				{
					vector3.y = groundHeight + this.m_tailRadius;
				}
			}
			vector3 = Vector3.Lerp(tailSegment.pos, vector3, this.m_smoothness);
			if (vector == vector3)
			{
				return;
			}
			Vector3 normalized = (vector - vector3).normalized;
			Vector3 rhs = Vector3.Cross(Vector3.up, -normalized);
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross(-normalized, rhs), -normalized);
			quaternion = Quaternion.Slerp(tailSegment.rot, quaternion, this.m_smoothness);
			tailSegment.transform.position = vector3;
			tailSegment.transform.rotation = quaternion;
			tailSegment.pos = vector3;
			tailSegment.rot = quaternion;
		}
		if (this.m_tailBody)
		{
			this.m_tailBody.velocity = Vector3.zero;
			this.m_tailBody.angularVelocity = Vector3.zero;
		}
	}

	// Token: 0x17000007 RID: 7
	// (get) Token: 0x0600033B RID: 827 RVA: 0x00018C93 File Offset: 0x00016E93
	public static List<Tail> Instances { get; } = new List<Tail>();

	// Token: 0x0400031A RID: 794
	public List<Transform> m_tailJoints = new List<Transform>();

	// Token: 0x0400031B RID: 795
	public float m_maxAngle = 80f;

	// Token: 0x0400031C RID: 796
	public float m_gravity = 2f;

	// Token: 0x0400031D RID: 797
	public float m_gravityInWater = 0.1f;

	// Token: 0x0400031E RID: 798
	public bool m_waterSurfaceCheck;

	// Token: 0x0400031F RID: 799
	public bool m_groundCheck;

	// Token: 0x04000320 RID: 800
	public float m_smoothness = 0.1f;

	// Token: 0x04000321 RID: 801
	public float m_tailRadius;

	// Token: 0x04000322 RID: 802
	public Character m_character;

	// Token: 0x04000323 RID: 803
	public Rigidbody m_characterBody;

	// Token: 0x04000324 RID: 804
	public Rigidbody m_tailBody;

	// Token: 0x04000325 RID: 805
	private readonly List<Tail.TailSegment> m_positions = new List<Tail.TailSegment>();

	// Token: 0x02000035 RID: 53
	private class TailSegment
	{
		// Token: 0x04000327 RID: 807
		public Transform transform;

		// Token: 0x04000328 RID: 808
		public Vector3 pos;

		// Token: 0x04000329 RID: 809
		public Quaternion rot;

		// Token: 0x0400032A RID: 810
		public float distance;
	}
}
