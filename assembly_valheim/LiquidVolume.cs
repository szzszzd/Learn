using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Token: 0x02000261 RID: 609
public class LiquidVolume : MonoBehaviour
{
	// Token: 0x06001775 RID: 6005 RVA: 0x0009B24C File Offset: 0x0009944C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_meshFilter = base.GetComponent<MeshFilter>();
		base.transform.rotation = Quaternion.identity;
		int num = this.m_width + 1;
		int num2 = num * num;
		this.m_depths = new List<float>(num2);
		this.m_heights = new List<float>(num2);
		for (int i = 0; i < num2; i++)
		{
			this.m_depths.Add(0f);
			this.m_heights.Add(0f);
		}
		this.m_mesh = new Mesh();
		this.m_mesh.name = "___LiquidVolume m_mesh";
		if (this.HaveSavedData())
		{
			this.CheckLoad();
		}
		else
		{
			this.InitializeLevels();
		}
		this.m_builder = new Thread(new ThreadStart(this.UpdateThread));
		this.m_builder.Start();
	}

	// Token: 0x06001776 RID: 6006 RVA: 0x0009B323 File Offset: 0x00099523
	private void OnDestroy()
	{
		this.m_stopThread = true;
		this.m_builder.Join();
		this.m_timerLock.Close();
		this.m_meshDataLock.Close();
		UnityEngine.Object.Destroy(this.m_mesh);
	}

	// Token: 0x06001777 RID: 6007 RVA: 0x0009B358 File Offset: 0x00099558
	private void InitializeLevels()
	{
		int num = this.m_width / 2;
		int initialArea = this.m_initialArea;
		int num2 = this.m_width + 1;
		float value = this.m_initialVolume / (float)(initialArea * initialArea);
		for (int i = num - initialArea / 2; i <= num + initialArea / 2; i++)
		{
			for (int j = num - initialArea / 2; j <= num + initialArea / 2; j++)
			{
				this.m_depths[i * num2 + j] = value;
			}
		}
	}

	// Token: 0x06001778 RID: 6008 RVA: 0x0009B3CD File Offset: 0x000995CD
	private void CheckSave(float dt)
	{
		this.m_timeSinceSaving += dt;
		if (this.m_needsSaving && this.m_timeSinceSaving > this.m_saveInterval)
		{
			this.m_needsSaving = false;
			this.m_timeSinceSaving = 0f;
			this.Save();
		}
	}

	// Token: 0x06001779 RID: 6009 RVA: 0x0009B40C File Offset: 0x0009960C
	private void Save()
	{
		if (this.m_nview == null || !this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_meshDataLock.WaitOne();
		ZPackage zpackage = new ZPackage();
		zpackage.Write(2);
		float num = 0f;
		zpackage.Write(this.m_depths.Count);
		for (int i = 0; i < this.m_depths.Count; i++)
		{
			float num2 = this.m_depths[i];
			short data = (short)(num2 * 100f);
			zpackage.Write(data);
			num += num2;
		}
		zpackage.Write(num);
		byte[] array = Utils.Compress(zpackage.GetArray());
		this.m_nview.GetZDO().Set(ZDOVars.s_liquidData, array);
		this.m_lastDataRevision = this.m_nview.GetZDO().DataRevision;
		this.m_meshDataLock.ReleaseMutex();
		ZLog.Log("Saved liquid:" + array.Length.ToString() + " bytes  total:" + num.ToString());
	}

	// Token: 0x0600177A RID: 6010 RVA: 0x0009B51F File Offset: 0x0009971F
	private void CheckLoad()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.GetZDO().DataRevision != this.m_lastDataRevision)
		{
			this.Load();
		}
	}

	// Token: 0x0600177B RID: 6011 RVA: 0x0009B55B File Offset: 0x0009975B
	private bool HaveSavedData()
	{
		return !(this.m_nview == null) && this.m_nview.IsValid() && this.m_nview.GetZDO().GetByteArray(ZDOVars.s_liquidData, null) != null;
	}

	// Token: 0x0600177C RID: 6012 RVA: 0x0009B594 File Offset: 0x00099794
	private void Load()
	{
		this.m_lastDataRevision = this.m_nview.GetZDO().DataRevision;
		this.m_needsSaving = false;
		byte[] byteArray = this.m_nview.GetZDO().GetByteArray(ZDOVars.s_liquidData, null);
		if (byteArray == null)
		{
			return;
		}
		ZPackage zpackage = new ZPackage(Utils.Decompress(byteArray));
		int num = zpackage.ReadInt();
		int num2 = zpackage.ReadInt();
		this.m_meshDataLock.WaitOne();
		if (num2 != this.m_depths.Count)
		{
			ZLog.LogWarning("Depth array size missmatch");
			return;
		}
		float num3 = 0f;
		int num4 = 0;
		for (int i = 0; i < this.m_depths.Count; i++)
		{
			float num5 = (float)zpackage.ReadShort() / 100f;
			this.m_depths[i] = num5;
			num3 += num5;
			if (num5 > 0f)
			{
				num4++;
			}
		}
		if (num >= 2)
		{
			float num6 = zpackage.ReadSingle();
			if (num4 > 0)
			{
				float num7 = (num6 - num3) / (float)num4;
				for (int j = 0; j < this.m_depths.Count; j++)
				{
					float num8 = this.m_depths[j];
					if (num8 > 0f)
					{
						this.m_depths[j] = num8 + num7;
					}
				}
			}
		}
		this.m_meshDataLock.ReleaseMutex();
	}

	// Token: 0x0600177D RID: 6013 RVA: 0x0009B6D8 File Offset: 0x000998D8
	private void UpdateThread()
	{
		while (!this.m_stopThread)
		{
			this.m_timerLock.WaitOne();
			bool flag = false;
			if (this.m_timeToSimulate >= 0.05f && this.m_haveHeights)
			{
				this.m_timeToSimulate = 0f;
				flag = true;
			}
			this.m_timerLock.ReleaseMutex();
			if (flag)
			{
				this.m_meshDataLock.WaitOne();
				this.UpdateLiquid(0.05f);
				if (this.m_dirty)
				{
					this.m_dirty = false;
					this.PrebuildMesh();
				}
				this.m_meshDataLock.ReleaseMutex();
			}
			Thread.Sleep(1);
		}
	}

	// Token: 0x0600177E RID: 6014 RVA: 0x0009B770 File Offset: 0x00099970
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if (this.m_nview != null)
		{
			if (!this.m_nview.IsValid())
			{
				return;
			}
			this.CheckLoad();
			if (this.m_nview.IsOwner())
			{
				this.CheckSave(deltaTime);
			}
		}
		this.m_updateDelayTimer += deltaTime;
		if (this.m_updateDelayTimer > 1f)
		{
			this.m_timerLock.WaitOne();
			this.m_timeToSimulate += deltaTime;
			this.m_timerLock.ReleaseMutex();
		}
		this.updateHeightTimer -= deltaTime;
		if (this.updateHeightTimer <= 0f && this.m_meshDataLock.WaitOne(0))
		{
			this.UpdateHeights();
			this.m_haveHeights = true;
			this.m_meshDataLock.ReleaseMutex();
			this.updateHeightTimer = 1f;
		}
		if (this.m_dirtyMesh && this.m_meshDataLock.WaitOne(0))
		{
			this.m_dirtyMesh = false;
			this.PostBuildMesh();
			this.m_meshDataLock.ReleaseMutex();
		}
		this.UpdateEffects(deltaTime);
	}

	// Token: 0x0600177F RID: 6015 RVA: 0x0009B87C File Offset: 0x00099A7C
	private void UpdateLiquid(float dt)
	{
		float num = 0f;
		for (int i = 0; i < this.m_depths.Count; i++)
		{
			num += this.m_depths[i];
		}
		int num2 = this.m_width + 1;
		float maxD = dt * this.m_viscocity;
		for (int j = 0; j < num2 - 1; j++)
		{
			for (int k = 0; k < num2 - 1; k++)
			{
				int index = j * num2 + k;
				int index2 = j * num2 + k + 1;
				this.EvenDepth(index, index2, maxD);
			}
		}
		for (int l = 0; l < num2 - 1; l++)
		{
			for (int m = 0; m < num2 - 1; m++)
			{
				int index3 = m * num2 + l;
				int index4 = (m + 1) * num2 + l;
				this.EvenDepth(index3, index4, maxD);
			}
		}
		float num3 = 0f;
		int num4 = 0;
		for (int n = 0; n < this.m_depths.Count; n++)
		{
			float num5 = this.m_depths[n];
			num3 += num5;
			if (num5 > 0f)
			{
				num4++;
			}
		}
		float num6 = num - num3;
		if (num6 != 0f && num4 > 0)
		{
			float num7 = num6 / (float)num4;
			for (int num8 = 0; num8 < this.m_depths.Count; num8++)
			{
				float num9 = this.m_depths[num8];
				if (num9 > 0f)
				{
					this.m_depths[num8] = num9 + num7;
				}
			}
		}
		for (int num10 = 0; num10 < num2; num10++)
		{
			this.m_depths[num10] = 0f;
			this.m_depths[this.m_width * num2 + num10] = 0f;
			this.m_depths[num10 * num2] = 0f;
			this.m_depths[num10 * num2 + this.m_width] = 0f;
		}
	}

	// Token: 0x06001780 RID: 6016 RVA: 0x0009BA64 File Offset: 0x00099C64
	private void EvenDepth(int index0, int index1, float maxD)
	{
		float num = this.m_depths[index0];
		float num2 = this.m_depths[index1];
		if (num == 0f && num2 == 0f)
		{
			return;
		}
		float num3 = this.m_heights[index0];
		float num4 = this.m_heights[index1];
		float num5 = num3 + num;
		float num6 = num4 + num2;
		if (Mathf.Abs(num6 - num5) < 0.001f)
		{
			return;
		}
		if (num5 > num6)
		{
			if (num <= 0f)
			{
				return;
			}
			float num7 = num5 - num6;
			float num8 = num7 * this.m_viscocity;
			num8 = Mathf.Pow(num8, 0.5f);
			num8 = Mathf.Min(num8, num7 * 0.5f);
			num8 = Mathf.Min(num8, num);
			num -= num8;
			num2 += num8;
		}
		else
		{
			if (num2 <= 0f)
			{
				return;
			}
			float num9 = num6 - num5;
			float num10 = num9 * this.m_viscocity;
			num10 = Mathf.Pow(num10, 0.5f);
			num10 = Mathf.Min(num10, num9 * 0.5f);
			num10 = Mathf.Min(num10, num2);
			num2 -= num10;
			num += num10;
		}
		this.m_depths[index0] = Mathf.Max(0f, num);
		this.m_depths[index1] = Mathf.Max(0f, num2);
		this.m_dirty = true;
		this.m_needsSaving = true;
	}

	// Token: 0x06001781 RID: 6017 RVA: 0x0009BBAC File Offset: 0x00099DAC
	private void UpdateHeights()
	{
		int value = this.m_groundLayer.value;
		int num = this.m_width + 1;
		float num2 = -this.m_maxDepth;
		float y = base.transform.position.y;
		float maxDistance = this.m_maxDepth * 2f;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 origin = this.CalcMaxVertex(j, i);
				float value2 = num2;
				RaycastHit raycastHit;
				if (Physics.Raycast(origin, Vector3.down, out raycastHit, maxDistance, value))
				{
					value2 = raycastHit.point.y - y;
				}
				float value3 = Mathf.Clamp(value2, -this.m_maxDepth, this.m_maxDepth);
				this.m_heights[i * num + j] = value3;
			}
		}
	}

	// Token: 0x06001782 RID: 6018 RVA: 0x0009BC70 File Offset: 0x00099E70
	private void PrebuildMesh()
	{
		int num = this.m_width + 1;
		this.m_tempVertises.Clear();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 item = this.CalcVertex(j, i, false);
				this.m_tempVertises.Add(item);
			}
		}
		this.m_tempNormals.Clear();
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				if (l == num - 1 || k == num - 1)
				{
					this.m_tempNormals.Add(Vector3.up);
				}
				else
				{
					Vector3 b = this.m_tempVertises[k * num + l];
					Vector3 a = this.m_tempVertises[k * num + l + 1];
					Vector3 normalized = Vector3.Cross(this.m_tempVertises[(k + 1) * num + l] - b, a - b).normalized;
					this.m_tempNormals.Add(normalized);
				}
			}
		}
		this.m_tempColors.Clear();
		Color c = new Color(1f, 1f, 1f, 0f);
		Color c2 = new Color(1f, 1f, 1f, 1f);
		for (int m = 0; m < this.m_depths.Count; m++)
		{
			if (this.m_depths[m] < 0.001f)
			{
				this.m_tempColors.Add(c);
			}
			else
			{
				this.m_tempColors.Add(c2);
			}
		}
		if (this.m_tempIndices.Count == 0)
		{
			this.m_tempUVs.Clear();
			for (int n = 0; n < num; n++)
			{
				for (int num2 = 0; num2 < num; num2++)
				{
					this.m_tempUVs.Add(new Vector2((float)num2 / (float)this.m_width, (float)n / (float)this.m_width));
				}
			}
			this.m_tempIndices.Clear();
			for (int num3 = 0; num3 < num - 1; num3++)
			{
				for (int num4 = 0; num4 < num - 1; num4++)
				{
					int item2 = num3 * num + num4;
					int item3 = num3 * num + num4 + 1;
					int item4 = (num3 + 1) * num + num4 + 1;
					int item5 = (num3 + 1) * num + num4;
					this.m_tempIndices.Add(item2);
					this.m_tempIndices.Add(item5);
					this.m_tempIndices.Add(item3);
					this.m_tempIndices.Add(item3);
					this.m_tempIndices.Add(item5);
					this.m_tempIndices.Add(item4);
				}
			}
		}
		this.m_tempColliderVertises.Clear();
		int num5 = this.m_width / 2;
		int num6 = num5 + 1;
		for (int num7 = 0; num7 < num6; num7++)
		{
			for (int num8 = 0; num8 < num6; num8++)
			{
				Vector3 item6 = this.CalcVertex(num8 * 2, num7 * 2, true);
				this.m_tempColliderVertises.Add(item6);
			}
		}
		if (this.m_tempColliderIndices.Count == 0)
		{
			this.m_tempColliderIndices.Clear();
			for (int num9 = 0; num9 < num5; num9++)
			{
				for (int num10 = 0; num10 < num5; num10++)
				{
					int item7 = num9 * num6 + num10;
					int item8 = num9 * num6 + num10 + 1;
					int item9 = (num9 + 1) * num6 + num10 + 1;
					int item10 = (num9 + 1) * num6 + num10;
					this.m_tempColliderIndices.Add(item7);
					this.m_tempColliderIndices.Add(item10);
					this.m_tempColliderIndices.Add(item8);
					this.m_tempColliderIndices.Add(item8);
					this.m_tempColliderIndices.Add(item10);
					this.m_tempColliderIndices.Add(item9);
				}
			}
		}
		this.m_dirtyMesh = true;
	}

	// Token: 0x06001783 RID: 6019 RVA: 0x0009C05C File Offset: 0x0009A25C
	private void SmoothNormals(List<Vector3> normals, float yScale)
	{
		int num = this.m_width + 1;
		for (int i = 1; i < num - 1; i++)
		{
			for (int j = 1; j < num - 1; j++)
			{
				Vector3 vector = normals[i * num + j];
				Vector3 b = normals[(i - 1) * num + j];
				Vector3 b2 = normals[(i + 1) * num + j];
				b.y *= yScale;
				b2.y *= yScale;
				vector = (vector + b + b2).normalized;
				normals[i * num + j] = vector;
			}
		}
		for (int k = 1; k < num - 1; k++)
		{
			for (int l = 1; l < num - 1; l++)
			{
				Vector3 vector2 = normals[k * num + l];
				Vector3 b3 = normals[k * num + l - 1];
				Vector3 b4 = normals[k * num + l + 1];
				b3.y *= yScale;
				b4.y *= yScale;
				vector2 = (vector2 + b3 + b4).normalized;
				normals[k * num + l] = vector2;
			}
		}
	}

	// Token: 0x06001784 RID: 6020 RVA: 0x0009C194 File Offset: 0x0009A394
	private void PostBuildMesh()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		this.m_mesh.SetVertices(this.m_tempVertises);
		this.m_mesh.SetNormals(this.m_tempNormals);
		this.m_mesh.SetColors(this.m_tempColors);
		if (this.m_mesh.GetIndexCount(0) == 0U)
		{
			this.m_mesh.SetUVs(0, this.m_tempUVs);
			this.m_mesh.SetIndices(this.m_tempIndices.ToArray(), MeshTopology.Triangles, 0);
		}
		this.m_mesh.RecalculateBounds();
		if (this.m_meshFilter)
		{
			this.m_meshFilter.sharedMesh = this.m_mesh;
		}
	}

	// Token: 0x06001785 RID: 6021 RVA: 0x0009C23C File Offset: 0x0009A43C
	private void RebuildMesh()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		int num = this.m_width + 1;
		float num2 = -999999f;
		float num3 = 999999f;
		this.m_tempVertises.Clear();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 vector = this.CalcVertex(j, i, false);
				this.m_tempVertises.Add(vector);
				if (vector.y > num2)
				{
					num2 = vector.y;
				}
				if (vector.y < num3)
				{
					num3 = vector.y;
				}
			}
		}
		this.m_mesh.SetVertices(this.m_tempVertises);
		this.m_tempColors.Clear();
		Color c = new Color(1f, 1f, 1f, 0f);
		Color c2 = new Color(1f, 1f, 1f, 1f);
		for (int k = 0; k < this.m_depths.Count; k++)
		{
			if (this.m_depths[k] < 0.001f)
			{
				this.m_tempColors.Add(c);
			}
			else
			{
				this.m_tempColors.Add(c2);
			}
		}
		this.m_mesh.SetColors(this.m_tempColors);
		int num4 = (num - 1) * (num - 1) * 6;
		if ((ulong)this.m_mesh.GetIndexCount(0) != (ulong)((long)num4))
		{
			this.m_tempUVs.Clear();
			for (int l = 0; l < num; l++)
			{
				for (int m = 0; m < num; m++)
				{
					this.m_tempUVs.Add(new Vector2((float)m / (float)this.m_width, (float)l / (float)this.m_width));
				}
			}
			this.m_mesh.SetUVs(0, this.m_tempUVs);
			this.m_tempIndices.Clear();
			for (int n = 0; n < num - 1; n++)
			{
				for (int num5 = 0; num5 < num - 1; num5++)
				{
					int item = n * num + num5;
					int item2 = n * num + num5 + 1;
					int item3 = (n + 1) * num + num5 + 1;
					int item4 = (n + 1) * num + num5;
					this.m_tempIndices.Add(item);
					this.m_tempIndices.Add(item4);
					this.m_tempIndices.Add(item2);
					this.m_tempIndices.Add(item2);
					this.m_tempIndices.Add(item4);
					this.m_tempIndices.Add(item3);
				}
			}
			this.m_mesh.SetIndices(this.m_tempIndices.ToArray(), MeshTopology.Triangles, 0);
		}
		ZLog.Log("Update mesh1 " + ((Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f).ToString());
		realtimeSinceStartup = Time.realtimeSinceStartup;
		this.m_mesh.RecalculateNormals();
		ZLog.Log("Update mesh2 " + ((Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f).ToString());
		realtimeSinceStartup = Time.realtimeSinceStartup;
		this.m_mesh.RecalculateTangents();
		ZLog.Log("Update mesh3 " + ((Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f).ToString());
		realtimeSinceStartup = Time.realtimeSinceStartup;
		this.m_mesh.RecalculateBounds();
		ZLog.Log("Update mesh4 " + ((Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f).ToString());
		realtimeSinceStartup = Time.realtimeSinceStartup;
		if (this.m_collider)
		{
			this.m_collider.sharedMesh = this.m_mesh;
		}
		ZLog.Log("Update mesh5 " + ((Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f).ToString());
		realtimeSinceStartup = Time.realtimeSinceStartup;
		if (this.m_meshFilter)
		{
			this.m_meshFilter.sharedMesh = this.m_mesh;
		}
		ZLog.Log("Update mesh6 " + ((Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f).ToString());
		realtimeSinceStartup = Time.realtimeSinceStartup;
	}

	// Token: 0x06001786 RID: 6022 RVA: 0x0009C634 File Offset: 0x0009A834
	private Vector3 CalcMaxVertex(int x, int y)
	{
		int num = this.m_width + 1;
		Vector3 b = new Vector3((float)this.m_width * this.m_scale * -0.5f, this.m_maxDepth, (float)this.m_width * this.m_scale * -0.5f);
		return base.transform.position + b + new Vector3((float)x * this.m_scale, 0f, (float)y * this.m_scale);
	}

	// Token: 0x06001787 RID: 6023 RVA: 0x0009C6B4 File Offset: 0x0009A8B4
	private void ClampHeight(int x, int y, ref float height)
	{
		if (x < 0 || y < 0 || x >= this.m_width + 1 || y >= this.m_width + 1)
		{
			return;
		}
		int num = this.m_width + 1;
		int index = y * num + x;
		float num2 = this.m_depths[index];
		if ((double)num2 <= 0.0)
		{
			return;
		}
		float num3 = this.m_heights[index];
		height = num3 + num2;
		height -= 0.1f;
	}

	// Token: 0x06001788 RID: 6024 RVA: 0x0009C728 File Offset: 0x0009A928
	private bool HasTarNeighbour(int cx, int cy)
	{
		int num = this.m_width + 1;
		for (int i = cy - 2; i <= cy + 2; i++)
		{
			for (int j = cx - 2; j <= cx + 2; j++)
			{
				if (j >= 0 && i >= 0 && j < num && i < num && this.m_depths[i * num + j] > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06001789 RID: 6025 RVA: 0x0009C788 File Offset: 0x0009A988
	private void ClampToNeighbourSurface(int x, int y, ref float d)
	{
		this.ClampHeight(x - 1, y - 1, ref d);
		this.ClampHeight(x, y - 1, ref d);
		this.ClampHeight(x + 1, y - 1, ref d);
		this.ClampHeight(x - 1, y + 1, ref d);
		this.ClampHeight(x, y + 1, ref d);
		this.ClampHeight(x + 1, y + 1, ref d);
		this.ClampHeight(x - 1, y, ref d);
		this.ClampHeight(x + 1, y, ref d);
	}

	// Token: 0x0600178A RID: 6026 RVA: 0x0009C7F8 File Offset: 0x0009A9F8
	private Vector3 CalcVertex(int x, int y, bool collider)
	{
		int num = this.m_width + 1;
		int index = y * num + x;
		float num2 = this.m_heights[index];
		float num3 = this.m_depths[index];
		if (!collider)
		{
			if (num3 > 0f)
			{
				num3 = Mathf.Max(0.1f, num3);
				num2 += num3;
			}
		}
		else
		{
			if (num3 < 0.001f)
			{
				num2 -= 1f;
			}
			else
			{
				num2 += num3;
			}
			num2 += this.m_physicsOffset;
		}
		return new Vector3((float)this.m_width * this.m_scale * -0.5f, 0f, (float)this.m_width * this.m_scale * -0.5f) + new Vector3((float)x * this.m_scale, num2, (float)y * this.m_scale);
	}

	// Token: 0x0600178B RID: 6027 RVA: 0x0009C8BC File Offset: 0x0009AABC
	public float GetSurface(Vector3 p)
	{
		Vector2 vector = this.WorldToLocal(p);
		float num = this.GetDepth(vector.x, vector.y);
		float height = this.GetHeight(vector.x, vector.y);
		if ((double)num <= 0.001)
		{
			num -= 0.5f;
		}
		else
		{
			num += Mathf.Sin(p.x * this.m_noiseFrequency + Time.time * this.m_noiseSpeed) * Mathf.Sin(p.z * this.m_noiseFrequency + Time.time * 0.78521f * this.m_noiseSpeed) * this.m_noiseHeight;
		}
		return base.transform.position.y + height + num;
	}

	// Token: 0x0600178C RID: 6028 RVA: 0x0009C974 File Offset: 0x0009AB74
	private float GetDepth(float x, float y)
	{
		x = Mathf.Clamp(x, 0f, (float)this.m_width);
		x = Mathf.Clamp(x, 0f, (float)this.m_width);
		int num = (int)x;
		int num2 = (int)y;
		float t = x - (float)num;
		float t2 = y - (float)num2;
		float a = Mathf.Lerp(this.GetDepth(num, num2), this.GetDepth(num + 1, num2), t);
		float b = Mathf.Lerp(this.GetDepth(num, num2 + 1), this.GetDepth(num + 1, num2 + 1), t);
		return Mathf.Lerp(a, b, t2);
	}

	// Token: 0x0600178D RID: 6029 RVA: 0x0009C9F8 File Offset: 0x0009ABF8
	private float GetHeight(float x, float y)
	{
		x = Mathf.Clamp(x, 0f, (float)this.m_width);
		x = Mathf.Clamp(x, 0f, (float)this.m_width);
		int num = (int)x;
		int num2 = (int)y;
		float t = x - (float)num;
		float t2 = y - (float)num2;
		float a = Mathf.Lerp(this.GetHeight(num, num2), this.GetHeight(num + 1, num2), t);
		float b = Mathf.Lerp(this.GetHeight(num, num2 + 1), this.GetHeight(num + 1, num2 + 1), t);
		return Mathf.Lerp(a, b, t2);
	}

	// Token: 0x0600178E RID: 6030 RVA: 0x0009CA7C File Offset: 0x0009AC7C
	private float GetDepth(int x, int y)
	{
		int num = this.m_width + 1;
		x = Mathf.Clamp(x, 0, this.m_width);
		y = Mathf.Clamp(y, 0, this.m_width);
		return this.m_depths[y * num + x];
	}

	// Token: 0x0600178F RID: 6031 RVA: 0x0009CAC0 File Offset: 0x0009ACC0
	private float GetHeight(int x, int y)
	{
		int num = this.m_width + 1;
		x = Mathf.Clamp(x, 0, this.m_width);
		y = Mathf.Clamp(y, 0, this.m_width);
		return this.m_heights[y * num + x];
	}

	// Token: 0x06001790 RID: 6032 RVA: 0x0009CB04 File Offset: 0x0009AD04
	private Vector2 WorldToLocal(Vector3 v)
	{
		Vector3 position = base.transform.position;
		float num = (float)this.m_width * this.m_scale * -0.5f;
		Vector2 result = new Vector2(v.x, v.z);
		result.x -= position.x + num;
		result.y -= position.z + num;
		result.x /= this.m_scale;
		result.y /= this.m_scale;
		return result;
	}

	// Token: 0x06001791 RID: 6033 RVA: 0x0009CB90 File Offset: 0x0009AD90
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(base.transform.position, new Vector3((float)this.m_width * this.m_scale, this.m_maxDepth * 2f, (float)this.m_width * this.m_scale));
	}

	// Token: 0x06001792 RID: 6034 RVA: 0x0009CBE4 File Offset: 0x0009ADE4
	private void UpdateEffects(float dt)
	{
		this.m_randomEffectTimer += dt;
		if (this.m_randomEffectTimer < this.m_randomEffectInterval)
		{
			return;
		}
		this.m_randomEffectTimer = 0f;
		Vector2Int vector2Int = new Vector2Int(UnityEngine.Random.Range(0, this.m_width), UnityEngine.Random.Range(0, this.m_width));
		if (this.GetDepth(vector2Int.x, vector2Int.y) < 0.2f)
		{
			return;
		}
		Vector3 basePos = this.CalcVertex(vector2Int.x, vector2Int.y, false) + base.transform.position;
		this.m_randomEffectList.Create(basePos, Quaternion.identity, null, 1f, -1);
	}

	// Token: 0x040018D5 RID: 6357
	private const int liquidSaveVersion = 2;

	// Token: 0x040018D6 RID: 6358
	private float updateHeightTimer = -1000f;

	// Token: 0x040018D7 RID: 6359
	private List<Vector3> m_tempVertises = new List<Vector3>();

	// Token: 0x040018D8 RID: 6360
	private List<Vector3> m_tempNormals = new List<Vector3>();

	// Token: 0x040018D9 RID: 6361
	private List<Vector2> m_tempUVs = new List<Vector2>();

	// Token: 0x040018DA RID: 6362
	private List<int> m_tempIndices = new List<int>();

	// Token: 0x040018DB RID: 6363
	private List<Color32> m_tempColors = new List<Color32>();

	// Token: 0x040018DC RID: 6364
	private List<Vector3> m_tempColliderVertises = new List<Vector3>();

	// Token: 0x040018DD RID: 6365
	private List<int> m_tempColliderIndices = new List<int>();

	// Token: 0x040018DE RID: 6366
	public int m_width = 32;

	// Token: 0x040018DF RID: 6367
	public float m_scale = 1f;

	// Token: 0x040018E0 RID: 6368
	public float m_maxDepth = 10f;

	// Token: 0x040018E1 RID: 6369
	public LiquidType m_liquidType = LiquidType.Tar;

	// Token: 0x040018E2 RID: 6370
	public float m_physicsOffset = -2f;

	// Token: 0x040018E3 RID: 6371
	public float m_initialVolume = 1000f;

	// Token: 0x040018E4 RID: 6372
	public int m_initialArea = 8;

	// Token: 0x040018E5 RID: 6373
	public float m_viscocity = 1f;

	// Token: 0x040018E6 RID: 6374
	public float m_noiseHeight = 0.1f;

	// Token: 0x040018E7 RID: 6375
	public float m_noiseFrequency = 1f;

	// Token: 0x040018E8 RID: 6376
	public float m_noiseSpeed = 1f;

	// Token: 0x040018E9 RID: 6377
	public bool m_castShadow = true;

	// Token: 0x040018EA RID: 6378
	public LayerMask m_groundLayer;

	// Token: 0x040018EB RID: 6379
	public MeshCollider m_collider;

	// Token: 0x040018EC RID: 6380
	public float m_saveInterval = 4f;

	// Token: 0x040018ED RID: 6381
	public float m_randomEffectInterval = 3f;

	// Token: 0x040018EE RID: 6382
	public EffectList m_randomEffectList = new EffectList();

	// Token: 0x040018EF RID: 6383
	private List<float> m_heights;

	// Token: 0x040018F0 RID: 6384
	private List<float> m_depths;

	// Token: 0x040018F1 RID: 6385
	private float m_randomEffectTimer;

	// Token: 0x040018F2 RID: 6386
	private bool m_haveHeights;

	// Token: 0x040018F3 RID: 6387
	private bool m_needsSaving;

	// Token: 0x040018F4 RID: 6388
	private float m_timeSinceSaving;

	// Token: 0x040018F5 RID: 6389
	private bool m_dirty = true;

	// Token: 0x040018F6 RID: 6390
	private bool m_dirtyMesh;

	// Token: 0x040018F7 RID: 6391
	private Mesh m_mesh;

	// Token: 0x040018F8 RID: 6392
	private MeshFilter m_meshFilter;

	// Token: 0x040018F9 RID: 6393
	private Thread m_builder;

	// Token: 0x040018FA RID: 6394
	private Mutex m_meshDataLock = new Mutex();

	// Token: 0x040018FB RID: 6395
	private bool m_stopThread;

	// Token: 0x040018FC RID: 6396
	private Mutex m_timerLock = new Mutex();

	// Token: 0x040018FD RID: 6397
	private float m_timeToSimulate;

	// Token: 0x040018FE RID: 6398
	private float m_updateDelayTimer;

	// Token: 0x040018FF RID: 6399
	private ZNetView m_nview;

	// Token: 0x04001900 RID: 6400
	private uint m_lastDataRevision = uint.MaxValue;
}
