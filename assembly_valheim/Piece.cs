using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000273 RID: 627
public class Piece : StaticTarget
{
	// Token: 0x06001817 RID: 6167 RVA: 0x000A0AFC File Offset: 0x0009ECFC
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		Piece.s_allPieces.Add(this);
		this.m_myListIndex = Piece.s_allPieces.Count - 1;
		if (this.m_comfort > 0)
		{
			Piece.s_allComfortPieces.Add(this);
		}
		if (this.m_nview && this.m_nview.IsValid())
		{
			this.m_creator = this.m_nview.GetZDO().GetLong(ZDOVars.s_creator, 0L);
		}
		if (Piece.s_ghostLayer == 0)
		{
			Piece.s_ghostLayer = LayerMask.NameToLayer("ghost");
		}
		if (Piece.s_pieceRayMask == 0)
		{
			Piece.s_pieceRayMask = LayerMask.GetMask(new string[]
			{
				"piece",
				"piece_nonsolid"
			});
		}
	}

	// Token: 0x06001818 RID: 6168 RVA: 0x000A0BBC File Offset: 0x0009EDBC
	private void OnDestroy()
	{
		if (this.m_myListIndex >= 0)
		{
			Piece.s_allPieces[this.m_myListIndex] = Piece.s_allPieces[Piece.s_allPieces.Count - 1];
			Piece.s_allPieces[this.m_myListIndex].m_myListIndex = this.m_myListIndex;
			Piece.s_allPieces.RemoveAt(Piece.s_allPieces.Count - 1);
			this.m_myListIndex = -1;
		}
		if (this.m_comfort > 0)
		{
			Piece.s_allComfortPieces.Remove(this);
		}
	}

	// Token: 0x06001819 RID: 6169 RVA: 0x000A0C48 File Offset: 0x0009EE48
	public bool CanBeRemoved()
	{
		Container componentInChildren = base.GetComponentInChildren<Container>();
		if (componentInChildren != null)
		{
			return componentInChildren.CanBeRemoved();
		}
		Ship componentInChildren2 = base.GetComponentInChildren<Ship>();
		return !(componentInChildren2 != null) || componentInChildren2.CanBeRemoved();
	}

	// Token: 0x0600181A RID: 6170 RVA: 0x000A0C84 File Offset: 0x0009EE84
	public void DropResources()
	{
		Container container = null;
		foreach (Piece.Requirement requirement in this.m_resources)
		{
			if (!(requirement.m_resItem == null) && requirement.m_recover)
			{
				GameObject gameObject = requirement.m_resItem.gameObject;
				int j = requirement.m_amount;
				if (!this.IsPlacedByPlayer())
				{
					j = Mathf.Max(1, j / 3);
				}
				if (this.m_destroyedLootPrefab)
				{
					while (j > 0)
					{
						ItemDrop.ItemData itemData = gameObject.GetComponent<ItemDrop>().m_itemData.Clone();
						itemData.m_dropPrefab = gameObject;
						itemData.m_stack = Mathf.Min(j, itemData.m_shared.m_maxStackSize);
						j -= itemData.m_stack;
						if (container == null || !container.GetInventory().HaveEmptySlot())
						{
							container = UnityEngine.Object.Instantiate<GameObject>(this.m_destroyedLootPrefab, base.transform.position + Vector3.up, Quaternion.identity).GetComponent<Container>();
						}
						container.GetInventory().AddItem(itemData);
					}
				}
				else
				{
					while (j > 0)
					{
						ItemDrop component = UnityEngine.Object.Instantiate<GameObject>(gameObject, base.transform.position + Vector3.up, Quaternion.identity).GetComponent<ItemDrop>();
						component.SetStack(Mathf.Min(j, component.m_itemData.m_shared.m_maxStackSize));
						j -= component.m_itemData.m_stack;
					}
				}
			}
		}
	}

	// Token: 0x0600181B RID: 6171 RVA: 0x000A0E04 File Offset: 0x0009F004
	public override bool IsPriorityTarget()
	{
		return base.IsPriorityTarget() && (this.m_targetNonPlayerBuilt || this.IsPlacedByPlayer());
	}

	// Token: 0x0600181C RID: 6172 RVA: 0x000A0E20 File Offset: 0x0009F020
	public override bool IsRandomTarget()
	{
		return base.IsRandomTarget() && (this.m_targetNonPlayerBuilt || this.IsPlacedByPlayer());
	}

	// Token: 0x0600181D RID: 6173 RVA: 0x000A0E3C File Offset: 0x0009F03C
	public void SetCreator(long uid)
	{
		if (this.m_nview == null)
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			if (this.GetCreator() != 0L)
			{
				return;
			}
			this.m_creator = uid;
			this.m_nview.GetZDO().Set(ZDOVars.s_creator, uid);
		}
	}

	// Token: 0x0600181E RID: 6174 RVA: 0x000A0E8B File Offset: 0x0009F08B
	public long GetCreator()
	{
		return this.m_creator;
	}

	// Token: 0x0600181F RID: 6175 RVA: 0x000A0E94 File Offset: 0x0009F094
	public bool IsCreator()
	{
		long creator = this.GetCreator();
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		return creator == playerID;
	}

	// Token: 0x06001820 RID: 6176 RVA: 0x000A0EBA File Offset: 0x0009F0BA
	public bool IsPlacedByPlayer()
	{
		return this.GetCreator() != 0L;
	}

	// Token: 0x06001821 RID: 6177 RVA: 0x000A0EC8 File Offset: 0x0009F0C8
	public void SetInvalidPlacementHeightlight(bool enabled)
	{
		if ((enabled && this.m_invalidPlacementMaterials != null) || (!enabled && this.m_invalidPlacementMaterials == null))
		{
			return;
		}
		Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
		if (enabled)
		{
			this.m_invalidPlacementMaterials = new List<KeyValuePair<Renderer, Material[]>>();
			foreach (Renderer renderer in componentsInChildren)
			{
				Material[] sharedMaterials = renderer.sharedMaterials;
				this.m_invalidPlacementMaterials.Add(new KeyValuePair<Renderer, Material[]>(renderer, sharedMaterials));
			}
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Material material in array[i].materials)
				{
					if (material.HasProperty("_EmissionColor"))
					{
						material.SetColor("_EmissionColor", Color.red * 0.7f);
					}
					material.color = Color.red;
				}
			}
			return;
		}
		foreach (KeyValuePair<Renderer, Material[]> keyValuePair in this.m_invalidPlacementMaterials)
		{
			if (keyValuePair.Key)
			{
				keyValuePair.Key.materials = keyValuePair.Value;
			}
		}
		this.m_invalidPlacementMaterials = null;
	}

	// Token: 0x06001822 RID: 6178 RVA: 0x000A1008 File Offset: 0x0009F208
	public static void GetSnapPoints(Vector3 point, float radius, List<Transform> points, List<Piece> pieces)
	{
		int num = Physics.OverlapSphereNonAlloc(point, radius, Piece.s_pieceColliders, Piece.s_pieceRayMask);
		for (int i = 0; i < num; i++)
		{
			Piece componentInParent = Piece.s_pieceColliders[i].GetComponentInParent<Piece>();
			if (componentInParent != null)
			{
				componentInParent.GetSnapPoints(points);
				pieces.Add(componentInParent);
			}
		}
	}

	// Token: 0x06001823 RID: 6179 RVA: 0x000A1058 File Offset: 0x0009F258
	public static void GetAllComfortPiecesInRadius(Vector3 p, float radius, List<Piece> pieces)
	{
		foreach (Piece piece in Piece.s_allComfortPieces)
		{
			if (piece.gameObject.layer != Piece.s_ghostLayer && Vector3.Distance(p, piece.transform.position) < radius)
			{
				pieces.Add(piece);
			}
		}
	}

	// Token: 0x06001824 RID: 6180 RVA: 0x000A10D0 File Offset: 0x0009F2D0
	public void GetSnapPoints(List<Transform> points)
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.CompareTag("snappoint"))
			{
				points.Add(child);
			}
		}
	}

	// Token: 0x06001825 RID: 6181 RVA: 0x000A1114 File Offset: 0x0009F314
	public int GetComfort()
	{
		if (this.m_comfortObject != null && !this.m_comfortObject.activeInHierarchy)
		{
			return 0;
		}
		return this.m_comfort;
	}

	// Token: 0x040019A6 RID: 6566
	public bool m_targetNonPlayerBuilt = true;

	// Token: 0x040019A7 RID: 6567
	[Header("Basic stuffs")]
	public Sprite m_icon;

	// Token: 0x040019A8 RID: 6568
	public string m_name = "";

	// Token: 0x040019A9 RID: 6569
	public string m_description = "";

	// Token: 0x040019AA RID: 6570
	public bool m_enabled = true;

	// Token: 0x040019AB RID: 6571
	public Piece.PieceCategory m_category;

	// Token: 0x040019AC RID: 6572
	public bool m_isUpgrade;

	// Token: 0x040019AD RID: 6573
	[Header("Comfort")]
	public int m_comfort;

	// Token: 0x040019AE RID: 6574
	public Piece.ComfortGroup m_comfortGroup;

	// Token: 0x040019AF RID: 6575
	public GameObject m_comfortObject;

	// Token: 0x040019B0 RID: 6576
	[Header("Placement rules")]
	public bool m_groundPiece;

	// Token: 0x040019B1 RID: 6577
	public bool m_allowAltGroundPlacement;

	// Token: 0x040019B2 RID: 6578
	public bool m_groundOnly;

	// Token: 0x040019B3 RID: 6579
	public bool m_cultivatedGroundOnly;

	// Token: 0x040019B4 RID: 6580
	public bool m_waterPiece;

	// Token: 0x040019B5 RID: 6581
	public bool m_clipGround;

	// Token: 0x040019B6 RID: 6582
	public bool m_clipEverything;

	// Token: 0x040019B7 RID: 6583
	public bool m_noInWater;

	// Token: 0x040019B8 RID: 6584
	public bool m_notOnWood;

	// Token: 0x040019B9 RID: 6585
	public bool m_notOnTiltingSurface;

	// Token: 0x040019BA RID: 6586
	public bool m_inCeilingOnly;

	// Token: 0x040019BB RID: 6587
	public bool m_notOnFloor;

	// Token: 0x040019BC RID: 6588
	public bool m_noClipping;

	// Token: 0x040019BD RID: 6589
	public bool m_onlyInTeleportArea;

	// Token: 0x040019BE RID: 6590
	public bool m_allowedInDungeons;

	// Token: 0x040019BF RID: 6591
	public float m_spaceRequirement;

	// Token: 0x040019C0 RID: 6592
	public bool m_repairPiece;

	// Token: 0x040019C1 RID: 6593
	public bool m_canBeRemoved = true;

	// Token: 0x040019C2 RID: 6594
	public bool m_allowRotatedOverlap;

	// Token: 0x040019C3 RID: 6595
	public bool m_vegetationGroundOnly;

	// Token: 0x040019C4 RID: 6596
	public List<Piece> m_blockingPieces = new List<Piece>();

	// Token: 0x040019C5 RID: 6597
	public float m_blockRadius;

	// Token: 0x040019C6 RID: 6598
	public ZNetView m_mustConnectTo;

	// Token: 0x040019C7 RID: 6599
	public float m_connectRadius;

	// Token: 0x040019C8 RID: 6600
	public bool m_mustBeAboveConnected;

	// Token: 0x040019C9 RID: 6601
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_onlyInBiome;

	// Token: 0x040019CA RID: 6602
	[Header("Effects")]
	public EffectList m_placeEffect = new EffectList();

	// Token: 0x040019CB RID: 6603
	[Header("Requirements")]
	public string m_dlc = "";

	// Token: 0x040019CC RID: 6604
	public CraftingStation m_craftingStation;

	// Token: 0x040019CD RID: 6605
	public Piece.Requirement[] m_resources = Array.Empty<Piece.Requirement>();

	// Token: 0x040019CE RID: 6606
	public GameObject m_destroyedLootPrefab;

	// Token: 0x040019CF RID: 6607
	private ZNetView m_nview;

	// Token: 0x040019D0 RID: 6608
	private List<KeyValuePair<Renderer, Material[]>> m_invalidPlacementMaterials;

	// Token: 0x040019D1 RID: 6609
	private long m_creator;

	// Token: 0x040019D2 RID: 6610
	private int m_myListIndex = -1;

	// Token: 0x040019D3 RID: 6611
	private static int s_ghostLayer = 0;

	// Token: 0x040019D4 RID: 6612
	private static int s_pieceRayMask = 0;

	// Token: 0x040019D5 RID: 6613
	private static readonly Collider[] s_pieceColliders = new Collider[2000];

	// Token: 0x040019D6 RID: 6614
	private static readonly List<Piece> s_allPieces = new List<Piece>();

	// Token: 0x040019D7 RID: 6615
	private static readonly HashSet<Piece> s_allComfortPieces = new HashSet<Piece>();

	// Token: 0x02000274 RID: 628
	public enum PieceCategory
	{
		// Token: 0x040019D9 RID: 6617
		Misc,
		// Token: 0x040019DA RID: 6618
		Crafting,
		// Token: 0x040019DB RID: 6619
		Building,
		// Token: 0x040019DC RID: 6620
		Furniture,
		// Token: 0x040019DD RID: 6621
		Max,
		// Token: 0x040019DE RID: 6622
		All = 100
	}

	// Token: 0x02000275 RID: 629
	public enum ComfortGroup
	{
		// Token: 0x040019E0 RID: 6624
		None,
		// Token: 0x040019E1 RID: 6625
		Fire,
		// Token: 0x040019E2 RID: 6626
		Bed,
		// Token: 0x040019E3 RID: 6627
		Banner,
		// Token: 0x040019E4 RID: 6628
		Chair,
		// Token: 0x040019E5 RID: 6629
		Table,
		// Token: 0x040019E6 RID: 6630
		Carpet
	}

	// Token: 0x02000276 RID: 630
	[Serializable]
	public class Requirement
	{
		// Token: 0x06001828 RID: 6184 RVA: 0x000A11DE File Offset: 0x0009F3DE
		public int GetAmount(int qualityLevel)
		{
			if (qualityLevel <= 1)
			{
				return this.m_amount;
			}
			return (qualityLevel - 1) * this.m_amountPerLevel;
		}

		// Token: 0x040019E7 RID: 6631
		[Header("Resource")]
		public ItemDrop m_resItem;

		// Token: 0x040019E8 RID: 6632
		public int m_amount = 1;

		// Token: 0x040019E9 RID: 6633
		public int m_extraAmountOnlyOneIngredient;

		// Token: 0x040019EA RID: 6634
		[Header("Item")]
		public int m_amountPerLevel = 1;

		// Token: 0x040019EB RID: 6635
		[Header("Piece")]
		public bool m_recover = true;
	}
}
