using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class PlayerSettingsState {
     private float musicVolume;
     private float sfxVolume;
     private float gameSpeed;
     private bool fullScreen;
     private bool autoUpgrade;
     private bool showStarterDeckCardsInShop;
     private int renderScale;

     public PlayerSettingsState()
     {
          this.musicVolume = OptionsViewController.Instance.getMusicVolumeSliderValue();
          this.sfxVolume = OptionsViewController.Instance.getSFXVolumeSliderValue();
          this.gameSpeed = OptionsViewController.Instance.getGameSpeedSliderValue();
          this.fullScreen = OptionsViewController.Instance.getIsFullScreened();
          this.autoUpgrade = OptionsViewController.Instance.getIsAutoUpgradeEnabled();
          this.showStarterDeckCardsInShop = OptionsViewController.Instance.getShowStarterDeckCardsInShop();
          this.renderScale = OptionsViewController.Instance.getRenderScaleDropdown();
     }

     public void LoadPlayerSettings()
     {
          OptionsViewController.Instance.setMusicVolumeSliderValue(this.musicVolume);
          OptionsViewController.Instance.setSFXVolumeSliderValue(this.sfxVolume);
          OptionsViewController.Instance.setGameSpeedSliderValue(this.gameSpeed);
          OptionsViewController.Instance.setIsFullScreened(this.fullScreen);
          OptionsViewController.Instance.setIsAutoUpgradeEnabled(this.autoUpgrade);
          OptionsViewController.Instance.setShowStarterDeckCardsInShop(this.showStarterDeckCardsInShop);
          OptionsViewController.Instance.setRenderScaleDropdown(this.renderScale);
     }
}