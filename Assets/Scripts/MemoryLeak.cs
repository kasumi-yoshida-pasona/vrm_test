using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace VRMTest
{
    public class MemoryLeak : MonoBehaviour
    {
        [SerializeField] private Text FpsText;
        [SerializeField] private Button ResetFps;
        float fps;
        float[] fpsList = new float[10];
        string text;

        private void Awake() {
            ResetFps.onClick.AddListener(OnClickReset);
        }

        private void OnClickReset() {
            fpsList = new float[10];
        }

        void Update()
        {
            text = null;
            fps = Time.deltaTime * 1000.0f;

            // 上位１０位以内に入ったらfpsListに入れる
            // 数値が大きい方が遅延が発生している状態であるため、最小値よりも大きい値であれば追加する
            if (fpsList.Length < 10 || fps > fpsList.Min())
            {
                // リストに新しいFPS値を追加
                List<float> tempList = fpsList.ToList();
                tempList.Add(fps);

                // リストを降順にソート（遅いフレームが上にくる）
                tempList.Sort((a, b) => b.CompareTo(a));

                // リストのサイズを10に保持
                if (tempList.Count > 10)
                {
                    tempList.RemoveAt(10);
                }

                // 配列を更新
                fpsList = tempList.ToArray();
            }


            for (int i = 0; i < 10; i++)
            {
                text = text + fpsList[i].ToString("0.00") + " ms\n";
            }

            FpsText.text = fps.ToString("0.00") + " ms\n" + text;

        }
    }
}