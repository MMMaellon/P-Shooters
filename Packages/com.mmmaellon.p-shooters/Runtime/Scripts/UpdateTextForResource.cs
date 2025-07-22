
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class UpdateTextForResource : ResourceListener
    {
        public string textToReplaceWithPlayerName = "<player>";
        public string textToReplaceWithResourceName = "<resource>";
        public string textToReplaceWithResourceValue = "<value>";
        [TextArea]
        public string textFormat = "<value>";
        public TMPro.TextMeshProUGUI text;
        public ResourceManager specificResource;
        public Player specificPlayer;
        public bool LocalPlayerOnly = true;

        public void Start()
        {
            //if we want our thingy to be specific to a resource, we assign this listener to that resource on start
            if (specificResource == null)
            {
                return;
            }
            for (int i = 0; i < specificResource.listeners.Length; i++)
            {
                if (specificResource.listeners[i] == this)
                {
                    return;
                }
            }
            var newList = new ResourceListener[specificResource.listeners.Length + 1];
            System.Array.Copy(specificResource.listeners, newList, specificResource.listeners.Length);
            newList[newList.Length - 1] = this;
            specificResource.listeners = newList;
            Debug.LogWarning("OOOOOOWEEE");
        }

        public override void OnChange(Player player, ResourceManager resource, int oldValue, int newValue)
        {
            if (specificResource != null && specificResource != resource)
            {
                return;
            }

            if (specificPlayer != null && specificPlayer != player)
            {
                return;
            }

            if (LocalPlayerOnly && !player.IsOwnerLocal())
            {
                return;
            }

            text.text = textFormat.Replace(textToReplaceWithPlayerName, player.Owner.displayName).Replace(textToReplaceWithResourceName, resource.resourceName).Replace(textToReplaceWithResourceValue, newValue.ToString());
        }
    }
}
