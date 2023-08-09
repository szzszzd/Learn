using System;
using UnityEngine;

// Token: 0x02000283 RID: 643
public class RandomSpeak : MonoBehaviour
{
	// Token: 0x06001888 RID: 6280 RVA: 0x000A3A2D File Offset: 0x000A1C2D
	private void Start()
	{
		base.InvokeRepeating("Speak", UnityEngine.Random.Range(0f, this.m_interval), this.m_interval);
	}

	// Token: 0x06001889 RID: 6281 RVA: 0x000A3A50 File Offset: 0x000A1C50
	private void Speak()
	{
		if (UnityEngine.Random.value > this.m_chance)
		{
			return;
		}
		if (this.m_texts.Length == 0)
		{
			return;
		}
		if (Player.m_localPlayer == null || Vector3.Distance(base.transform.position, Player.m_localPlayer.transform.position) > this.m_triggerDistance)
		{
			return;
		}
		if (this.m_onlyOnItemStand && !base.gameObject.GetComponentInParent<ItemStand>())
		{
			return;
		}
		this.m_speakEffects.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
		string text = this.m_texts[UnityEngine.Random.Range(0, this.m_texts.Length)];
		Chat.instance.SetNpcText(base.gameObject, this.m_offset, this.m_cullDistance, this.m_ttl, this.m_topic, text, this.m_useLargeDialog);
		if (this.m_onlyOnce)
		{
			base.CancelInvoke("Speak");
		}
	}

	// Token: 0x04001A61 RID: 6753
	public float m_interval = 5f;

	// Token: 0x04001A62 RID: 6754
	public float m_chance = 0.5f;

	// Token: 0x04001A63 RID: 6755
	public float m_triggerDistance = 5f;

	// Token: 0x04001A64 RID: 6756
	public float m_cullDistance = 10f;

	// Token: 0x04001A65 RID: 6757
	public float m_ttl = 10f;

	// Token: 0x04001A66 RID: 6758
	public Vector3 m_offset = new Vector3(0f, 0f, 0f);

	// Token: 0x04001A67 RID: 6759
	public EffectList m_speakEffects = new EffectList();

	// Token: 0x04001A68 RID: 6760
	public bool m_useLargeDialog;

	// Token: 0x04001A69 RID: 6761
	public bool m_onlyOnce;

	// Token: 0x04001A6A RID: 6762
	public bool m_onlyOnItemStand;

	// Token: 0x04001A6B RID: 6763
	public string m_topic = "";

	// Token: 0x04001A6C RID: 6764
	public string[] m_texts = new string[0];
}
