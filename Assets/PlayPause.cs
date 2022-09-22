using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using UnityEngine.XR;

public class PlayPause : MonoBehaviour
{
    AudioSource[] audioSources;
 
    // eyes closed detection timer
    public float blinkTime = 0.1f;
    float timer = 0.0f;
    bool repeatTriggerUp = false;
    bool repeatTriggerDown = true;
    bool eyesOpen = true;

    // audio volume envelope coroutine
    float timeElapsed;
    float lerpDuration = 0.1f;
    float valueToLerp;
    bool isRunning = false;
    bool hmdPresent;
    bool hmdMemory = false; // used to remember whether eyes were open or closed when HMD was taken off

    // Start is called before the first frame update
    void Start()
    {  
    audioSources = GetComponents<AudioSource>();
    OutputPause();     // set volume to 0
    }

    // Update is called once per frame
    private void Update ()
{    
    // Get eye tracking data in world space
    var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
    // For social use cases, data in local space may be easier to work with
    var eyeTrackingDataLocal = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

    // The EyeBlinking bool is true when the eye is closed
    var isLeftEyeBlinking = eyeTrackingDataLocal.IsLeftEyeBlinking;
    var isRightEyeBlinking = eyeTrackingDataLocal.IsRightEyeBlinking;
    
    // check whether HMD is active 
    InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.userPresence, out hmdPresent);

    // main eye detection section
   if (hmdPresent){ 
        if (isLeftEyeBlinking == true && isRightEyeBlinking == true){
            timer += Time.deltaTime;
            
            if (timer > blinkTime){
                
                if (!isRunning && !repeatTriggerUp){
                    OutputPlay();
                    repeatTriggerUp = true;
                    }   
                repeatTriggerDown = false;
            }
        } else
        {
            timer = 0.0f;
            if (!isRunning && !repeatTriggerDown){
                OutputPause();
                repeatTriggerDown = true;
            }
            repeatTriggerUp = false;        
        
        }

   }else{
    OutputPause();
   } 

}

// management of audio sources (all audiosources connected to gameObject will be controlled)
private void OutputPause(){
    
    // adjust all audio sources attached to gameobject 

    for (int i = 0; i < audioSources.Length; i++) {
       audioSources[i].Pause();
         }
    
     // transform.GetChild[i]gameObject.SetActive(true); // alternatively (dis)activate child objects  

}

private void OutputPlay(){
    
    // adjust all audio sources attached to gameobject 

    for (int i = 0; i < audioSources.Length; i++) {
       audioSources[i].Play();
         }
    
     // transform.GetChild[i]gameObject.SetActive(true); // alternatively (dis)activate child objects  

}

// coroutine to manage audio envelopes
IEnumerator Lerp(float startVolume, float endVolume)
    {
        isRunning = true;

        float timeElapsed = 0;
        while (timeElapsed < lerpDuration)
        {
            valueToLerp = Mathf.Lerp(startVolume, endVolume, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            //outputManager(valueToLerp);
            yield return null;
        }
        valueToLerp = endVolume;
        //outputManager(valueToLerp);
        isRunning = false;
    }
}
