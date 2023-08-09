using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000288 RID: 648
public class RuneStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x060018C1 RID: 6337 RVA: 0x000A4F3D File Offset: 0x000A313D
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_rune_read");
	}

	// Token: 0x060018C2 RID: 6338 RVA: 0x000A4F59 File Offset: 0x000A3159
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x060018C3 RID: 6339 RVA: 0x000A4F64 File Offset: 0x000A3164
	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		if (hold)
		{
			return false;
		}
		Player player = character as Player;
		if (!string.IsNullOrEmpty(this.m_locationName))
		{
			Game.instance.DiscoverClosestLocation(this.m_locationName, base.transform.position, this.m_pinName, (int)this.m_pinType, this.m_showMap);
		}
		RuneStone.RandomRuneText randomText = this.GetRandomText();
		if (randomText != null)
		{
			if (randomText.m_label.Length > 0)
			{
				player.AddKnownText(randomText.m_label, randomText.m_text);
			}
			TextViewer.instance.ShowText(TextViewer.Style.Rune, randomText.m_topic, randomText.m_text, true);
		}
		else
		{
			if (this.m_label.Length > 0)
			{
				player.AddKnownText(this.m_label, this.m_text);
			}
			TextViewer.instance.ShowText(TextViewer.Style.Rune, this.m_topic, this.m_text, true);
		}
		return false;
	}

	// Token: 0x060018C4 RID: 6340 RVA: 0x0000247B File Offset: 0x0000067B
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060018C5 RID: 6341 RVA: 0x000A5034 File Offset: 0x000A3234
	private RuneStone.RandomRuneText GetRandomText()
	{
		if (this.m_randomTexts.Count == 0)
		{
			return null;
		}
		Vector3 position = base.transform.position;
		int seed = (int)position.x * (int)position.z;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(seed);
		RuneStone.RandomRuneText result = this.m_randomTexts[UnityEngine.Random.Range(0, this.m_randomTexts.Count)];
		UnityEngine.Random.state = state;
		return result;
	}

	// Token: 0x04001AAD RID: 6829
	public string m_name = "Rune stone";

	// Token: 0x04001AAE RID: 6830
	public string m_topic = "";

	// Token: 0x04001AAF RID: 6831
	public string m_label = "";

	// Token: 0x04001AB0 RID: 6832
	[TextArea]
	public string m_text = "";

	// Token: 0x04001AB1 RID: 6833
	public List<RuneStone.RandomRuneText> m_randomTexts;

	// Token: 0x04001AB2 RID: 6834
	public string m_locationName = "";

	// Token: 0x04001AB3 RID: 6835
	public string m_pinName = "Pin";

	// Token: 0x04001AB4 RID: 6836
	public Minimap.PinType m_pinType = Minimap.PinType.Boss;

	// Token: 0x04001AB5 RID: 6837
	public bool m_showMap;

	// Token: 0x02000289 RID: 649
	[Serializable]
	public class RandomRuneText
	{
		// Token: 0x04001AB6 RID: 6838
		public string m_topic = "";

		// Token: 0x04001AB7 RID: 6839
		public string m_label = "";

		// Token: 0x04001AB8 RID: 6840
		public string m_text = "";
	}
}
