using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000135 RID: 309
public class MonoUpdaters : MonoBehaviour
{
	// Token: 0x06000C08 RID: 3080 RVA: 0x00057C4C File Offset: 0x00055E4C
	private void Awake()
	{
		MonoUpdaters.s_instance = this;
	}

	// Token: 0x06000C09 RID: 3081 RVA: 0x00057C54 File Offset: 0x00055E54
	private void OnDestroy()
	{
		MonoUpdaters.s_instance = null;
	}

	// Token: 0x06000C0A RID: 3082 RVA: 0x00057C5C File Offset: 0x00055E5C
	private void FixedUpdate()
	{
		MonoUpdaters.s_updateCount++;
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.m_syncTransformInstances.AddRange(ZSyncTransform.Instances);
		this.m_syncAnimationInstances.AddRange(ZSyncAnimation.Instances);
		this.m_floatingInstances.AddRange(Floating.Instances);
		this.m_shipInstances.AddRange(Ship.Instances);
		this.m_fishInstances.AddRange(Fish.Instances);
		this.m_characterAnimEventInstances.AddRange(CharacterAnimEvent.Instances);
		this.m_baseAIInstances.AddRange(BaseAI.Instances);
		this.m_monsterAIInstances.AddRange(MonsterAI.Instances);
		this.m_animalAIInstances.AddRange(AnimalAI.Instances);
		this.m_humanoidInstances.AddRange(Humanoid.Instances);
		this.m_characterInstances.AddRange(Character.Instances);
		foreach (ZSyncTransform zsyncTransform in this.m_syncTransformInstances)
		{
			zsyncTransform.CustomFixedUpdate(fixedDeltaTime);
		}
		foreach (ZSyncAnimation zsyncAnimation in this.m_syncAnimationInstances)
		{
			zsyncAnimation.CustomFixedUpdate();
		}
		foreach (Floating floating in this.m_floatingInstances)
		{
			floating.CustomFixedUpdate(fixedDeltaTime);
		}
		foreach (Ship ship in this.m_shipInstances)
		{
			ship.CustomFixedUpdate();
		}
		foreach (Fish fish in this.m_fishInstances)
		{
			fish.CustomFixedUpdate();
		}
		foreach (CharacterAnimEvent characterAnimEvent in this.m_characterAnimEventInstances)
		{
			characterAnimEvent.CustomFixedUpdate();
		}
		this.m_updateAITimer += fixedDeltaTime;
		if (this.m_updateAITimer >= 0.05f)
		{
			foreach (BaseAI baseAI in this.m_baseAIInstances)
			{
				baseAI.UpdateAI(fixedDeltaTime);
			}
			foreach (MonsterAI monsterAI in this.m_monsterAIInstances)
			{
				monsterAI.UpdateAI(fixedDeltaTime);
			}
			foreach (AnimalAI animalAI in this.m_animalAIInstances)
			{
				animalAI.UpdateAI(fixedDeltaTime);
			}
			this.m_updateAITimer -= 0.05f;
		}
		foreach (Humanoid humanoid in this.m_humanoidInstances)
		{
			humanoid.CustomFixedUpdate();
		}
		foreach (Character character in this.m_characterInstances)
		{
			character.CustomFixedUpdate();
		}
		this.m_syncTransformInstances.Clear();
		this.m_syncAnimationInstances.Clear();
		this.m_floatingInstances.Clear();
		this.m_shipInstances.Clear();
		this.m_fishInstances.Clear();
		this.m_characterAnimEventInstances.Clear();
		this.m_baseAIInstances.Clear();
		this.m_monsterAIInstances.Clear();
		this.m_animalAIInstances.Clear();
		this.m_humanoidInstances.Clear();
		this.m_characterInstances.Clear();
	}

	// Token: 0x06000C0B RID: 3083 RVA: 0x0005809C File Offset: 0x0005629C
	private void Update()
	{
		MonoUpdaters.s_updateCount++;
		float deltaTime = Time.deltaTime;
		this.m_waterVolumeInstances.AddRange(WaterVolume.Instances);
		this.m_smokeInstances.AddRange(Smoke.Instances);
		this.m_zsfxInstances.AddRange(ZSFX.Instances);
		this.m_heightmapInstances.AddRange(Heightmap.Instances);
		this.m_visEquipmentInstances.AddRange(VisEquipment.Instances);
		this.m_footStepInstances.AddRange(FootStep.Instances);
		this.m_instanceRendererInstances.AddRange(InstanceRenderer.Instances);
		this.m_waterTriggerInstances.AddRange(WaterTrigger.Instances);
		if (this.m_waterVolumeInstances.Count > 0)
		{
			WaterVolume.StaticUpdate();
			foreach (WaterVolume waterVolume in this.m_waterVolumeInstances)
			{
				waterVolume.Update1();
			}
			foreach (WaterVolume waterVolume2 in this.m_waterVolumeInstances)
			{
				waterVolume2.Update2();
			}
		}
		foreach (Smoke smoke in this.m_smokeInstances)
		{
			smoke.CustomUpdate(deltaTime);
		}
		foreach (ZSFX zsfx in this.m_zsfxInstances)
		{
			zsfx.CustomUpdate(deltaTime);
		}
		if (RenderGroupSystem.IsGroupActive(RenderGroup.Overworld))
		{
			foreach (Heightmap heightmap in this.m_heightmapInstances)
			{
				heightmap.CustomUpdate();
			}
		}
		foreach (VisEquipment visEquipment in this.m_visEquipmentInstances)
		{
			visEquipment.CustomUpdate();
		}
		foreach (FootStep footStep in this.m_footStepInstances)
		{
			footStep.CustomUpdate(deltaTime);
		}
		foreach (InstanceRenderer instanceRenderer in this.m_instanceRendererInstances)
		{
			instanceRenderer.CustomUpdate();
		}
		foreach (WaterTrigger waterTrigger in this.m_waterTriggerInstances)
		{
			waterTrigger.CustomUpdate(deltaTime);
		}
		this.m_waterVolumeInstances.Clear();
		this.m_smokeInstances.Clear();
		this.m_zsfxInstances.Clear();
		this.m_heightmapInstances.Clear();
		this.m_visEquipmentInstances.Clear();
		this.m_footStepInstances.Clear();
		this.m_instanceRendererInstances.Clear();
		this.m_waterTriggerInstances.Clear();
	}

