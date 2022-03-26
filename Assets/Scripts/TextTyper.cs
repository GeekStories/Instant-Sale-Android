using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Text))]
[RequireComponent (typeof (RectTransform))]

public class TextTyper : MonoBehaviour 
{	
	private float typeSpeed = 0.05f;
	private float startDelay = 0.5f;

	private int counter;
	private Text textComp;

    public string textToType;
    private bool typing;

    void Awake(){
		textComp = GetComponent<Text>();

		counter = 0;
		textToType = textComp.text;
		textComp.text = "";
	}

	public void StartTyping(){	
		if(!typing){
			InvokeRepeating("Type", startDelay, typeSpeed);
		}else{
			print(gameObject.name + " : Is already typing!");
		}
	}

	public void StopTyping(){
        CancelInvoke("Type");
        counter = 0;
        typing = false;
        textComp.text = "";
        textToType = "";
	}

	private void Type(){	
		typing = true;
		textComp.text = textComp.text + textToType[counter];
		counter++;

		if(counter == textToType.Length){	
			typing = false;
			CancelInvoke("Type");
		}
	}

    public bool IsTyping() { return typing; }
}
