using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000011 RID: 17
public class FootStep : MonoBehaviour
{
	// Token: 0x06000140 RID: 320 RVA: 0x00008BAC File Offset: 0x00006DAC
	private void Start()
	{
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_character = base.GetComponent<Character>();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_footstep = this.m_animator.GetFloat(FootStep.s_footstepID);
		if (this.m_pieceLayer == 0)
		{
			this.m_pieceLayer = LayerMask.NameToLayer("piece");
		}
		Character character = this.m_character;
		character.m_onLand = (Action<Vector3>)Delegate.Combine(character.m_onLand, new Action<Vector3>(this.OnLand));
		if (this.m_nview.IsValid())
		{
			this.m_nview.Register<int, Vector3>("Step", new Action<long, int, Vector3>(this.RPC_Step));
		}
	}

	// Token: 0x06000141 RID: 321 RVA: 0x00008C5B File Offset: 0x00006E5B
	private void OnEnable()
	{
		FootStep.Instances.Add(this);
	}

	// Token: 0x06000142 RID: 322 RVA: 0x00008C68 File Offset: 0x00006E68
	private void OnDisable()
	{
		FootStep.Instances.Remove(this);
	}

	// Token: 0x06000143 RID: 323 RVA: 0x00008C76 File Offset: 0x00006E76
	public void CustomUpdate(float dt)
	{
		if (this.m_nview == null || !this.m_nview.IsOwner())
		{
			return;
		}
		this.UpdateFootstep(dt);
	}

