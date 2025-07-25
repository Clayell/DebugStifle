
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.Screens.DebugToolbar;
using TMPro;

namespace DebugStifle
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class DebugStifle : MonoBehaviour
    {
		private static bool loaded;
		private static bool processed;

		private static DebugScreenConsole console;

		private bool open;

		private void Log(string message) => Debug.Log("[Debug_Stifler] " + message);

        private void Awake()
		{
			if (loaded)
			{
				Destroy(gameObject);
				return;
			}

			loaded = true;

			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			if (processed)
				return;

			if (!open && Input.GetKeyDown(KeyCode.F12) && GameSettings.MODIFIER_KEY.GetKey(false))
			{
				open = true;

				StartCoroutine(WaitForDebug());
			}
		}

		private IEnumerator WaitForDebug()
		{
			int timer = 0;

			while (GameObject.FindObjectOfType<DebugScreen>() == null && timer < 20)
			{
				timer++;
				Log("Searching For Debug Panel...");
				yield return new WaitForSeconds(1);
			}

			yield return new WaitForSeconds(0.5f);

			AlterPrefab();
		}

		private void AlterPrefab()
		{
			Log("Altering Debug Panel...");

			DebugScreen screen = GameObject.FindObjectOfType<DebugScreen>();

			if (screen == null)
				return;

			console = screen.GetComponentInChildren<DebugScreenConsole>();

			if (console == null)
				return;

			Button toggle = GameObject.Instantiate<Button>(console.submitButton);

			toggle.transform.SetParent(console.submitButton.transform.parent, false);

			toggle.onClick.RemoveAllListeners();
			toggle.onClick.AddListener(ToggleInput);

			TextMeshProUGUI toggleText = toggle.GetComponentInChildren<TextMeshProUGUI>();

			if (toggleText == null)
				return;

			toggleText.text = "Toggle Input";

			console.submitButton.transform.SetAsLastSibling();

			LayoutElement layout = toggle.GetComponent<LayoutElement>();

			if (layout == null)
				return;

			layout.minWidth += 38;
			layout.preferredWidth += 38;

			processed = true;

			Log("Debug Panel Altered");

			Destroy(gameObject);		
		}

		private static void ToggleInput()
		{
			if (console.inputField.interactable)
			{
				console.inputField.DeactivateInputField();
				console.inputField.interactable = false;
			}
			else
				console.inputField.interactable = true;
		}
    }
}
