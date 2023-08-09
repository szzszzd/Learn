using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200011D RID: 285
public class VariantDialog : MonoBehaviour
{
	// Token: 0x06000B02 RID: 2818 RVA: 0x00051BCC File Offset: 0x0004FDCC
	public void Setup(ItemDrop.ItemData item)
	{
		base.gameObject.SetActive(true);
		foreach (GameObject obj in this.m_elements)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_elements.Clear();
		for (int i = 0; i < item.m_shared.m_variants; i++)
		{
			Sprite sprite = item.m_shared.m_icons[i];
			int num = i / this.m_gridWidth;
			int num2 = i % this.m_gridWidth;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, Vector3.zero, Quaternion.identity, this.m_listRoot);
			gameObject.SetActive(true);
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2((float)num2 * this.m_spacing, (float)(-(float)num) * this.m_spacing);
			Button component = gameObject.transform.Find("Button").GetComponent<Button>();
			int buttonIndex = i;
			component.onClick.AddListener(delegate
			{
				this.OnClicked(buttonIndex);
			});
			component.GetComponent<Image>().sprite = sprite;
			this.m_elements.Add(gameObject);
		}
	}

	// Token: 0x06000B03 RID: 2819 RVA: 0x00050D98 File Offset: 0x0004EF98
	public void OnClose()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x06000B04 RID: 2820 RVA: 0x00051D1C File Offset: 0x0004FF1C
	private void OnClicked(int index)
	{
		ZLog.Log("Clicked button " + index.ToString());
		base.gameObject.SetActive(false);
		this.m_selected(index);
	}

	// Token: 0x04000D36 RID: 3382
	public Transform m_listRoot;

	// Token: 0x04000D37 RID: 3383
	public GameObject m_elementPrefab;

	// Token: 0x04000D38 RID: 3384
	public float m_spacing = 70f;

	// Token: 0x04000D39 RID: 3385
	public int m_gridWidth = 5;

	// Token: 0x04000D3A RID: 3386
	private List<GameObject> m_elements = new List<GameObject>();

	// Token: 0x04000D3B RID: 3387
	public Action<int> m_selected;
}
