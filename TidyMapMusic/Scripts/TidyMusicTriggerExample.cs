using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TidyMusicTriggerExample : MonoBehaviour
{
    public string uniqueId;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerTag")
            MultiplayerARPG.TidyAudioManager.Singleton.PlayMapMusic(uniqueId);
    }
}
