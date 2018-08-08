using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTitlePanel : MonoBehaviour
{
	class PlayerTitle
	{
		public Text Title;
		public Player Player;
	}

	public Canvas Canvas;

	public Text TitlePrefab;

	private List<PlayerTitle> playerTitles = new List<PlayerTitle>();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach(Player p in PlayerManager.Instance.Players)
		{
			var playerTitle = playerTitles.Find(o => o.Player == p);
			if(playerTitle != null)
			{
				UpdatePos(playerTitle);
			}
			else
			{
				PlayerTitle t = new PlayerTitle();
				t.Title = Instantiate(TitlePrefab);
				t.Title.rectTransform.SetParent(TitlePrefab.transform.parent);
				t.Title.rectTransform.localScale = Vector3.one;
				t.Title.rectTransform.localRotation = Quaternion.identity;
				t.Title.gameObject.SetActive(true);
				t.Title.text = p.PlayerId.ToString();
				t.Player = p;
				playerTitles.Add(t);
			}
		}
	}

	private void UpdatePos(PlayerTitle title)
	{
		title.Title.rectTransform.localPosition = WorldToUIPoint(Canvas, title.Player.transform, title.Title.rectTransform);
		//title.Title.rectTransform.localPosition = WordToScenePoint(Canvas, title.Player.transform.position);
		//title.Title.rectTransform.localPosition = Camera.main.WorldToScreenPoint(title.Player.transform.position);
	}

	public static Vector3 WorldToUIPoint(Canvas canvas, Transform worldGo, RectTransform transform)
	{

		Vector3 v_v3 = Camera.main.WorldToScreenPoint(worldGo.position);
		//Vector2 p;

		//RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, v_v3, Camera.main, out p);
		v_v3.x = canvas.pixelRect.width / Screen.width * (v_v3.x - Screen.width * 0.5f);
		v_v3.y = canvas.pixelRect.height / Screen.height * (v_v3.y - Screen.height * 0.5f);
		return v_v3;


		Vector3 v_ui = canvas.worldCamera.ScreenToWorldPoint(v_v3);
		Vector3 v_new = new Vector3(v_ui.x, v_ui.y, canvas.GetComponent<RectTransform>().anchoredPosition3D.z);
		return v_new;
	}

	public static Vector3 WordToScenePoint(Canvas canvas, Vector3 pos)
	{
		var screenPos =Camera.main.WorldToScreenPoint(pos);
		CanvasScaler canvasScaler = canvas.gameObject.GetComponent<CanvasScaler>();

		float resolutionX = canvasScaler.referenceResolution.x;
		float resolutionY = canvasScaler.referenceResolution.y;

		screenPos.x += resolutionX * 0.5f;
		screenPos.y += resolutionY * 0.5f;


		return screenPos;
	}
}
