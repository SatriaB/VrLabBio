using System;
using System.Collections.Generic;
using UnityEngine;

namespace FatahDev
{
    public enum PanelId { Login, MainMenu, Settings, Tutorial, Hud }

    [Serializable]
    public class PanelEntry
    {
        public PanelId id;
        public GameObject root;
    }

    public class PanelRouter : MonoBehaviour
    {
        [SerializeField] private List<PanelEntry> panels = new();
        public PanelId Current { get; private set; }

        private void Awake()
        {
            // Pastikan minimal satu panel aktif (MainMenu) saat start
            Show(PanelId.MainMenu);
        }

        public void Show(PanelId id)
        {
            foreach (var p in panels) if (p.root) p.root.SetActive(p.id == id);
            Current = id;
        }
        
        public void ShowLogin() => Show(PanelId.Login);
        public void ShowMainMenu() => Show(PanelId.MainMenu);
        public void ShowSettings() => Show(PanelId.Settings);
        public void ShowTutorial() => Show(PanelId.Tutorial);
        public void ShowHud()      => Show(PanelId.Hud);
    }
}