	// Token: 0x06000144 RID: 324 RVA: 0x00008C9C File Offset: 0x00006E9C
	private void UpdateFootstep(float dt)
	{
		if (this.m_feet.Length == 0)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, mainCamera.transform.position) > this.m_footstepCullDistance)
		{
			return;
		}
		this.UpdateFootstepCurveTrigger(dt);
	}

	// Token: 0x06000145 RID: 325 RVA: 0x00008CF0 File Offset: 0x00006EF0
	private void UpdateFootstepCurveTrigger(float dt)
	{
		this.m_footstepTimer += dt;
		float @float = this.m_animator.GetFloat(FootStep.s_footstepID);
		if (Utils.SignDiffers(@float, this.m_footstep) && Mathf.Max(Mathf.Abs(this.m_animator.GetFloat(FootStep.s_forwardSpeedID)), Mathf.Abs(this.m_animator.GetFloat(FootStep.s_sidewaySpeedID))) > 0.2f && this.m_footstepTimer > 0.2f)
		{
			this.m_footstepTimer = 0f;
			this.OnFoot();
		}
		this.m_footstep = @float;
	}

	// Token: 0x06000146 RID: 326 RVA: 0x00008D88 File Offset: 0x00006F88
	private Transform FindActiveFoot()
	{
		Transform transform = null;
		float num = 9999f;
		Vector3 forward = base.transform.forward;
		foreach (Transform transform2 in this.m_feet)
		{
			if (!(transform2 == null))
			{
				Vector3 rhs = transform2.position - base.transform.position;
				float num2 = Vector3.Dot(forward, rhs);
				if (num2 > num || transform == null)
				{
					transform = transform2;
					num = num2;
				}
			}
		}
		return transform;
	}

	// Token: 0x06000147 RID: 327 RVA: 0x00008E0C File Offset: 0x0000700C
	private Transform FindFoot(string name)
	{
		foreach (Transform transform in this.m_feet)
		{
			if (transform.gameObject.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	// Token: 0x06000148 RID: 328 RVA: 0x00008E48 File Offset: 0x00007048
	public void OnFoot()
	{
		Transform foot = this.FindActiveFoot();
		this.OnFoot(foot);
	}

	// Token: 0x06000149 RID: 329 RVA: 0x00008E64 File Offset: 0x00007064
	public void OnFoot(string name)
	{
		Transform transform = this.FindFoot(name);
		if (transform == null)
		{
			ZLog.LogWarning("FAiled to find foot:" + name);
			return;
		}
		this.OnFoot(transform);
	}

	// Token: 0x0600014A RID: 330 RVA: 0x00008E9C File Offset: 0x0000709C
	private void OnLand(Vector3 point)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		FootStep.GroundMaterial groundMaterial = this.GetGroundMaterial(this.m_character, point);
		int num = this.FindBestStepEffect(groundMaterial, FootStep.MotionType.Land);
		if (num != -1)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "Step", new object[]
			{
				num,
				point
			});
		}
	}

	// Token: 0x0600014B RID: 331 RVA: 0x00008F00 File Offset: 0x00007100
	private void OnFoot(Transform foot)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		Vector3 vector = (foot != null) ? foot.position : base.transform.position;
		FootStep.MotionType motionType = FootStep.GetMotionType(this.m_character);
		FootStep.GroundMaterial groundMaterial = this.GetGroundMaterial(this.m_character, vector);
		int num = this.FindBestStepEffect(groundMaterial, motionType);
		if (num != -1)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "Step", new object[]
			{
				num,
				vector
			});
		}
	}

	// Token: 0x0600014C RID: 332 RVA: 0x00008F8C File Offset: 0x0000718C
	private static void PurgeOldEffects()
	{
		while (FootStep.s_stepInstances.Count > 30)
		{
			GameObject gameObject = FootStep.s_stepInstances.Dequeue();
			if (gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	// Token: 0x0600014D RID: 333 RVA: 0x00008FC4 File Offset: 0x000071C4
	private void DoEffect(FootStep.StepEffect effect, Vector3 point)
	{
		foreach (GameObject gameObject in effect.m_effectPrefabs)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, point, base.transform.rotation);
			FootStep.s_stepInstances.Enqueue(gameObject2);
			if (gameObject2.GetComponent<ZNetView>() != null)
			{
				ZLog.LogWarning(string.Concat(new string[]
				{
					"Foot step effect ",
					effect.m_name,
					" prefab ",
					gameObject.name,
					" in ",
					this.m_character.gameObject.name,
					" should not contain a ZNetView component"
				}));
			}
		}
		FootStep.PurgeOldEffects();
	}

	// Token: 0x0600014E RID: 334 RVA: 0x00009078 File Offset: 0x00007278
	private void RPC_Step(long sender, int effectIndex, Vector3 point)
	{
		FootStep.StepEffect effect = this.m_effects[effectIndex];
		this.DoEffect(effect, point);
	}

	// Token: 0x0600014F RID: 335 RVA: 0x0000909A File Offset: 0x0000729A
	private static FootStep.MotionType GetMotionType(Character character)
	{
		if (character.IsWalking())
		{
			return FootStep.MotionType.Walk;
		}
		if (character.IsSwimming())
		{
			return FootStep.MotionType.Swimming;
		}
		if (character.IsWallRunning())
		{
			return FootStep.MotionType.Climbing;
		}
		if (character.IsRunning())
		{
			return FootStep.MotionType.Run;
		}
		if (character.IsSneaking())
		{
			return FootStep.MotionType.Sneak;
		}
		return FootStep.MotionType.Jog;
	}

	// Token: 0x06000150 RID: 336 RVA: 0x000090D4 File Offset: 0x000072D4
	private FootStep.GroundMaterial GetGroundMaterial(Character character, Vector3 point)
	{
		if (character.InWater())
		{
			return FootStep.GroundMaterial.Water;
		}
		if (character.InLiquid())
		{
			return FootStep.GroundMaterial.Tar;
		}
		if (!character.IsOnGround())
		{
			return FootStep.GroundMaterial.None;
		}
		Collider lastGroundCollider = character.GetLastGroundCollider();
		if (lastGroundCollider == null)
		{
			return FootStep.GroundMaterial.Default;
		}
		Heightmap component = lastGroundCollider.GetComponent<Heightmap>();
		if (component != null)
		{
			float num = Mathf.Acos(Mathf.Clamp01(character.GetLastGroundNormal().y)) * 57.29578f;
			Heightmap.Biome biome = component.GetBiome(point);
			if (biome == Heightmap.Biome.Mountain || biome == Heightmap.Biome.DeepNorth)
			{
				if (num < 40f && !component.IsCleared(point))
				{
					return FootStep.GroundMaterial.Snow;
				}
			}
			else if (biome == Heightmap.Biome.Swamp)
			{
				if (num < 40f)
				{
					return FootStep.GroundMaterial.Mud;
				}
			}
			else if ((biome == Heightmap.Biome.Meadows || biome == Heightmap.Biome.BlackForest) && num < 25f)
			{
				return FootStep.GroundMaterial.Grass;
			}
			return FootStep.GroundMaterial.GenericGround;
		}
		if (lastGroundCollider.gameObject.layer != this.m_pieceLayer)
		{
			return FootStep.GroundMaterial.Default;
		}
		WearNTear componentInParent = lastGroundCollider.GetComponentInParent<WearNTear>();
		if (!componentInParent)
		{
			return FootStep.GroundMaterial.Default;
		}
		switch (componentInParent.m_materialType)
		{
		case WearNTear.MaterialType.Wood:
			return FootStep.GroundMaterial.Wood;
		case WearNTear.MaterialType.Stone:
		case WearNTear.MaterialType.Marble:
			return FootStep.GroundMaterial.Stone;
		case WearNTear.MaterialType.Iron:
			return FootStep.GroundMaterial.Metal;
		case WearNTear.MaterialType.HardWood:
			return FootStep.GroundMaterial.Wood;
		default:
			return FootStep.GroundMaterial.Default;
		}
	}

	// Token: 0x06000151 RID: 337 RVA: 0x000091EC File Offset: 0x000073EC
	public void FindJoints()
	{
		ZLog.Log("Finding joints");
		Transform transform = Utils.FindChild(base.transform, "LeftFootFront");
		Transform transform2 = Utils.FindChild(base.transform, "RightFootFront");
		Transform transform3 = Utils.FindChild(base.transform, "LeftFoot");
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "LeftFootBack");
		}
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "l_foot");
		}
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "Foot.l");
		}
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "foot.l");
		}
		Transform transform4 = Utils.FindChild(base.transform, "RightFoot");
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "RightFootBack");
		}
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "r_foot");
		}
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "Foot.r");
		}
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "foot.r");
		}
		List<Transform> list = new List<Transform>();
		if (transform)
		{
			list.Add(transform);
		}
		if (transform2)
		{
			list.Add(transform2);
		}
		if (transform3)
		{
			list.Add(transform3);
		}
		if (transform4)
		{
			list.Add(transform4);
		}
		this.m_feet = list.ToArray();
	}

	// Token: 0x06000152 RID: 338 RVA: 0x0000936C File Offset: 0x0000756C
	private int FindBestStepEffect(FootStep.GroundMaterial material, FootStep.MotionType motion)
	{
		FootStep.StepEffect stepEffect = null;
		int result = -1;
		for (int i = 0; i < this.m_effects.Count; i++)
		{
			FootStep.StepEffect stepEffect2 = this.m_effects[i];
			if (((stepEffect2.m_material & material) != FootStep.GroundMaterial.None || (stepEffect == null && (stepEffect2.m_material & FootStep.GroundMaterial.Default) != FootStep.GroundMaterial.None)) && (stepEffect2.m_motionType & motion) != (FootStep.MotionType)0)
			{
				stepEffect = stepEffect2;
				result = i;
			}
		}
		return result;
	}

	// Token: 0x17000004 RID: 4
	// (get) Token: 0x06000153 RID: 339 RVA: 0x000093C6 File Offset: 0x000075C6
	public static List<FootStep> Instances { get; } = new List<FootStep>();

	// Token: 0x04000147 RID: 327
	public float m_footstepCullDistance = 20f;

	// Token: 0x04000148 RID: 328
	public List<FootStep.StepEffect> m_effects = new List<FootStep.StepEffect>();

	// Token: 0x04000149 RID: 329
	public Transform[] m_feet = Array.Empty<Transform>();

	// Token: 0x0400014A RID: 330
	private static readonly int s_footstepID = ZSyncAnimation.GetHash("footstep");

	// Token: 0x0400014B RID: 331
	private static readonly int s_forwardSpeedID = ZSyncAnimation.GetHash("forward_speed");

	// Token: 0x0400014C RID: 332
	private static readonly int s_sidewaySpeedID = ZSyncAnimation.GetHash("sideway_speed");

	// Token: 0x0400014D RID: 333
	private static readonly Queue<GameObject> s_stepInstances = new Queue<GameObject>();

	// Token: 0x0400014E RID: 334
	private float m_footstep;

	// Token: 0x0400014F RID: 335
	private float m_footstepTimer;

	// Token: 0x04000150 RID: 336
	private int m_pieceLayer;

	// Token: 0x04000151 RID: 337
	private const float c_MinFootstepInterval = 0.2f;

	// Token: 0x04000152 RID: 338
	private const int c_MaxFootstepInstances = 30;

	// Token: 0x04000153 RID: 339
	private Animator m_animator;

	// Token: 0x04000154 RID: 340
	private Character m_character;

	// Token: 0x04000155 RID: 341
	private ZNetView m_nview;

	// Token: 0x02000012 RID: 18
	[Flags]
	public enum MotionType
	{
		// Token: 0x04000158 RID: 344
		Jog = 1,
		// Token: 0x04000159 RID: 345
		Run = 2,
		// Token: 0x0400015A RID: 346
		Sneak = 4,
		// Token: 0x0400015B RID: 347
		Climbing = 8,
		// Token: 0x0400015C RID: 348
		Swimming = 16,
		// Token: 0x0400015D RID: 349
		Land = 32,
		// Token: 0x0400015E RID: 350
		Walk = 64
	}

	// Token: 0x02000013 RID: 19
	[Flags]
	public enum GroundMaterial
	{
		// Token: 0x04000160 RID: 352
		None = 0,
		// Token: 0x04000161 RID: 353
		Default = 1,
		// Token: 0x04000162 RID: 354
		Water = 2,
		// Token: 0x04000163 RID: 355
		Stone = 4,
		// Token: 0x04000164 RID: 356
		Wood = 8,
		// Token: 0x04000165 RID: 357
		Snow = 16,
		// Token: 0x04000166 RID: 358
		Mud = 32,
		// Token: 0x04000167 RID: 359
		Grass = 64,
		// Token: 0x04000168 RID: 360
		GenericGround = 128,
		// Token: 0x04000169 RID: 361
		Metal = 256,
		// Token: 0x0400016A RID: 362
		Tar = 512
	}

	// Token: 0x02000014 RID: 20
	[Serializable]
	public class StepEffect
	{
		// Token: 0x0400016B RID: 363
		public string m_name = "";

		// Token: 0x0400016C RID: 364
		[BitMask(typeof(FootStep.MotionType))]
		public FootStep.MotionType m_motionType = FootStep.MotionType.Jog;

		// Token: 0x0400016D RID: 365
		[BitMask(typeof(FootStep.GroundMaterial))]
		public FootStep.GroundMaterial m_material = FootStep.GroundMaterial.Default;

		// Token: 0x0400016E RID: 366
		public GameObject[] m_effectPrefabs = Array.Empty<GameObject>();
	}
}
