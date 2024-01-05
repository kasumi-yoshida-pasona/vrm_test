using UnityEngine;
using VRM;
using UniGLTF;
using System.IO;
using VRMShaders;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace VRMTest
{
public class VRMTest : MonoBehaviour
    {
        [SerializeField]
        private RuntimeAnimatorController m_animCtrl;
        private string filePath;
        [SerializeField] private Button OneTimeButton;
        [SerializeField] private Button FiftyTimesButton;
        [SerializeField] private Button FiveHundredsTimesButton;

        void Start()
        {
            filePath = Path.Combine(Application.streamingAssetsPath, "1903884660012638236.vrm");
            OneTimeButton.onClick.AddListener(OnClickOneTime);
            FiftyTimesButton.onClick.AddListener(OnClickFiftyTimes);
            FiveHundredsTimesButton.onClick.AddListener(OnClick500Times);
        }
        void disableBtns()
        {
            OneTimeButton.interactable = false;
            FiftyTimesButton.interactable = false;
            FiveHundredsTimesButton.interactable = false;
        }

        void ableBtns()
        {
            OneTimeButton.interactable = true;
            FiftyTimesButton.interactable = true;
            FiveHundredsTimesButton.interactable = true;
        }

        async void OnClickOneTime()
        {
            disableBtns();
            await AssetLoader(1);
            ableBtns();
        }

        async void OnClickFiftyTimes()
        {
            disableBtns();
            await AssetLoader(50);
            ableBtns();
        }
        async void OnClick500Times()
        {
            disableBtns();
            await AssetLoaderWithoutDelete(50);
            ableBtns();
        }

        // 指定回数分ランタイムロードしてGameObjectのVRMを返すX10
        async Task AssetLoaderWithoutDelete(int loopNum)
        {
            bool isURI = Uri.IsWellFormedUriString(filePath, UriKind.Absolute);
            byte[] data = { };

            for (int n = 0; n < 10; n++)
            {
                GameObject[] VRMList = new GameObject[loopNum];
                for (int i = 0; i < loopNum; i++)
                {
                    if (isURI)
                    {
                        var request = UnityWebRequest.Get(filePath);
                        var requestAwaiter = request.SendWebRequest();

                        while (!requestAwaiter.isDone)
                        {
                            await Task.Delay(10);
                        }

                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            data = request.downloadHandler.data;
                            GameObject vrmObj = await runtimeLoadVRMAsync(data, filePath);
                            VRMList[i] = vrmObj;
                        }
                        else
                        {
                            Debug.Log(request.error);
                        }
                    }
                    else
                    {
                        try
                        {
                            data = await File.ReadAllBytesAsync(filePath);
                            GameObject vrmObj = await runtimeLoadVRMAsync(data, filePath);
                            VRMList[i] = vrmObj;
                        }
                        catch(Exception ex)
                        {
                            Debug.Log(ex.Message);
                        }
                    }
                }
                for (int i = 0; i < loopNum; i++)
                {
                    deleteVRMAsync(VRMList[i]);
                }
            }

        }


        // 指定回数分ランタイムロードして破棄する
        async Task AssetLoader(int loopNum)
        {
            bool isURI = Uri.IsWellFormedUriString(filePath, UriKind.Absolute);
            byte[] data = { };

            for (int i = 0; i < loopNum; i++)
            {
                if (isURI)
                {
                    var request = UnityWebRequest.Get(filePath);
                    var requestAwaiter = request.SendWebRequest();

                    while (!requestAwaiter.isDone)
                    {
                        await Task.Delay(10);
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        data = request.downloadHandler.data;
                        GameObject vrmObj = await runtimeLoadVRMAsync(data, filePath);
                        deleteVRMAsync(vrmObj);
                    }
                    else
                    {
                        Debug.Log(request.error);
                    }
                }
                else
                {
                    try
                    {
                        data = await File.ReadAllBytesAsync(filePath);
                        GameObject vrmObj = await runtimeLoadVRMAsync(data, filePath);
                        deleteVRMAsync(vrmObj);
                    }
                    catch(Exception ex)
                    {
                        Debug.Log(ex.Message);
                    }
                }
            }
        }

        async Task<GameObject> runtimeLoadVRMAsync(byte[] data, string filePath)
        {
            RuntimeGltfInstance instance = await VrmUtility.LoadBytesAsync(filePath, data, new RuntimeOnlyAwaitCaller());
            // ↓はWebGLで並列処理を避けるためにImmediate
            // RuntimeGltfInstance instance = await VrmUtility.LoadBytesAsync(filePath, data, awaitCaller: (IAwaitCaller)new ImmediateCaller());
            instance.ShowMeshes();
            GameObject vrmObj = instance.gameObject;
            if (vrmObj != null)
            {
                var animCtrl = vrmObj.GetComponent<Animator>();
                if (animCtrl && animCtrl.runtimeAnimatorController == null)
                {
                    animCtrl.runtimeAnimatorController = m_animCtrl;
                }
                await Task.Delay(100);
            }
            return vrmObj;
        }

        void deleteVRMAsync(GameObject vrmObj)
        {
            
                // モデルの削除
                Destroy(vrmObj);
                vrmObj = null;
        }

    }
}