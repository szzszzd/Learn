using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x020000A8 RID: 168
public class EnemyHud : MonoBehaviour
{
	// Token: 0x1700002A RID: 42
	// (get) Token: 0x06000747 RID: 1863 RVA: 0x00037F96 File Offset: 0x00036196
	public static EnemyHud instance
	{
		get
		{
			return EnemyHud.m_instance;
		}
	}

	// Token: 0x06000748 RID: 1864 RVA: 0x00037F9D File Offset: 0x0003619D
	private void Awake()
	{
		EnemyHud.m_instance = this;
		this.m_baseHud.SetActive(false);
		this.m_baseHudBoss.SetActive(false);
		this.m_baseHudPlayer.SetActive(false);
		this.m_baseHudMount.SetActive(false);
	}

	// Token: 0x06000749 RID: 1865 RVA: 0x00037FD5 File Offset: 0x000361D5
	private void OnDestroy()
	{
		EnemyHud.m_instance = null;
	}

	// Token: 0x0600074A RID: 1866 RVA: 0x00037FE0 File Offset: 0x000361E0
	private void LateUpdate()
	{
		this.m_hudRoot.SetActive(!Hud.IsUserHidden());
		Sadle sadle = null;
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer != null)
		{
			this.m_refPoint = localPlayer.transform.position;
			sadle = (localPlayer.GetDoodadController() as Sadle);
		}
		foreach (Character character in Character.GetAllCharacters())
		{
			if (!(character == localPlayer) && (!sadle || !(character == sadle.GetCharacter())) && this.TestShow(character, false))
			{
				bool isMount = sadle && character == sadle.GetCharacter();
				this.ShowHud(character, isMount);
			}
		}
		this.UpdateHuds(localPlayer, sadle, Time.deltaTime);
	}

	// Token: 0x0600074B RID: 1867 RVA: 0x000380C4 File Offset: 0x000362C4
	private bool TestShow(Character c, bool isVisible)
	{
		float num = Vector3.SqrMagnitude(c.transform.position - this.m_refPoint);
		if (c.IsBoss() && num < this.m_maxShowDistanceBoss * this.m_maxShowDistanceBoss)
		{
			if (isVisible && c.m_dontHideBossHud)
			{
				return true;
			}
			if (c.GetComponent<BaseAI>().IsAlerted())
			{
				return true;
			}
		}
		else if (num < this.m_maxShowDistance * this.m_maxShowDistance)
		{
			return !c.IsPlayer() || !c.IsCrouching();
		}
		return false;
	}

	// Token: 0x0600074C RID: 1868 RVA: 0x00038148 File Offset: 0x00036348
	private void ShowHud(Character c, bool isMount)
	{
		EnemyHud.HudData hudData;
		if (this.m_huds.TryGetValue(c, out hudData))
		{
			return;
		}
		GameObject original;
		if (isMount)
		{
			original = this.m_baseHudMount;
		}
		else if (c.IsPlayer())
		{
			original = this.m_baseHudPlayer;
		}
		else if (c.IsBoss())
		{
			original = this.m_baseHudBoss;
		}
		else
		{
			original = this.m_baseHud;
		}
		hudData = new EnemyHud.HudData();
		hudData.m_character = c;
		hudData.m_ai = c.GetComponent<BaseAI>();
		hudData.m_gui = UnityEngine.Object.Instantiate<GameObject>(original, this.m_hudRoot.transform);
		hudData.m_gui.SetActive(true);
		hudData.m_healthFast = hudData.m_gui.transform.Find("Health/health_fast").GetComponent<GuiBar>();
		hudData.m_healthSlow = hudData.m_gui.transform.Find("Health/health_slow").GetComponent<GuiBar>();
		Transform transform = hudData.m_gui.transform.Find("Health/health_fast_friendly");
		if (transform)
		{
			hudData.m_healthFastFriendly = transform.GetComponent<GuiBar>();
		}
		if (isMount)
		{
			hudData.m_stamina = hudData.m_gui.transform.Find("Stamina/stamina_fast").GetComponent<GuiBar>();
			hudData.m_staminaText = hudData.m_gui.transform.Find("Stamina/StaminaText").GetComponent<TextMeshProUGUI>();
			hudData.m_healthText = hudData.m_gui.transform.Find("Health/HealthText").GetComponent<TextMeshProUGUI>();
		}
		hudData.m_level2 = (hudData.m_gui.transform.Find("level_2") as RectTransform);
		hudData.m_level3 = (hudData.m_gui.transform.Find("level_3") as RectTransform);
		hudData.m_alerted = (hudData.m_gui.transform.Find("Alerted") as RectTransform);
		hudData.m_aware = (hudData.m_gui.transform.Find("Aware") as RectTransform);
		hudData.m_name = hudData.m_gui.transform.Find("Name").GetComponent<TextMeshProUGUI>();
		hudData.m_name.text = Localization.instance.Localize(c.GetHoverName());
		hudData.m_isMount = isMount;
		this.m_huds.Add(c, hudData);
	}

	// Token: 0x0600074D RID: 1869 RVA: 0x00038374 File Offset: 0x00036574
	private void UpdateHuds(Player player, Sadle sadle, float dt)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (!mainCamera)
		{
			return;
		}
		Character y = sadle ? sadle.GetCharacter() : null;
		Character y2 = player ? player.GetHoverCreature() : null;
		Character character = null;
		foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in this.m_huds)
		{
			EnemyHud.HudData value = keyValuePair.Value;
			if (!value.m_character || !this.TestShow(value.m_character, true) || value.m_character == y)
			{
				if (character == null)
				{
					character = value.m_character;
					UnityEngine.Object.Destroy(value.m_gui);
				}
			}
			else
			{
				if (value.m_character == y2)
				{
					value.m_hoverTimer = 0f;
				}
				value.m_hoverTimer += dt;
				float healthPercentage = value.m_character.GetHealthPercentage();
				if (value.m_character.IsPlayer() || value.m_character.IsBoss() || value.m_isMount || value.m_hoverTimer < this.m_hoverShowDuration)
				{
					value.m_gui.SetActive(true);
					int level = value.m_character.GetLevel();
					if (value.m_level2)
					{
						value.m_level2.gameObject.SetActive(level == 2);
					}
					if (value.m_level3)
					{
						value.m_level3.gameObject.SetActive(level == 3);
					}
					value.m_name.text = Localization.instance.Localize(value.m_character.GetHoverName());
					if (!value.m_character.IsBoss() && !value.m_character.IsPlayer())
					{
						bool flag = value.m_character.GetBaseAI().HaveTarget();
						bool flag2 = value.m_character.GetBaseAI().IsAlerted();
						value.m_alerted.gameObject.SetActive(flag2);
						value.m_aware.gameObject.SetActive(!flag2 && flag);
					}
				}
				else
				{
					value.m_gui.SetActive(false);
				}
				value.m_healthSlow.SetValue(healthPercentage);
				if (value.m_healthFastFriendly)
				{
					bool flag3 = !player || BaseAI.IsEnemy(player, value.m_character);
					value.m_healthFast.gameObject.SetActive(flag3);
					value.m_healthFastFriendly.gameObject.SetActive(!flag3);
					value.m_healthFast.SetValue(healthPercentage);
					value.m_healthFastFriendly.SetValue(healthPercentage);
				}
				else
				{
					value.m_healthFast.SetValue(healthPercentage);
				}
				if (value.m_isMount)
				{
					float stamina = sadle.GetStamina();
					float maxStamina = sadle.GetMaxStamina();
					value.m_stamina.SetValue(stamina / maxStamina);
					value.m_healthText.text = Mathf.CeilToInt(value.m_character.GetHealth()).ToString();
					value.m_staminaText.text = Mathf.CeilToInt(stamina).ToString();
				}
				if (!value.m_character.IsBoss() && value.m_gui.activeSelf)
				{
					Vector3 position = Vector3.zero;
					if (value.m_character.IsPlayer())
					{
						position = value.m_character.GetHeadPoint() + Vector3.up * 0.3f;
					}
					else if (value.m_isMount)
					{
						position = player.transform.position - player.transform.up * 0.5f;
					}
					else
					{
						position = value.m_character.GetTopPoint();
					}
					Vector3 vector = mainCamera.WorldToScreenPoint(position);
					if (vector.x < 0f || vector.x > (float)Screen.width || vector.y < 0f || vector.y > (float)Screen.height || vector.z > 0f)
					{
						value.m_gui.transform.position = vector;
						value.m_gui.SetActive(true);
					}
					else
					{
						value.m_gui.SetActive(false);
					}
				}
			}
		}
		if (character != null)
		{
			this.m_huds.Remove(character);
		}
	}

	// Token: 0x0600074E RID: 1870 RVA: 0x00038800 File Offset: 0x00036A00
	public bool ShowingBossHud()
	{
		foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in this.m_huds)
		{
			if (keyValuePair.Value.m_character && keyValuePair.Value.m_character.IsBoss())
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600074F RID: 1871 RVA: 0x0003887C File Offset: 0x00036A7C
	public Character GetActiveBoss()
	{
		foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in this.m_huds)
		{
			if (keyValuePair.Value.m_character && keyValuePair.Value.m_character.IsBoss())
			{
				return keyValuePair.Value.m_character;
			}
		}
		return null;
	}

	// Token: 0x040008D6 RID: 2262
	private static EnemyHud m_instance;

	// Token: 0x040008D7 RID: 2263
	public GameObject m_hudRoot;

	// Token: 0x040008D8 RID: 2264
	public GameObject m_baseHud;

	// Token: 0x040008D9 RID: 2265
	public GameObject m_baseHudBoss;

	// Token: 0x040008DA RID: 2266
	public GameObject m_baseHudPlayer;

	// Token: 0x040008DB RID: 2267
	public GameObject m_baseHudMount;

	// Token: 0x040008DC RID: 2268
	public float m_maxShowDistance = 10f;

	// Token: 0x040008DD RID: 2269
	public float m_maxShowDistanceBoss = 100f;

	// Token: 0x040008DE RID: 2270
	public float m_hoverShowDuration = 60f;

	// Token: 0x040008DF RID: 2271
	private Vector3 m_refPoint = Vector3.zero;

	// Token: 0x040008E0 RID: 2272
	private Dictionary<Character, EnemyHud.HudData> m_huds = new Dictionary<Character, EnemyHud.HudData>();

	// Token: 0x020000A9 RID: 169
	private class HudData
	{
		// Token: 0x040008E1 RID: 2273
		public Character m_character;

		// Token: 0x040008E2 RID: 2274
		public BaseAI m_ai;

		// Token: 0x040008E3 RID: 2275
		public GameObject m_gui;

		// Token: 0x040008E4 RID: 2276
		public RectTransform m_level2;

		// Token: 0x040008E5 RID: 2277
		public RectTransform m_level3;

		// Token: 0x040008E6 RID: 2278
		public RectTransform m_alerted;

		// Token: 0x040008E7 RID: 2279
		public RectTransform m_aware;

		// Token: 0x040008E8 RID: 2280
		public GuiBar m_healthFast;

		// Token: 0x040008E9 RID: 2281
		public GuiBar m_healthFastFriendly;

		// Token: 0x040008EA RID: 2282
		public GuiBar m_healthSlow;

		// Token: 0x040008EB RID: 2283
		public TextMeshProUGUI m_healthText;

		// Token: 0x040008EC RID: 2284
		public GuiBar m_stamina;

		// Token: 0x040008ED RID: 2285
		public TextMeshProUGUI m_staminaText;

		// Token: 0x040008EE RID: 2286
		public TextMeshProUGUI m_name;

		// Token: 0x040008EF RID: 2287
		public float m_hoverTimer = 99999f;

		// Token: 0x040008F0 RID: 2288
		public bool m_isMount;
	}
}
