using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AiManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown whiteDropDown;
    public TMPro.TMP_Dropdown blackDropDown;
    public bool isBlackStockfish;
    public bool isWhiteStockfish;
    public huggingFaceStock stocky;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void WhiteDropDown(){
        if (whiteDropDown.value == 0){
            isWhiteStockfish = false;
        } else {
            isWhiteStockfish =true;
        }
    } 
     public void BlackDropDown(){
        if(blackDropDown.value == 0){
            isBlackStockfish = false;
        } else{
            isBlackStockfish = true;
        }
    } 
}
