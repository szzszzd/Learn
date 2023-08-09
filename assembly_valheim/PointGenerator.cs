using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000279 RID: 633
public class PointGenerator
{
	// Token: 0x06001838 RID: 6200 RVA: 0x000A18B4 File Offset: 0x0009FAB4
	public PointGenerator(int amount, float gridSize)
	{
		this.m_amount = amount;
		this.m_gridSize = gridSize;
	}

	// Token: 0x06001839 RID: 6201 RVA: 0x000A1900 File Offset: 0x0009FB00
	public void Update(Vector3 center, float radius, List<Vector3> newPoints, List<Vector3> removedPoints)
	{
		Vector2Int grid = this.GetGrid(center);
		if (this.m_currentCenterGrid == grid)
		{
			newPoints.Clear();
			removedPoints.Clear();
			return;
		}
		int num = Mathf.CeilToInt(radius / this.m_gridSize);
		if (this.m_currentCenterGrid != grid || this.m_currentGridWith != num)
		{
			this.RegeneratePoints(grid, num);
		}
	}

	// Token: 0x0600183A RID: 6202 RVA: 0x000A1960 File Offset: 0x0009FB60
	private void RegeneratePoints(Vector2Int centerGrid, int gridWith)
	{
		this.m_currentCenterGrid = centerGrid;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		this.m_points.Clear();
		for (int i = centerGrid.y - gridWith; i <= centerGrid.y + gridWith; i++)
		{
			for (int j = centerGrid.x - gridWith; j <= centerGrid.x + gridWith; j++)
			{
				UnityEngine.Random.InitState(j + i * 100);
				Vector3 gridPos = this.GetGridPos(new Vector2Int(j, i));
				for (int k = 0; k < this.m_amount; k++)
				{
					Vector3 item = new Vector3(UnityEngine.Random.Range(gridPos.x - this.m_gridSize, gridPos.x + this.m_gridSize), UnityEngine.Random.Range(gridPos.z - this.m_gridSize, gridPos.z + this.m_gridSize));
					this.m_points.Add(item);
				}
			}
		}
		UnityEngine.Random.state = state;
	}

	// Token: 0x0600183B RID: 6203 RVA: 0x000A1A50 File Offset: 0x0009FC50
	public Vector2Int GetGrid(Vector3 point)
	{
		int x = Mathf.FloorToInt((point.x + this.m_gridSize / 2f) / this.m_gridSize);
		int y = Mathf.FloorToInt((point.z + this.m_gridSize / 2f) / this.m_gridSize);
		return new Vector2Int(x, y);
	}

	// Token: 0x0600183C RID: 6204 RVA: 0x000A1AA2 File Offset: 0x0009FCA2
	public Vector3 GetGridPos(Vector2Int grid)
	{
		return new Vector3((float)grid.x * this.m_gridSize, 0f, (float)grid.y * this.m_gridSize);
	}

	// Token: 0x04001A08 RID: 6664
	private int m_amount;

	// Token: 0x04001A09 RID: 6665
	private float m_gridSize = 8f;

	// Token: 0x04001A0A RID: 6666
	private Vector2Int m_currentCenterGrid = new Vector2Int(99999, 99999);

	// Token: 0x04001A0B RID: 6667
	private int m_currentGridWith;

	// Token: 0x04001A0C RID: 6668
	private List<Vector3> m_points = new List<Vector3>();
}
