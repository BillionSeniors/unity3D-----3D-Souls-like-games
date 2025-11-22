using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QHZ
{
    public class TalkToNPCEnding : Interactable
    {
        public override void Interact(PlayerManager playerManager)
        {
            playerManager.uiManager.endingWindow.SetActive(true);
            playerManager.interactableUIGameObject.SetActive(false);
            playerManager.itemInteractableGameObject.SetActive(false);
        }
    }
}
