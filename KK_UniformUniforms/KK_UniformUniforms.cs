using BepInEx;
using Harmony;
using KKAPI;
using KKAPI.Maker;
using UniRx;
using UnityEngine;
using Illusion.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BepInEx.Logging;
using Logger = BepInEx.Logger;
using ExtensibleSaveFormat;
using KKABMX.Core;

namespace KK_UniformUniforms
{
    //[BepInDependency(KoikatuAPI.GUID)]
    //[BepInDependency(KKABMX_Core.GUID)]
    //[BepInDependency(ExtendedSave.GUID)]
    [BepInPlugin(GUID, PluginName, Version)]
    public class KK_UniformUniforms : BaseUnityPlugin
    {
        public const string GUID = "com.cptgrey.bepinex.uniform";
        public const string PluginName = "KK Uniform Uniforms";
        public const string PluginNameInternal = "KK_UniformUniforms";
        public const string Version = "0.1";

        public enum Clothes
        {
            top,
            bottom,
            bra,
            panties,
            gloves,
            pantyhose,
            legwear,
            inshoe,
            outshoe,
            subshirt = 0,
            subjacketcollar = 1,
            subdecoration = 2,
        }


        private static List<int> TopList = new List<int> { 1, 2 };
        private static List<int> BottomList = new List<int> { 4, 5, 6, 7, 8 };
        private static List<int> PantyhoseList = new List<int> { };
        private static List<int> GlovesList = new List<int> { };
        private static List<int> LegwearList = new List<int> { };
        private static List<int> InshoeList = new List<int> { };
        private static List<int> OutshoeList = new List<int> { };

        private static List<int> SubshirtList = new List<int> { 0, 1, 2, 3, 4, 6, 8};
        private static List<int> SubsailorList = new List<int> { 0, 1, 2, 3, 4, 5};
        private static List<int> SubjacketList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        private static List<int> SubcollarList = new List<int> { 0, 1, 2, 3, 4, 5 };
        private static List<int> SubblazerdecoList = new List<int> { 0, 1, 2, 3, 4, 5};
        private static List<int> SubsailordecoList = new List<int> { 0, 1, 2, 3, 4, 5, 6};


        [DisplayName("ColorPicker")]
        public static SavedKeyboardShortcut ColorPicker { get; private set; }
        private static Rect _guiRect { get; set; }
        private static int _emblemID { get; set; }
        private static int _guiCurrClothing { get; set; }
        private static int _guiCurrIndex { get; set; }
        private static bool _guiHSV { get; set; }
        private static Color _copy { get; set; }
        private static bool _guiVisible { get; set; }
        private static List<SaveData.CharaData> _charList { get; set; }
        private static ActionGame.ClassRoomSelectScene _classroomScene { get; set; }

        public static Dictionary<int, Color[]> _colorDict { get; set; }
        public static bool IsInClassRoom { get; private set; }
        
        void Start()
        {
            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(typeof(KK_UniformUniforms));

            _guiVisible = false;
            _guiCurrClothing = (int)Clothes.top;
            _guiCurrIndex = 0;
            _guiHSV = false;
            _charList = new List<SaveData.CharaData>();
            _colorDict = new Dictionary<int, Color[]>();
            _copy = new Color(1, 1, 1);
            _colorDict.Add((int)Clothes.top, new Color[]
            {
                new Color(1,1,1),
                new Color(1,0,0),
                new Color(1,0,1),
                new Color(1,0,0),
                new Color(0,1,1)
            });
            _colorDict.Add((int)Clothes.bottom, new Color[]
            {
                new Color(1,1,1),
                new Color(1,0,0),
                new Color(1,0,1),
                new Color(1,0,0),
                new Color(0,1,1)
            });
        }

        void Update()
        {
            _classroomScene = Singleton<ActionGame.ClassRoomSelectScene>.Instance;
            if (_classroomScene != null) IsInClassRoom = true;
            if (IsInClassRoom)
            {
                _emblemID = Singleton<Manager.Game>.Instance.saveData.emblemID;
                _guiVisible = true;
            }
            else _guiVisible = true;
        }

