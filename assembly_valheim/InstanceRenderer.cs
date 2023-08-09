using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020001CE RID: 462
public class InstanceRenderer : MonoBehaviour
{
	// Token: 0x060012F2 RID: 4850 RVA: 0x0007D19E File Offset: 0x0007B39E
	private void OnEnable()
	{
		InstanceRenderer.Instances.Add(this);
	}

	// Token: 0x060012F3 RID: 4851 RVA: 0x0007D1AB File Offset: 0x0007B3AB
	private void OnDisable()
	{
		InstanceRenderer.Instances.Remove(this);
	}

	// Token: 0x060012F4 RID: 4852 RVA: 0x0007D1BC File Offset: 0x0007B3BC
	public void CustomUpdate()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (this.m_instanceCount == 0 || mainCamera == null)
		{
			return;
		}
		if (this.m_frustumCull)
		{
			if (this.m_dirtyBounds)
			{
				this.UpdateBounds();
			}
			if (!Utils.InsideMainCamera(this.m_bounds))
			{
				return;
			}
		}
		if (this.m_useLod)
		{
			float num = this.m_useXZLodDistance ? Utils.DistanceXZ(mainCamera.transform.position, base.transform.position) : Vector3.Distance(mainCamera.transform.position, base.transform.position);
			int num2 = (int)((1f - Utils.LerpStep(this.m_lodMinDistance, this.m_lodMaxDistance, num)) * (float)this.m_instanceCount);
			float maxDelta = Time.deltaTime * (float)this.m_instanceCount;
			this.m_lodCount = Mathf.MoveTowards(this.m_lodCount, (float)num2, maxDelta);
			if (this.m_firstFrame)
			{
				if (num < this.m_lodMinDistance)
				{
					this.m_lodCount = (float)num2;
				}
				this.m_firstFrame = false;
			}
			this.m_lodCount = Mathf.Min(this.m_lodCount, (float)this.m_instanceCount);
			int num3 = (int)this.m_lodCount;
			if (num3 > 0)
			{
				Graphics.DrawMeshInstanced(this.m_mesh, 0, this.m_material, this.m_instances, num3, null, this.m_shadowCasting);
				return;
			}
		}
		else
		{
			Graphics.DrawMeshInstanced(this.m_mesh, 0, this.m_material, this.m_instances, this.m_instanceCount, null, this.m_shadowCasting);
		}
	}

	// Token: 0x060012F5 RID: 4853 RVA: 0x0007D324 File Offset: 0x0007B524
	private void UpdateBounds()
	{
		this.m_dirtyBounds = false;
		Vector3 vector = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 vector2 = new Vector3(-9999999f, -9999999f, -9999999f);
		float magnitude = this.m_mesh.bounds.extents.magnitude;
		for (int i = 0; i < this.m_instanceCount; i++)
		{
			Matrix4x4 matrix4x = this.m_instances[i];
			Vector3 a = new Vector3(matrix4x[0, 3], matrix4x[1, 3], matrix4x[2, 3]);
			Vector3 lossyScale = matrix4x.lossyScale;
			float num = Mathf.Max(Mathf.Max(lossyScale.x, lossyScale.y), lossyScale.z);
			Vector3 b = new Vector3(num * magnitude, num * magnitude, num * magnitude);
			vector2 = Vector3.Max(vector2, a + b);
			vector = Vector3.Min(vector, a - b);
		}
		this.m_bounds.position = (vector2 + vector) * 0.5f;
		this.m_bounds.radius = Vector3.Distance(vector2, this.m_bounds.position);
	}

	// Token: 0x060012F6 RID: 4854 RVA: 0x0007D464 File Offset: 0x0007B664
	public void AddInstance(Vector3 pos, Quaternion rot, float scale)
	{
		Matrix4x4 m = Matrix4x4.TRS(pos, rot, this.m_scale * scale);
		this.AddInstance(m);
	}

	// Token: 0x060012F7 RID: 4855 RVA: 0x0007D48C File Offset: 0x0007B68C
	public void AddInstance(Vector3 pos, Quaternion rot)
	{
		Matrix4x4 m = Matrix4x4.TRS(pos, rot, this.m_scale);
		this.AddInstance(m);
	}

	// Token: 0x060012F8 RID: 4856 RVA: 0x0007D4AE File Offset: 0x0007B6AE
	public void AddInstance(Matrix4x4 m)
	{
		if (this.m_instanceCount >= 1023)
		{
			return;
		}
		this.m_instances[this.m_instanceCount] = m;
		this.m_instanceCount++;
		this.m_dirtyBounds = true;
	}

	// Token: 0x060012F9 RID: 4857 RVA: 0x0007D4E5 File Offset: 0x0007B6E5
	public void Clear()
	{
		this.m_instanceCount = 0;
		this.m_dirtyBounds = true;
	}

	// Token: 0x060012FA RID: 4858 RVA: 0x0007D4F8 File Offset: 0x0007B6F8
	public void SetInstance(int index, Vector3 pos, Quaternion rot, float scale)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(pos, rot, this.m_scale * scale);
		this.m_instances[index] = matrix4x;
		this.m_dirtyBounds = true;
	}

	// Token: 0x060012FB RID: 4859 RVA: 0x0007D52E File Offset: 0x0007B72E
	private void Resize(int instances)
	{
		this.m_instanceCount = instances;
		this.m_dirtyBounds = true;
	}

	// Token: 0x060012FC RID: 4860 RVA: 0x0007D540 File Offset: 0x0007B740
	public void SetInstances(List<Transform> transforms, bool faceCamera = false)
	{
		this.Resize(transforms.Count);
		for (int i = 0; i < transforms.Count; i++)
		{
			Transform transform = transforms[i];
			this.m_instances[i] = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		}
		this.m_dirtyBounds = true;
	}

	// Token: 0x060012FD RID: 4861 RVA: 0x0007D59C File Offset: 0x0007B79C
	public void SetInstancesBillboard(List<Vector4> points)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 forward = -mainCamera.transform.forward;
		this.Resize(points.Count);
		for (int i = 0; i < points.Count; i++)
		{
			Vector4 vector = points[i];
			Vector3 pos = new Vector3(vector.x, vector.y, vector.z);
			float w = vector.w;
			Quaternion q = Quaternion.LookRotation(forward);
			this.m_instances[i] = Matrix4x4.TRS(pos, q, w * this.m_scale);
		}
		this.m_dirtyBounds = true;
	}

	// Token: 0x060012FE RID: 4862 RVA: 0x000023E2 File Offset: 0x000005E2
	private void OnDrawGizmosSelected()
	{
	}

	// Token: 0x170000C5 RID: 197
	// (get) Token: 0x060012FF RID: 4863 RVA: 0x0007D641 File Offset: 0x0007B841
	public static List<InstanceRenderer> Instances { get; } = new List<InstanceRenderer>();

	// Token: 0x040013CB RID: 5067
	public Mesh m_mesh;

	// Token: 0x040013CC RID: 5068
	public Material m_material;

	// Token: 0x040013CD RID: 5069
	public Vector3 m_scale = Vector3.one;

	// Token: 0x040013CE RID: 5070
	public bool m_frustumCull = true;

	// Token: 0x040013CF RID: 5071
	public bool m_useLod;

	// Token: 0x040013D0 RID: 5072
	public bool m_useXZLodDistance = true;

	// Token: 0x040013D1 RID: 5073
	public float m_lodMinDistance = 5f;

	// Token: 0x040013D2 RID: 5074
	public float m_lodMaxDistance = 20f;

	// Token: 0x040013D3 RID: 5075
	public ShadowCastingMode m_shadowCasting;

	// Token: 0x040013D4 RID: 5076
	private bool m_dirtyBounds = true;

	// Token: 0x040013D5 RID: 5077
	private BoundingSphere m_bounds;

	// Token: 0x040013D6 RID: 5078
	private float m_lodCount;

	// Token: 0x040013D7 RID: 5079
	private Matrix4x4[] m_instances = new Matrix4x4[1024];

	// Token: 0x040013D8 RID: 5080
	private int m_instanceCount;

	// Token: 0x040013D9 RID: 5081
	private bool m_firstFrame = true;
}
