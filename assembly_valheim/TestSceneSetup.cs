using System;
using UnityEngine;

// Token: 0x020002A9 RID: 681
public class TestSceneSetup : MonoBehaviour
{
	// Token: 0x060019EC RID: 6636 RVA: 0x000AC108 File Offset: 0x000AA308
	private void Awake()
	{
		WorldGenerator.Initialize(World.GetMenuWorld());
	}

	// Token: 0x060019ED RID: 6637 RVA: 0x000023E2 File Offset: 0x000005E2
	private void Update()
	{
	}
}
