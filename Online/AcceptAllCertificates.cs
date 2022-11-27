using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Online
{
    public class AcceptAllCertificates : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}