using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// Textを1文字ごとに出すコンポーネント.
/// </summary>
public class TextDisplayController : MonoBehaviour {

	/// <summary>Text表示領域.</summary>
	[SerializeField]
	private Text displayText;
	[SerializeField, Header("文字送り速度")]
	private double wordSpeed = 0.1;

	private string[] sentenseArray;
	/// <summary>次表示文字列.</summary>
	private string nextString;
	/// <summary>文字送り中かどうか.</summary>
	private bool isBusy = false;
	/// <summary>文章Index.</summary>
	private int index = -1;
	/// <summary>文字送りDisposer.</summary>
	private IDisposable disposer;

	/// <summary>終了フラグ.</summary>
	[HideInInspector]
	public bool IsEnd {
		get { return isEnd; }
	}
	private bool isEnd = false;
	/// <summary>文字送り全部終了のコールバック.</summary>
	private UnityAction endEvent;

	/// <summary>
	/// 初期化.
	/// </summary>
	/// <param name="arg">Argument.</param>
	public void Initialize (string str, UnityAction callback = null) {
		this.sentenseArray = new string[] { str };
		this.endEvent = callback;
		this.Next ();
	}

	/// <summary>
	/// 初期化.
	/// </summary>
	/// <param name="stringArray">String array.</param>
	public void Initialize (string[] stringArray, UnityAction callback = null) {
		this.sentenseArray = stringArray;
		this.endEvent = callback;
		this.Next ();
	}

	/// <summary>
	/// 文字送り開始.
	/// </summary>
	/// <param name="displayString">Display string.</param>
	public void StartDisplay (string displayString) {
		// 文字送り中にする.
		this.isBusy = true;
		// 表示文字を空にする.
		this.displayText.text = "";
		// 文字を分割する.
		char[] charArray = displayString.ToCharArray ();
		// 文字列を記憶.
		this.nextString = displayString;

		// 文字送りする.
		int cIndex = 0;
		this.disposer = Observable.EveryUpdate ()
			.ThrottleFirst (System.TimeSpan.FromSeconds (this.wordSpeed))
			.Where (_ => this.isBusy)
			.TakeWhile (_ => charArray.Length > cIndex)
			.Subscribe (_ => {
				this.displayText.text += charArray[cIndex];
				cIndex++;
			}, () => {
				this.isBusy = false;
			});
	}

	/// <summary>
	/// Windowを押されたとき.
	/// </summary>
	public void OnClickWindow () {
		// 文字送り中に押されたら全て出す.
		if (this.isBusy) {
			this.isBusy = false;
			this.displayText.text = this.nextString;
			this.disposer.Dispose ();
		} else {
			Debug.Log ("次だぜ");
			this.Next ();
		}
	}

	/// <summary>
	/// 次の文に進む.
	/// </summary>
	private void Next () {
		this.index++;
		if (this.index < this.sentenseArray.Length) {
			this.StartDisplay (this.sentenseArray [this.index]);
		} else {
			this.End ();
		}
	}

	/// <summary>
	/// 前の文に進む.
	/// </summary>
	private void Prev () {
		this.index--;
		if (this.index < 0) {
			this.index = 0;
		}
		this.StartDisplay (this.sentenseArray [this.index]);
	}

	/// <summary>
	/// 全てをSkipする.
	/// </summary>
	private void Skip () {
		if (this.disposer != null) {
			this.disposer.Dispose ();
		}
		this.End ();
	}

	/// <summary>
	/// 終了.
	/// </summary>
	private void End () {
		Debug.Log ("終わり");
		this.isEnd = true;
		if (endEvent != null) {
			endEvent.Invoke ();
		}
	}
}