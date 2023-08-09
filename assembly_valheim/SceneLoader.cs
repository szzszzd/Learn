using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020001F5 RID: 501
public class SceneLoader : MonoBehaviour
{
	// Token: 0x0600143A RID: 5178 RVA: 0x00084306 File Offset: 0x00082506
	private void Start()
	{
		this.StartLoading();
	}

	// Token: 0x0600143B RID: 5179 RVA: 0x0008430E File Offset: 0x0008250E
	private IEnumerator LoadYourAsyncScene()
	{
		ZLog.Log("Starting to load scene:" + this.m_scene);
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(this.m_scene, LoadSceneMode.Single);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		yield break;
	}

	// Token: 0x0600143C RID: 5180 RVA: 0x0008431D File Offset: 0x0008251D
	private void StartLoading()
	{
		base.StartCoroutine(this.LoadYourAsyncScene());
	}

	// Token: 0x040014E7 RID: 5351
	public string m_scene = "";
}
