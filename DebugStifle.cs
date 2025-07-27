
using KSP.UI.Screens.DebugToolbar;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebugStifle
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class DebugStifle : MonoBehaviour
    {
		private static bool loaded;
		private static bool processed;

		private static DebugScreenConsole console;

		private bool open;
        private static bool enableInput = false;
        readonly static string SettingsPath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/DebugStifle/PluginData/settings.cfg");

        private void Log(string message) => Debug.Log("[Debug_Stifler] " + message);

        internal static void TryReadValue<T>(ref T target, ConfigNode node, string name)
        {
            if (node.HasValue(name))
            {
                try
                {
                    target = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(node.GetValue(name));
                }
                catch
                {
                    // just skip over it
                }
            }
            // skip again
        }

        private static void SaveSettings()
        {
			ConfigNode settings = new ConfigNode("SETTINGS");

            Dictionary<string, object> settingValues = new Dictionary<string, object>
            {
                { "enableInput", enableInput },
            };

            foreach (KeyValuePair<string, object> kvp in settingValues) settings.AddValue(kvp.Key, kvp.Value);

            ConfigNode root = new ConfigNode();
            root.AddNode(settings);

            root.Save(SettingsPath); // this makes a new file if settings.cfg didnt exist already
        }

        private static void LoadSettings()
        {
            ConfigNode root = ConfigNode.Load(SettingsPath);
            if (root != null)
            {
                ConfigNode settings = root.GetNode("SETTINGS");
                if (settings != null)
                {
                    void Read<T>(ref T field, string name) => TryReadValue(ref field, settings, name);

                    Read(ref enableInput, "enableInput");
                }
            }
        }

        private void Awake()
		{
			if (loaded)
			{
				Destroy(gameObject);
				return;
			}

			loaded = true;

			DontDestroyOnLoad(gameObject);

            LoadSettings();
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

            if (console.inputField != null)
            {
                console.inputField.interactable = enableInput;
            }

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
            if (console?.inputField == null) return;

            if (console.inputField.interactable)
			{
				console.inputField.DeactivateInputField();
				console.inputField.interactable = false;
				enableInput = false;
			}
			else
			{
                console.inputField.interactable = true;
                enableInput = true;
            }

            SaveSettings(); // we have to do this here because every thing else gets destroyed the first time
        }
    }
}
