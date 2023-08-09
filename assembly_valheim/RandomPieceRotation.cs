using System;
using UnityEngine;

// Token: 0x02000281 RID: 641
public class RandomPieceRotation : MonoBehaviour
{
	// Token: 0x06001881 RID: 6273 RVA: 0x000A37E4 File Offset: 0x000A19E4
	private void Awake()
	{
		Vector3 position = base.transform.position;
		int seed = (int)position.x * (int)(position.y * 10f) * (int)(position.z * 100f);
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		float x = this.m_rotateX ? ((float)UnityEngine.Random.Range(0, this.m_stepsX) * 360f / (float)this.m_stepsX) : 0f;
		float y = this.m_rotateY ? ((float)UnityEngine.Random.Range(0, this.m_stepsY) * 360f / (float)this.m_stepsY) : 0f;
		float z = this.m_rotateZ ? ((float)UnityEngine.Random.Range(0, this.m_stepsZ) * 360f / (float)this.m_stepsZ) : 0f;
		base.transform.localRotation = Quaternion.Euler(x, y, z);
		UnityEngine.Random.state = state;
	}

	// Token: 0x04001A57 RID: 6743
	public bool m_rotateX;

	// Token: 0x04001A58 RID: 6744
	public bool m_rotateY;

	// Token: 0x04001A59 RID: 6745
	public bool m_rotateZ;

	// Token: 0x04001A5A RID: 6746
	public int m_stepsX = 4;

	// Token: 0x04001A5B RID: 6747
	public int m_stepsY = 4;

	// Token: 0x04001A5C RID: 6748
	public int m_stepsZ = 4;
}
