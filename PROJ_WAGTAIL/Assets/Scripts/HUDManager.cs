using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    static private HUDManager instance;
    static public HUDManager Instance { get { return instance; } }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    [SerializeField] private GameObject CaptureModeHUD;

    public void SetCapturemodHUD(bool value)
    {
        CaptureModeHUD.SetActive(value);
    }
}
