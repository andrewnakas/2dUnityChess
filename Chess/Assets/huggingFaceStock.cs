using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class huggingFaceStock : MonoBehaviour
{
        private const string URL = "https://nakas-stockfish-board-eval.hf.space/run/predict";
        public string stockData;

    void Start()
    {
        //StartCoroutine(ProcessRequest(URL,"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
    }

    public void FenToStock( string fen){
        StartCoroutine(ProcessRequest(URL,fen));

    }

    private IEnumerator ProcessRequest(string uri,string fen)
    {   ChessFen f = new ChessFen { data= new List<string>(){fen}};
        string postData =JsonUtility.ToJson(f);
        Debug.Log (postData);
        //byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postData);
        UnityWebRequest request = UnityWebRequest.Put(uri , postData);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("Content-Type", "application/json");
           //{"data": ["rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"]}
            //Content-Type: application/json' -d '{"data": ["rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"]}
            yield return request.SendWebRequest();


            if (request.isNetworkError)
            {
                Debug.Log(request.error);
            }
            else
            {
               stockData = request.downloadHandler.text;
               Debug.Log (stockData);
            }
    request.Dispose();   
    }
}

public class ChessFen {
    public List<string> data;
   

}
