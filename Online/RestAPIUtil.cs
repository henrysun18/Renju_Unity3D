using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace Assets.Scripts.Online
{
    public class RestAPIUtil
    {
        public static IEnumerator<UnityWebRequestAsyncOperation> GetRequest(string url, Action<string> callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                // https://forum.unity.com/threads/unitywebrequest-report-an-error-ssl-ca-certificate-error.617521/
                var cert = new AcceptAllCertificates();
                www.certificateHandler = cert;
                yield return www.SendWebRequest();
                //MAJOR KEY, otherwise anything written here assumes www is null
                //(spent hrs debugging this then realized I stumbled across this before)
                if (www.isDone)
                {
                    // passes in the JSON response to the provided handler
                    callback(www.downloadHandler.text);
                }
            }
        }
    }
}