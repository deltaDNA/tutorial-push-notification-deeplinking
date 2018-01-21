using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeltaDNA; 

namespace DeeplinkingTutorial
{
    public class Deeplinking : MonoBehaviour
    {
        public const string CLIENT_VERSION = "0.0.01";

        // Use this for initialization
        void Start()
        {
            ///////////////////////////////////////////////////////////////////
            // Start the deltaDNA SDK and use it to capture gameplay 
            
            // For demo purposes... uncomment the next line to clear all data and treat each run as if it were a fresh new install.
            // DDNA.Instance.ClearPersistentData(); 

            // Enter additional configuration here
            DDNA.Instance.ClientVersion = CLIENT_VERSION;
            DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
            DDNA.Instance.Settings.DebugMode = true;

            // Launch the SDK
            // This is currently connected to the DEMO game DEV environment            
            DDNA.Instance.StartSDK(
                "56919948607282167963952652014071",
                "https://collect2674dltcr.deltadna.net/collect/api",
                "https://engage2674dltcr.deltadna.net"
            );
            //////////////////////////////////////////////////////////////////
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}