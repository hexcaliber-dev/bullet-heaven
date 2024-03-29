﻿using System.Collections;
using System.Collections.Generic;
using PubNubAPI;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
public class MyClass {
    public string username;
    public string score;
    public string time;
    public string test;
}
public class PubNubConnection : MonoBehaviour {
    public static PubNub pubnub;
    public Text[] names;
    public Text[] scores;

    public Text[] times;
    public Button SubmitButton;
    public InputField FieldUsername;

    public GameObject HighScoreFields;
    // Use this for initialization
    void Start () {
        Button btn = SubmitButton.GetComponent<Button> ();
        btn.onClick.AddListener (TaskOnClick);
        // Use this for initialization
        PNConfiguration pnConfiguration = new PNConfiguration ();
        pnConfiguration.PublishKey = "pub-c-36e8fff3-a450-4b43-98be-5a14f10c69b4";
        pnConfiguration.SubscribeKey = "sub-c-68d3c0d8-9b58-11e9-9ac8-0ed882abeb26";
        pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
        pnConfiguration.UUID = Random.Range (0f, 999999f).ToString ();
        pubnub = new PubNub (pnConfiguration);
        Debug.Log (pnConfiguration.UUID);

        MyClass myFireObject = new MyClass ();
        myFireObject.test = "new user";
        string fireobject = JsonUtility.ToJson (myFireObject);
        pubnub.Fire ()
            .Channel ("my_channel")
            .Message (fireobject)
            .Async ((result, status) => {
                if (status.Error) {
                    Debug.Log (status.Error);
                    Debug.Log (status.ErrorData.Info);
                } else {
                    Debug.Log (string.Format ("Fire Timetoken: {0}", result.Timetoken));
                }
            });

        pubnub.SusbcribeCallback += (sender, e) => {
            SusbcribeEventEventArgs mea = e as SusbcribeEventEventArgs;
            if (mea.Status != null) { }
            if (mea.MessageResult != null) {
                Dictionary<string, object> msg = mea.MessageResult.Payload as Dictionary<string, object>;

                string[] strArr = msg["username"] as string[];
                string[] strScores = msg["score"] as string[];
                string[] strTimes = msg["time"] as string[];

                for (int i = 0; i < names.Length; i++) {
                    names[i].text = strArr[i];
                    scores[i].text = strScores[i];
                    times[i].text = strTimes[i];
                }

                // Hide submit stuff if score not high enough
                if (ScoreCounter.Score <= int.Parse (strScores[4]))
                    HighScoreFields.SetActive (false);

            }
            if (mea.PresenceEventResult != null) {
                Debug.Log ("In Example, SusbcribeCallback in presence" + mea.PresenceEventResult.Channel + mea.PresenceEventResult.Occupancy + mea.PresenceEventResult.Event);
            }
        };
        pubnub.Subscribe ()
            .Channels (new List<string> () {
                "my_channel2"
            })
            .WithPresence ()
            .Execute ();
    }
    void TaskOnClick () {
        var usernametext = FieldUsername.text; // this would be set somewhere else in the code
        var scoretext = "" + ScoreCounter.Score;
        MyClass myObject = new MyClass ();
        myObject.username = FieldUsername.text;
        myObject.score = "" + ScoreCounter.Score;

        string minutes = Mathf.Floor (ScoreCounter.TimeElapsed / 60).ToString ("00");
        string seconds = (ScoreCounter.TimeElapsed % 60).ToString ("00");

        myObject.time = minutes + ":" + seconds;

        string json = JsonUtility.ToJson (myObject);
        pubnub.Publish ()
            .Channel ("my_channel")
            .Message (json)
            .Async ((result, status) => {
                if (!status.Error) {
                    Debug.Log (string.Format ("Publish Timetoken: {0}", result.Timetoken));
                    HighScoreFields.SetActive (false);
                } else {
                    Debug.Log (status.Error);
                    Debug.Log (status.ErrorData.Info);
                }
            });
        //Output this to console when the Button is clicked
        Debug.Log ("You have clicked the button!");
    }
}