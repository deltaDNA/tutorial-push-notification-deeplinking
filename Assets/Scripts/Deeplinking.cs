using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeltaDNA; 

namespace DeeplinkingTutorial
{
    public class Deeplinking : MonoBehaviour
    {
        public const string CLIENT_VERSION = "0.0.01";
		private int coinBalance ; //NB for simplicity, this tutorial doesn't bother to persist coinBalance across sessions.
		public UnityEngine.UI.Text txtCoinBalance ; 

        // Use this for initialization
        void Start()
        {
			///////////////////////////////////////////////////////////////////
			// Register for Push Notifications and specify callbacks
			DDNA.Instance.IosNotifications.OnDidRegisterForPushNotifications += (string n) => {
				Debug.Log("Got an iOS push token: " + n);
			};
			DDNA.Instance.IosNotifications.OnDidReceivePushNotification += (string n) => {
				Debug.Log("Got an iOS push notification! " + n);
				ProcessNotification(n);
			};
			DDNA.Instance.IosNotifications.OnDidLaunchWithPushNotification += (string n) => {
				Debug.Log("Launched with an iOS push notification: " + n);
				ProcessNotification(n);
			};
			DDNA.Instance.IosNotifications.RegisterForPushNotifications();


            ///////////////////////////////////////////////////////////////////
            // Start the deltaDNA SDK and use it to capture gameplay 
            
            
			// DDNA.Instance.ClearPersistentData(); //  uncomment to clear data and treat each run as if it were a new player.

            // Enter additional configuration here
            DDNA.Instance.ClientVersion = CLIENT_VERSION;
            DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
            DDNA.Instance.Settings.DebugMode = true;

            // Launch the SDK
            // This is currently connected to the DEMO game DEV environment            
            DDNA.Instance.StartSDK(
				"56922021622026022056772309414071",
                "https://collect2674dltcr.deltadna.net/collect/api",
                "https://engage2674dltcr.deltadna.net"
            );
            //////////////////////////////////////////////////////////////////
        }



		///////////////////////////////////////////////////////////////////////////////////////
		// The game has received a Push notifcation, look at the notifcation payload for promoID
		void ProcessNotification(string notification)
		{
			Debug.Log ("Notifcation String Received " + notification); 
			// Notification will look something like {"aps":{"alert":"test 01 17:37"},"_ddLaunch":true,"promoChannel":"PUSH","_ddCampaign":-1,"_ddCohort":0,"promoID":"001"}

			Dictionary<string,object> payload = DeltaDNA.MiniJSON.Json.Deserialize (notification) as Dictionary<string,object>;

			if (payload.ContainsKey ("promoID") && payload.ContainsKey ("promoChannel")) 
			{
				// Check Engage for campaigns using this for promoID for gifting or deeplinking
				DeeplinkCampaignCheck (payload["promoID"].ToString(), payload["promoChannel"].ToString());
			}
		}



		///////////////////////////////////////////////////////////////////////////////////////
		// Make an Engage In-Game Campaign request on the deeplink decision point 
		// This will reveal if there are any active rewards or deeplinks for this player.
		void DeeplinkCampaignCheck(string promoID, string promoChannel)
		{

			Debug.Log (string.Format ("Checking for PromoID : {0} PromoChannel : {1}",promoID,promoChannel));

			// Create Engage Request to deeplink decision point
			var engagement = new Engagement("deeplink")
				.AddParam("promoID", promoID)
				.AddParam("promoChannel", promoChannel);

			DDNA.Instance.RequestEngagement(engagement, (Dictionary<string, object> response) => {
				
				// Handle response from Engage campaign
				// This example just uses a GameParameter response, but you could use a PopupImage response instead

				// Parse game parameters from campaign response
				if (response!= null && response.ContainsKey("parameters"))
				{
					// Engage response contains some paramters for the game client. 
					object p;
					response.TryGetValue("parameters", out p);
					Dictionary<string,object> parameters = p as Dictionary<string,object>;

					if (parameters.ContainsKey("promoType"))
					{
						string promoType = parameters["promoType"].ToString();

						// Start creating an event that will report back to deltaDNA that player received a promotion.
						var promoReceivedEvent = new GameEvent("promoReceived")
							.AddParam("promoID", promoID)
							.AddParam("promoChannel",promoChannel)
							.AddParam("promoType", promoType);
						

						// Extract deeplink or reward values
						int coinRewardAmount = 0 ;
						string deeplinkDestination = null ; 
					
						if (parameters.ContainsKey("coinRewardAmount")) 
							coinRewardAmount = System.Convert.ToInt32(parameters["coinRewardAmount"]) ; 

						if (parameters.ContainsKey("deeplinkDestination"))
									deeplinkDestination = parameters["deeplinkDestination"].ToString();


						switch(promoType)
						{
							case "COINS" :
							 	if(coinRewardAmount > 0 )
								{
									DeliverCoins(coinRewardAmount);
									promoReceivedEvent.AddParam("coinRewardAmount",coinRewardAmount);
								}
								break;

							case "DEEPLINK" : 
								if(deeplinkDestination !=null)
								{
									NavigateTo(deeplinkDestination);
									promoReceivedEvent.AddParam("deeplinkDestination",deeplinkDestination);
								}
								break ;
							
							default:
								break ; 
						}

						// Finally record the promoReceivedEvent
						DDNA.Instance.RecordEvent(promoReceivedEvent);
					}
				}
			});
				
		}



		// Deliver Coin reward to player
		void DeliverCoins(int coinAmount)
		{
			coinBalance += coinAmount; 
			UpdateHUD ();
		}



		// Navigate the player to a specific game screen or location
		void NavigateTo(string destination)
		{
			// Insert your own internal game navigation code here. 
			// NB a list of destinations will need to be specified 
			// and signed off by Marketing and Development.
			Debug.Log(string.Format("Navigating To {0} ...",destination));
		}



		// Update all HUD displays
		void UpdateHUD()
		{
			txtCoinBalance.text = string.Format ("Coins : {0}", coinBalance);
		}



		// Test Coin Reward mechanism
		public void CoinTest()
		{

			DeliverCoins (100);
			Debug.Log ("Have some test coins");
			//var exp = GetComponent<ParticleSystem> ();
			//exp.Emit (500);
			//exp.Play ();

		}

		public void EngageTest()
		{
			Debug.Log ("Engage test");
			DeeplinkCampaignCheck ("test", "PUSH");
		}
    }
}