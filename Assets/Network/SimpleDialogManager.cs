using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class SimpleDialogManager : MonoBehaviour {
	public string sceneName;
	public float transitionLength;
	public scene currentScene;
	public int eventIndex = 0;
	string response;
	public bool isSpeaking = false;
	public bool isListening = false;
	public bool isWaiting = false;
    public bool playOnAwake = false;

    public AMQ_Connection Connection;

    public Agent[] agents;
    public static string targetagent;
    // Use this for initialization

    // Memory list. NOT a MonoBehavior.
    public List<Memory> memories = new List<Memory>();

    void Start () {

        //Construct an instance of the XmlSerializer with the type
        // of object that is being deserialized.
        XmlSerializer seriealizer =
            new XmlSerializer(typeof(scene));
        // To read the file, create a FileStream.
        FileStream stream =
            new FileStream(sceneName + ".xml", FileMode.Open);
        // Call the Deserialize method and cast to the object type.
        currentScene = (scene)seriealizer.Deserialize(stream);

        if (playOnAwake)
        {
            //Load();
            StartCoroutine("RunGame");
        }
    }


    // Update is called once per frame
    void Update () {

	}

    /// <summary>
    /// Added by Ivan
    /// August 22 2017
    /// </summary>
    /// <param name="eventID">Event ID from the XML</param>
    /// <param name="type">Type of event memorized (i.e. dialog, response, animation, trigger, etc.)</param>
    void Memorize(string eventID, string type)
    {
        Memory tempMemory = new Memory()
        {
            eventMemorized = eventID,
            memorized = true,
            checkedMemory = false,
            type = type
        };
        memories.Add(tempMemory);
        //Debug.Log("Memorizing" + eventID + type);
    }

    /// <summary>
    /// Added by Ivan
    /// August 22 2017
    /// Checks if a memory is recorded. If so, returns true and sets the flag of checked memory to true.
    /// This flag is used to know which memories you have recalled in the past.
    /// </summary>
    /// <param name="eventID">Event ID to search for within memory</param>
    /// <returns></returns>
    bool CheckMemory(string eventID)
    {
        foreach (Memory m in memories)
        {
            if (m.eventMemorized == eventID)
            {
                m.checkedMemory = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Added by Ivan
    /// August 22 2017
    /// Prints memories by pressing M. Call of this function is on Update()
    /// </summary>
    void ShowMemories()
    {
        foreach (Memory m in memories)
        {
            Debug.Log("------------MEMORY------------");
            Debug.Log("Memory event: " + m.eventMemorized);
            Debug.Log("Memory recorded: " + m.memorized);
            Debug.Log("Memory checked: " + m.checkedMemory);
            Debug.Log("Memory type: " + m.type);
            Debug.Log("------------MEMORY------------");
        }
    }

    void sendGrammar(string[] choices){
		string msgAcum = "";
		foreach(string s in choices){
			msgAcum += s + ";";
		}

		Connection.SendMessage(msgAcum);
	}

	public void setResponse(string str){
		response = str;
		isListening = false;
	}

	IEnumerator RunGame(){
		while(eventIndex<currentScene.@event.Length){
			Debug.Log (currentScene.@event[eventIndex].GetType().ToString());

            //Play dialogs
            if (currentScene.@event[eventIndex].dialog != null)
            {
                Debug.Log("Playing...");
                while (isSpeaking)
                {
                    yield return null;
                }
                isSpeaking = true;

                targetagent = currentScene.@event[eventIndex].agent;
                SpeechControllerOVRLP thisAgent;
                if (targetagent == null)
                {
                    thisAgent = agents[0].speech;
                }
                else
                {
                    thisAgent = Array.Find(agents, SameName).speech;
                }

                thisAgent.Speak(currentScene.@event[eventIndex].id);
                Memorize(currentScene.@event[eventIndex].id, "dialog");
                SetNextEventIndex(currentScene.@event[eventIndex].jump_id);

                while (isSpeaking)
                {
                    yield return null;
                }
                continue;
            }

            //Play animations
            if (currentScene.@event[eventIndex].animation != null)
            {
                while (isWaiting)
                {
                    yield return null;
                }

                targetagent = currentScene.@event[eventIndex].agent;
                AnimationManager thisAgent;
                if (targetagent == null)
                {
                    thisAgent = agents[0].anim;
                }
                else
                {
                    thisAgent = Array.Find(agents, SameName).anim;
                }
                //Make sure to set the HasExitTime variable on the transitions to false;
                thisAgent.Play(currentScene.@event[eventIndex].animation.Value,
                               currentScene.@event[eventIndex].animation.length);
                Memorize(currentScene.@event[eventIndex].id, "animation");
                SetNextEventIndex(currentScene.@event[eventIndex].jump_id);
                continue;
            }

            //Recognize responses
            if (currentScene.@event[eventIndex].response!=null){
                Memorize(currentScene.@event[eventIndex].id, "response");
                while (isSpeaking){
					yield return null;
				}
				List<string> grammars = new List<string>();
				Dictionary<string,List<string>> grammarDictionary = new Dictionary<string, List<string>>();
				foreach(sceneEventResponseGrammar g in currentScene.@event[eventIndex].response.grammar)
                {
					grammarDictionary.Add(g.jump_id,new List<string>());
					foreach(string s in g.item){
						grammarDictionary[g.jump_id].Add(s);
						grammars.Add(s);
					}
				}
				sendGrammar(grammars.ToArray());
				isListening=true;
				while(isListening){
					yield return null;
				}

				string jump = null;
				foreach(string key in grammarDictionary.Keys){
					if(grammarDictionary[key].Contains(response)){
						jump = key;
					}
				}

				int i = 0;
				foreach(sceneEvent e in currentScene.@event){
					if(e.id.Equals(jump)){
						eventIndex = i;
						break;
					}
					i++;
				}
				continue;
			}

			//Activate trigger
			if(currentScene.@event[eventIndex].trigger!=null){
                Memorize(currentScene.@event[eventIndex].id, "trigger");
                sceneEventTrigger trigger = currentScene.@event[eventIndex].trigger;
				if(trigger.Value!=null){
					GameObject.Find(trigger.@object).SendMessage(trigger.method, trigger.Value);
				}
				else{
					GameObject.Find(trigger.@object).SendMessage(trigger.method);
				}
				SetNextEventIndex(currentScene.@event[eventIndex].jump_id);
				continue;
			}

            //Wait
            if (currentScene.@event[eventIndex].wait != null)
            {
                float timeOut = currentScene.@event[eventIndex].wait.time_out;
                if (timeOut > 0)
                {
                    Invoke("StopWaiting", timeOut);
                }
                isWaiting = true;
                while (isWaiting)
                {
                    yield return null;
                }
                SetNextEventIndex(currentScene.@event[eventIndex].jump_id);
                continue;
            }
            yield return null;

            //Memorize
            if (currentScene.@event[eventIndex].memory != null)
            {
                if (CheckMemory(currentScene.@event[eventIndex].memory.Value))
                {
                    Debug.Log("Yes I remember!");
                    SetNextEventIndex(currentScene.@event[eventIndex].memory.remembered);
                }

                else
                {
                    Debug.Log("No I dont remember!");
                    SetNextEventIndex(currentScene.@event[eventIndex].memory.notRemembered);
                }
                continue;
            }
            yield return null;
        }

		if(currentScene.nextScene.Equals("none")){
			Application.Quit();
		}
		else{
			Connection.KillConnection();
			//Application.LoadLevel(currentScene.nextScene);
		}
	}

	public void StopWaiting(){
		isWaiting = false;
	}

	void SetNextEventIndex(string jump_id){
		if(jump_id!=null && !jump_id.Equals("none")){
			int i = 0;
			foreach(sceneEvent e in currentScene.@event){
				if(e.id.Equals(jump_id)){
					eventIndex = i;
					break;
				}
				i++;
			}
		}
		else{
			eventIndex++;
		}
	}

    // Weird method to find an element in the array. Comment with a reference of how it works.
    private static bool SameName(Agent agent)
    {
        return agent.name == targetagent;
    }

    void SceneChange()
    {
        //Save();
        SceneManager.LoadScene(currentScene.nextScene);
    }

    /// <summary>
    /// Added by Ivan
    /// August 22 2017
    /// Save the memories. Can also be used to continue an interrupted experience.
    /// You can change the name on the inspector by making the save file name public
    /// to save different sessions.
    /// </summary>
    void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/memory.dat");

        PlayerData data = new PlayerData();
        data.memories = memories;

        bf.Serialize(file, data);
        file.Close();
    }

    /// <summary>
    /// Added by Ivan
    /// August 22 2017
    /// Load the memories. Can also be used to continue an interrupted experience.
    /// </summary>
    void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/memory.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/memory.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();
            memories = data.memories;
        }
    }
}

[Serializable]
class PlayerData
{
    public List<Memory> memories;
}
