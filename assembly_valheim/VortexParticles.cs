using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

// Token: 0x02000090 RID: 144
[ExecuteAlways]
public class VortexParticles : MonoBehaviour
{
	// Token: 0x0600063F RID: 1599 RVA: 0x0002F950 File Offset: 0x0002DB50
	private void Start()
	{
		this.ps = base.GetComponent<ParticleSystem>();
		if (this.ps == null)
		{
			ZLog.LogWarning("VortexParticles object '" + base.gameObject.name + "' is missing a particle system and disabled!");
			this.effectOn = false;
		}
	}

	// Token: 0x06000640 RID: 1600 RVA: 0x0002F9A0 File Offset: 0x0002DBA0
	private void Update()
	{
		if (this.ps.main.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			this.job.vortexCenter = this.centerOffset;
			this.job.upDir = new Vector3(0f, 1f, 0f);
		}
		else
		{
			this.job.vortexCenter = base.transform.position + this.centerOffset;
			this.job.upDir = base.transform.up;
		}
		this.job.pullStrength = this.pullStrength;
		this.job.vortexStrength = this.vortexStrength;
		this.job.lineAttraction = this.lineAttraction;
		this.job.useCustomData = this.useCustomData;
		this.job.deltaTime = Time.deltaTime;
	}

	// Token: 0x06000641 RID: 1601 RVA: 0x0002FA80 File Offset: 0x0002DC80
	private void OnParticleUpdateJobScheduled()
	{
		if (this.ps == null)
		{
			this.ps = base.GetComponent<ParticleSystem>();
			if (this.ps == null)
			{
				ZLog.LogWarning("VortexParticles object '" + base.gameObject.name + "' is missing a particle system and disabled!");
				this.effectOn = false;
			}
		}
		if (this.effectOn)
		{
			this.job.Schedule(this.ps, 1024, default(JobHandle));
		}
	}

	// Token: 0x04000797 RID: 1943
	private ParticleSystem ps;

	// Token: 0x04000798 RID: 1944
	private VortexParticles.VortexParticlesJob job;

	// Token: 0x04000799 RID: 1945
	[SerializeField]
	private bool effectOn = true;

	// Token: 0x0400079A RID: 1946
	[SerializeField]
	private Vector3 centerOffset;

	// Token: 0x0400079B RID: 1947
	[SerializeField]
	private float pullStrength;

	// Token: 0x0400079C RID: 1948
	[SerializeField]
	private float vortexStrength;

	// Token: 0x0400079D RID: 1949
	[SerializeField]
	private bool lineAttraction;

	// Token: 0x0400079E RID: 1950
	[SerializeField]
	private bool useCustomData;

	// Token: 0x02000091 RID: 145
	private struct VortexParticlesJob : IJobParticleSystemParallelFor
	{
		// Token: 0x06000643 RID: 1603 RVA: 0x0002FB14 File Offset: 0x0002DD14
		public void Execute(ParticleSystemJobData particles, int i)
		{
			ParticleSystemNativeArray3 particleSystemNativeArray = particles.velocities;
			float x = particleSystemNativeArray.x[i];
			particleSystemNativeArray = particles.velocities;
			float y = particleSystemNativeArray.y[i];
			particleSystemNativeArray = particles.velocities;
			Vector3 a = new Vector3(x, y, particleSystemNativeArray.z[i]);
			particleSystemNativeArray = particles.positions;
			float x2 = particleSystemNativeArray.x[i];
			particleSystemNativeArray = particles.positions;
			float y2 = particleSystemNativeArray.y[i];
			particleSystemNativeArray = particles.positions;
			Vector3 vector = new Vector3(x2, y2, particleSystemNativeArray.z[i]);
			Vector3 a2 = this.vortexCenter;
			float d = this.useCustomData ? particles.customData1.x[i] : this.vortexStrength;
			if (this.lineAttraction)
			{
				a2.y = vector.y;
			}
			Vector3 vector2 = a2 - vector;
			Vector3 a3 = Vector3.Cross(Vector3.Normalize(vector2), this.upDir);
			Vector3 vector3 = a + vector2 * this.pullStrength * this.deltaTime;
			vector3 += a3 * d * this.deltaTime;
			NativeArray<float> x3 = particles.velocities.x;
			NativeArray<float> y3 = particles.velocities.y;
			NativeArray<float> z = particles.velocities.z;
			x3[i] = vector3.x;
			y3[i] = vector3.y;
			z[i] = vector3.z;
		}

		// Token: 0x0400079F RID: 1951
		[ReadOnly]
		public Vector3 vortexCenter;

		// Token: 0x040007A0 RID: 1952
		[ReadOnly]
		public float pullStrength;

		// Token: 0x040007A1 RID: 1953
		[ReadOnly]
		public Vector3 upDir;

		// Token: 0x040007A2 RID: 1954
		[ReadOnly]
		public float vortexStrength;

		// Token: 0x040007A3 RID: 1955
		[ReadOnly]
		public bool lineAttraction;

		// Token: 0x040007A4 RID: 1956
		[ReadOnly]
		public bool useCustomData;

		// Token: 0x040007A5 RID: 1957
		[ReadOnly]
		public float deltaTime;
	}
}