        private void OnGUI()
        {
            if (_guiVisible)
            {
                GUIStyle black = new GUIStyle();
                black.normal.textColor = new Color(0, 0, 0);
                black.fontSize = 22;
                GUILayout.BeginVertical();
                GUILayout.Space(150);
                GUILayout.Label("Set School Uniforms", black);
                if (GUILayout.Button("Top 1"))       SetColorIndex(Clothes.top, 0);
                if (GUILayout.Button("Top 2"))       SetColorIndex(Clothes.top, 1);
                if (GUILayout.Button("Top 3"))       SetColorIndex(Clothes.top, 2);
                if (GUILayout.Button("Top 4"))       SetColorIndex(Clothes.top, 3);
                if (GUILayout.Button("Bottom 1"))    SetColorIndex(Clothes.bottom, 0);
                if (GUILayout.Button("Bottom 2"))    SetColorIndex(Clothes.bottom, 1);
                if (GUILayout.Button("Bottom 3"))    SetColorIndex(Clothes.bottom, 2);
                if (GUILayout.Button("Bottom 4"))    SetColorIndex(Clothes.bottom, 3);
                if (_guiHSV)
                {
                    float H, S, V;
                    Color.RGBToHSV(_colorDict[_guiCurrClothing][_guiCurrIndex], out H, out S, out V);                   
                    GUILayout.Label("Hue", black);
                    H = GUILayout.HorizontalScrollbar(H, 0.05f, 1, 0);
                    GUILayout.Label("Saturation", black);
                    S = GUILayout.HorizontalScrollbar(S, 0.05f, 1, 0);
                    GUILayout.Label("Value", black);
                    V = GUILayout.HorizontalScrollbar(V, 0.05f, 1, 0);
                    _colorDict[_guiCurrClothing][_guiCurrIndex] = Color.HSVToRGB(H, S, V);
                } else
                {
                    GUILayout.Label("Red", black);
                    _colorDict[_guiCurrClothing][_guiCurrIndex].r = GUILayout.HorizontalScrollbar(_colorDict[_guiCurrClothing][_guiCurrIndex].r, 0.05f, 1, 0);
                    GUILayout.Label("Green", black);
                    _colorDict[_guiCurrClothing][_guiCurrIndex].g = GUILayout.HorizontalScrollbar(_colorDict[_guiCurrClothing][_guiCurrIndex].g, 0.05f, 1, 0);
                    GUILayout.Label("Blue", black);
                    _colorDict[_guiCurrClothing][_guiCurrIndex].b = GUILayout.HorizontalScrollbar(_colorDict[_guiCurrClothing][_guiCurrIndex].b, 0.05f, 1, 0);
                }

                _colorDict[_guiCurrClothing][_guiCurrIndex].a = 1;
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, _colorDict[_guiCurrClothing][_guiCurrIndex]);
                tex.Apply();
                tex.wrapMode = TextureWrapMode.Repeat;
                GUI.DrawTexture(new Rect(35, 680, 140, 100), tex, ScaleMode.StretchToFill);
                if (GUILayout.Button(_guiHSV ? "RGB" : "HSV")) { _guiHSV = !_guiHSV; };
                if (GUILayout.Button("Copy Color")) { _copy = _colorDict[_guiCurrClothing][_guiCurrIndex]; };
                if (GUILayout.Button("Paste Color")) { _colorDict[_guiCurrClothing][_guiCurrIndex] = _copy; };
                GUILayout.Space(20);
                if (GUILayout.Button("Finalize")) SetAllCharRandomClothes();
            }
        }

        private static void SetColorIndex(Clothes clothes, int colidx)
        {
            _guiCurrClothing = (int)clothes;
            _guiCurrIndex = colidx;
        }

        private static void ClearCharList()
        {
            while (_charList.Count > 0) _charList.RemoveAt(_charList.Count-1);
        }

        private static void LoadCharList()
        {
            ClearCharList();
            List<SaveData.CharaData> tmp = Traverse.Create(_classroomScene.classRoomList).Field("charaList").GetValue<List<SaveData.CharaData>>();
            for (int i=0; i<tmp.Count; i++)
            {

                _charList.Add(tmp[i]);
            }
        }

        private static void SetRandomClothes(ChaFileControl chaFile)
        {
            System.Random Rand = new System.Random();
            for (int i=0; i < 2; i++)
            {
                ChaFileClothes.PartsInfo[] currParts = chaFile.coordinate[i].clothes.parts;
                int[] currSubParts = chaFile.coordinate[i].clothes.subPartsId;
                bool changeTop = false;

                currParts[(int)Clothes.top].emblemeId = _emblemID;
                if (!TopList.Contains(currParts[(int)Clothes.top].id))
                {
                    currParts[(int)Clothes.top].id = TopList[Rand.Next(TopList.Count)];
                    changeTop = true;
                }
                if (!TopList.Contains(currParts[(int)Clothes.top].id))
                {
                    currParts[(int)Clothes.bottom].id = TopList[Rand.Next(BottomList.Count)];
                }
                if (changeTop)
                {
                    switch (currParts[(int)Clothes.top].id)
                    {
                        case 1:
                            currSubParts[(int)Clothes.subshirt] = SubshirtList[Rand.Next(SubshirtList.Count)];
                            currSubParts[(int)Clothes.subjacketcollar] = SubjacketList[Rand.Next(SubjacketList.Count)];
                            currSubParts[(int)Clothes.subdecoration] = SubblazerdecoList[Rand.Next(SubblazerdecoList.Count)];
                            break;
                        case 2:
                            currSubParts[(int)Clothes.subshirt] = SubsailorList[Rand.Next(SubsailorList.Count)];
                            currSubParts[(int)Clothes.subjacketcollar] = SubcollarList[Rand.Next(SubcollarList.Count)];
                            currSubParts[(int)Clothes.subdecoration] = SubsailordecoList[Rand.Next(SubsailordecoList.Count)];
                            break;
                    }
                }
                for (int j = 0; j < 4; j++)
                {
                    currParts[(int)Clothes.top].colorInfo[j].baseColor = _colorDict[(int)Clothes.top][j];
                    currParts[(int)Clothes.bottom].colorInfo[j].baseColor = _colorDict[(int)Clothes.bottom][j];
                }
                Utils.Sound.Play(SystemSE.ok_l);
            }
        }

        //public static ChaFileControl GetSelectedClassCharacter()
        //{
        //    SaveData.CharaData data = Traverse.Create<ActionGame.PreviewClassData>().Field("data").GetValue<SaveData.CharaData>();
        //    if (data != null) return data.charFile;
        //    return null;
        //}

        private static void SetAllCharRandomClothes()
        {
            LoadCharList();
            foreach (SaveData.CharaData chaData in _charList)
            {
                if (chaData.GetType() != typeof(SaveData.Player))
                {
                    SaveData.Heroine curr = (SaveData.Heroine)chaData;
                    if (!curr.isTeacher) SetRandomClothes(curr.charFile);
                }
            }
        }
    }
}