	// Token: 0x06000C0C RID: 3084 RVA: 0x000583F4 File Offset: 0x000565F4
	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		this.m_syncTransformInstances.AddRange(ZSyncTransform.Instances);
		this.m_characterAnimEventInstances.AddRange(CharacterAnimEvent.Instances);
		this.m_heightmapInstances.AddRange(Heightmap.Instances);
		this.m_shipEffectsInstances.AddRange(ShipEffects.Instances);
		this.m_tailInstances.AddRange(Tail.Instances);
		foreach (ZSyncTransform zsyncTransform in this.m_syncTransformInstances)
		{
			zsyncTransform.CustomLateUpdate();
		}
		foreach (CharacterAnimEvent characterAnimEvent in this.m_characterAnimEventInstances)
		{
			characterAnimEvent.CustomLateUpdate();
		}
		foreach (Heightmap heightmap in this.m_heightmapInstances)
		{
			heightmap.CustomLateUpdate();
		}
		foreach (ShipEffects shipEffects in this.m_shipEffectsInstances)
		{
			shipEffects.CustomLateUpdate();
		}
		foreach (Tail tail in this.m_tailInstances)
		{
			tail.CustomLateUpdate(deltaTime);
		}
		this.m_syncTransformInstances.Clear();
		this.m_characterAnimEventInstances.Clear();
		this.m_heightmapInstances.Clear();
		this.m_shipEffectsInstances.Clear();
		this.m_tailInstances.Clear();
	}

	// Token: 0x1700006D RID: 109
	// (get) Token: 0x06000C0D RID: 3085 RVA: 0x000585D0 File Offset: 0x000567D0
	public static int UpdateCount
	{
		get
		{
			return MonoUpdaters.s_updateCount;
		}
	}

	// Token: 0x04000E5D RID: 3677
	private static MonoUpdaters s_instance;

	// Token: 0x04000E5E RID: 3678
	private readonly List<ZSyncTransform> m_syncTransformInstances = new List<ZSyncTransform>();

	// Token: 0x04000E5F RID: 3679
	private readonly List<ZSyncAnimation> m_syncAnimationInstances = new List<ZSyncAnimation>();

	// Token: 0x04000E60 RID: 3680
	private readonly List<Floating> m_floatingInstances = new List<Floating>();

	// Token: 0x04000E61 RID: 3681
	private readonly List<Ship> m_shipInstances = new List<Ship>();

	// Token: 0x04000E62 RID: 3682
	private readonly List<Fish> m_fishInstances = new List<Fish>();

	// Token: 0x04000E63 RID: 3683
	private readonly List<CharacterAnimEvent> m_characterAnimEventInstances = new List<CharacterAnimEvent>();

	// Token: 0x04000E64 RID: 3684
	private readonly List<BaseAI> m_baseAIInstances = new List<BaseAI>();

	// Token: 0x04000E65 RID: 3685
	private readonly List<MonsterAI> m_monsterAIInstances = new List<MonsterAI>();

	// Token: 0x04000E66 RID: 3686
	private readonly List<AnimalAI> m_animalAIInstances = new List<AnimalAI>();

	// Token: 0x04000E67 RID: 3687
	private readonly List<Humanoid> m_humanoidInstances = new List<Humanoid>();

	// Token: 0x04000E68 RID: 3688
	private readonly List<Character> m_characterInstances = new List<Character>();

	// Token: 0x04000E69 RID: 3689
	private readonly List<WaterVolume> m_waterVolumeInstances = new List<WaterVolume>();

	// Token: 0x04000E6A RID: 3690
	private readonly List<Smoke> m_smokeInstances = new List<Smoke>();

	// Token: 0x04000E6B RID: 3691
	private readonly List<ZSFX> m_zsfxInstances = new List<ZSFX>();

	// Token: 0x04000E6C RID: 3692
	private readonly List<Heightmap> m_heightmapInstances = new List<Heightmap>();

	// Token: 0x04000E6D RID: 3693
	private readonly List<VisEquipment> m_visEquipmentInstances = new List<VisEquipment>();

	// Token: 0x04000E6E RID: 3694
	private readonly List<FootStep> m_footStepInstances = new List<FootStep>();

	// Token: 0x04000E6F RID: 3695
	private readonly List<InstanceRenderer> m_instanceRendererInstances = new List<InstanceRenderer>();

	// Token: 0x04000E70 RID: 3696
	private readonly List<WaterTrigger> m_waterTriggerInstances = new List<WaterTrigger>();

	// Token: 0x04000E71 RID: 3697
	private readonly List<ShipEffects> m_shipEffectsInstances = new List<ShipEffects>();

	// Token: 0x04000E72 RID: 3698
	private readonly List<Tail> m_tailInstances = new List<Tail>();

	// Token: 0x04000E73 RID: 3699
	private static int s_updateCount;

	// Token: 0x04000E74 RID: 3700
	private float m_updateAITimer;
}
