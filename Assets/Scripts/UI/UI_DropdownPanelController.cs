using UnityEngine;

namespace UI
{
    public class UI_DropdownPanelController : MonoBehaviour
    {
        public GameObject panelHit;
        public GameObject panelInteraction;
        public GameObject panelMovement;

        public void OnDropdownValueChange(int value)
        {
            switch (value)
            {
                case 0:
                {
                    panelHit.SetActive(false);
                    panelInteraction.SetActive(false);
                    panelMovement.SetActive(false);
                }
                    break;
                case 1:
                {
                    panelHit.SetActive(true);
                    panelInteraction.SetActive(false);
                    panelMovement.SetActive(false);
                }
                    break;
                case 2:
                {
                    panelHit.SetActive(false);
                    panelInteraction.SetActive(true);
                    panelMovement.SetActive(false);
                }
                    break;
                case 3:
                {
                    panelHit.SetActive(false);
                    panelInteraction.SetActive(false);
                    panelMovement.SetActive(true);
                }
                    break;
            }
        }
    }
}
