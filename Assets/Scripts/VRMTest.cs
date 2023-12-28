using UnityEngine;
using VRM;
using UniGLTF;
using System.IO;
using VRMShaders;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace VRMTest
{
public class VRMTest : MonoBehaviour
    {
        [SerializeField]
        private RuntimeAnimatorController m_animCtrl;
        private string filePath;
        private GameObject vrmObj;
        private int count = 0;

        void Start()
        {
            filePath = Path.Combine(Application.streamingAssetsPath, "1903884660012638236.vrm");
            StartCoroutine(AssetLoader());
        }

        private void fiftyTimesVRMRuntimeLoad() {
            if (count < 50)
            {
                StartCoroutine(AssetLoader());
                count++;
            }
        }

        IEnumerator AssetLoader()
        {
            bool isURI = Uri.IsWellFormedUriString(filePath, UriKind.Absolute);
            byte[] data = { };

            if (isURI)
            {
                var request = UnityWebRequest.Get(filePath);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    data = request.downloadHandler.data;
                    runtimeLoadVRMAsync(data, filePath);
                }
                else
                {
                    Debug.Log(request.error);
                    yield break;
                }
            }
            else
            {
                try
                {
                    data = File.ReadAllBytes(filePath);
                    runtimeLoadVRMAsync(data, filePath);
                }
                catch(Exception ex)
                {
                    Debug.Log(ex.Message);
                    yield break;
                }
            }
        }

        async void runtimeLoadVRMAsync(byte[] data, string path)
        {
            RuntimeGltfInstance instance = await VrmUtility.LoadBytesAsync(path, data, awaitCaller: (IAwaitCaller)new ImmediateCaller());
            instance.ShowMeshes();
            vrmObj = instance.gameObject;
            if (vrmObj != null)
            {
                var animCtrl = vrmObj.GetComponent<Animator>();
                if (animCtrl && animCtrl.runtimeAnimatorController == null)
                {
                    animCtrl.runtimeAnimatorController = m_animCtrl;
                }
                await Task.Delay(1000);
                DestroyModel();
            }
        }

        void DestroyModel()
        {
            Destroy(vrmObj);
            vrmObj = null;
            Debug.Log("DestroyModel");
        }

    }
}