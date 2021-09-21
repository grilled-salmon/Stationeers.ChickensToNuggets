using ChickensToNuggets.Scripts;
using Stationeers.Addons;
using UnityEngine;

namespace ChickensToNuggets.Scripts
{
    public class ChickensToNuggets : IPlugin
    {
        public void OnLoad()
        {
            Debug.Log(ModReference.Name + ": Loaded");
        }

        public void OnUnload()
        {
            Debug.Log(ModReference.Name + ": Unloaded");
        }
    }
}
