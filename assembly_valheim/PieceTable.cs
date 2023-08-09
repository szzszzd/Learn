using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200012F RID: 303
public class PieceTable : MonoBehaviour
{
	// Token: 0x06000BCB RID: 3019 RVA: 0x000570D0 File Offset: 0x000552D0
	public void UpdateAvailable(HashSet<string> knownRecipies, Player player, bool hideUnavailable, bool noPlacementCost)
	{
		if (this.m_availablePieces.Count == 0)
		{
			for (int i = 0; i < 4; i++)
			{
				this.m_availablePieces.Add(new List<Piece>());
			}
		}
		foreach (List<Piece> list in this.m_availablePieces)
		{
			list.Clear();
		}
		foreach (GameObject gameObject in this.m_pieces)
		{
			Piece component = gameObject.GetComponent<Piece>();
			if (noPlacementCost || (knownRecipies.Contains(component.m_name) && component.m_enabled && (!hideUnavailable || player.HaveRequirements(component, Player.RequirementMode.CanAlmostBuild))))
			{
				if (component.m_category == Piece.PieceCategory.All)
				{
					for (int j = 0; j < 4; j++)
					{
						this.m_availablePieces[j].Add(component);
					}
				}
				else
				{
					this.m_availablePieces[(int)component.m_category].Add(component);
				}
			}
		}
	}

	// Token: 0x06000BCC RID: 3020 RVA: 0x000571FC File Offset: 0x000553FC
	public GameObject GetSelectedPrefab()
	{
		Piece selectedPiece = this.GetSelectedPiece();
		if (selectedPiece)
		{
			return selectedPiece.gameObject;
		}
		return null;
	}

	// Token: 0x06000BCD RID: 3021 RVA: 0x00057220 File Offset: 0x00055420
	public Piece GetPiece(int category, Vector2Int p)
	{
		if (this.m_availablePieces[category].Count == 0)
		{
			return null;
		}
		int num = p.y * 15 + p.x;
		if (num < 0 || num >= this.m_availablePieces[category].Count)
		{
			return null;
		}
		return this.m_availablePieces[category][num];
	}

	// Token: 0x06000BCE RID: 3022 RVA: 0x00057281 File Offset: 0x00055481
	public Piece GetPiece(Vector2Int p)
	{
		return this.GetPiece((int)this.m_selectedCategory, p);
	}

	// Token: 0x06000BCF RID: 3023 RVA: 0x00057290 File Offset: 0x00055490
	public bool IsPieceAvailable(Piece piece)
	{
		using (List<Piece>.Enumerator enumerator = this.m_availablePieces[(int)this.m_selectedCategory].GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == piece)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000BD0 RID: 3024 RVA: 0x000572F8 File Offset: 0x000554F8
	public Piece GetSelectedPiece()
	{
		Vector2Int selectedIndex = this.GetSelectedIndex();
		return this.GetPiece((int)this.m_selectedCategory, selectedIndex);
	}

	// Token: 0x06000BD1 RID: 3025 RVA: 0x00057319 File Offset: 0x00055519
	public int GetAvailablePiecesInCategory(Piece.PieceCategory cat)
	{
		return this.m_availablePieces[(int)cat].Count;
	}

	// Token: 0x06000BD2 RID: 3026 RVA: 0x0005732C File Offset: 0x0005552C
	public List<Piece> GetPiecesInSelectedCategory()
	{
		return this.m_availablePieces[(int)this.m_selectedCategory];
	}

	// Token: 0x06000BD3 RID: 3027 RVA: 0x0005733F File Offset: 0x0005553F
	public int GetAvailablePiecesInSelectedCategory()
	{
		return this.GetAvailablePiecesInCategory(this.m_selectedCategory);
	}

	// Token: 0x06000BD4 RID: 3028 RVA: 0x0005734D File Offset: 0x0005554D
	public Vector2Int GetSelectedIndex()
	{
		return this.m_selectedPiece[(int)this.m_selectedCategory];
	}

	// Token: 0x06000BD5 RID: 3029 RVA: 0x00057360 File Offset: 0x00055560
	public void SetSelected(Vector2Int p)
	{
		this.m_selectedPiece[(int)this.m_selectedCategory] = p;
	}

	// Token: 0x06000BD6 RID: 3030 RVA: 0x00057374 File Offset: 0x00055574
	public void LeftPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int x = vector2Int.x - 1;
		vector2Int.x = x;
		if (vector2Int.x < 0)
		{
			vector2Int.x = 14;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x06000BD7 RID: 3031 RVA: 0x000573E4 File Offset: 0x000555E4
	public void RightPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int x = vector2Int.x + 1;
		vector2Int.x = x;
		if (vector2Int.x >= 15)
		{
			vector2Int.x = 0;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x06000BD8 RID: 3032 RVA: 0x00057454 File Offset: 0x00055654
	public void DownPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int y = vector2Int.y + 1;
		vector2Int.y = y;
		if (vector2Int.y >= 6)
		{
			vector2Int.y = 0;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x06000BD9 RID: 3033 RVA: 0x000574C4 File Offset: 0x000556C4
	public void UpPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int y = vector2Int.y - 1;
		vector2Int.y = y;
		if (vector2Int.y < 0)
		{
			vector2Int.y = 5;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x06000BDA RID: 3034 RVA: 0x00057532 File Offset: 0x00055732
	public void NextCategory()
	{
		if (!this.m_useCategories)
		{
			return;
		}
		this.m_selectedCategory++;
		if (this.m_selectedCategory == Piece.PieceCategory.Max)
		{
			this.m_selectedCategory = Piece.PieceCategory.Misc;
		}
	}

	// Token: 0x06000BDB RID: 3035 RVA: 0x0005755B File Offset: 0x0005575B
	public void PrevCategory()
	{
		if (!this.m_useCategories)
		{
			return;
		}
		this.m_selectedCategory--;
		if (this.m_selectedCategory < Piece.PieceCategory.Misc)
		{
			this.m_selectedCategory = Piece.PieceCategory.Furniture;
		}
	}

	// Token: 0x06000BDC RID: 3036 RVA: 0x00057584 File Offset: 0x00055784
	public void SetCategory(int index)
	{
		if (!this.m_useCategories)
		{
			return;
		}
		this.m_selectedCategory = (Piece.PieceCategory)index;
		this.m_selectedCategory = (Piece.PieceCategory)Mathf.Clamp((int)this.m_selectedCategory, 0, 3);
	}

	// Token: 0x04000E39 RID: 3641
	public const int m_gridWidth = 15;

	// Token: 0x04000E3A RID: 3642
	public const int m_gridHeight = 6;

	// Token: 0x04000E3B RID: 3643
	public List<GameObject> m_pieces = new List<GameObject>();

	// Token: 0x04000E3C RID: 3644
	public bool m_useCategories = true;

	// Token: 0x04000E3D RID: 3645
	public bool m_canRemovePieces = true;

	// Token: 0x04000E3E RID: 3646
	[NonSerialized]
	private List<List<Piece>> m_availablePieces = new List<List<Piece>>();

	// Token: 0x04000E3F RID: 3647
	[NonSerialized]
	public Piece.PieceCategory m_selectedCategory;

	// Token: 0x04000E40 RID: 3648
	[NonSerialized]
	public Vector2Int[] m_selectedPiece = new Vector2Int[5];

	// Token: 0x04000E41 RID: 3649
	[NonSerialized]
	public Vector2Int[] m_lastSelectedPiece = new Vector2Int[5];
}
