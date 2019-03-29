using BepInEx;
using Harmony;
using UniRx;
using UnityEngine;
using Illusion.Game;
using System;
using System.Collections.Generic;

namespace KK_UniformUniforms
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class KK_UniformUniforms : BaseUnityPlugin
    {
        public const string GUID = "com.cptgrey.bepinex.uniform";
        public const string PluginName = "KK Uniform Uniforms";
        public const string PluginNameInternal = "KK_UniformUniforms";
        public const string Version = "1.0";

        public static class Clothes
        {
            public static int Top = 0;
            public static int Bottom = 1;
            public static int Bra = 2;
            public static int Panties = 3;
            public static int Gloves = 4;
            public static int Pantyhose = 5;
            public static int Legwear = 6;
            public static int Inshoe = 7;
            public static int Outshoe = 8;
            public static int Subshirt = 0;
            public static int Subjacketcollar = 1;
            public static int Subdecoration = 2;
        }

        private static class Lists
        {
            public static List<int> Top = new List<int> { 1, 2 };
            public static List<int> Bottom = new List<int> { 4, 5, 6, 7, 8 };
            public static List<int> Pantyhose = new List<int> { };
            public static List<int> Gloves = new List<int> { };
            public static List<int> Legwear = new List<int> { };
            public static List<int> Inshoe = new List<int> { };
            public static List<int> Outshoe = new List<int> { };

            public static List<int> Subshirt = new List<int> { 0, 1, 2, 3, 4, 6, 8 };
            public static List<int> Subsailor = new List<int> { 0, 1, 2, 3, 4, 5 };
            public static List<int> Subjacket = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            public static List<int> Subcollar = new List<int> { 0, 1, 2, 3, 4, 5 };
            public static List<int> Subblazerdeco = new List<int> { 0, 1, 2, 3, 4, 5 };
            public static List<int> Subsailordeco = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        }

        private static ActionGame.PreviewClassData _currClassData { get; set; }
        private static Rect _guiRect { get; set; }
        private static int _emblemID { get; set; }
        private static int _guiCurrClothing { get; set; }
        private static int _guiCurrIndex { get; set; }
        private static bool _guiHSV { get; set; }
        private static Color _copy { get; set; }
        private static bool _guiVisible { get; set; }
        private static List<SaveData.CharaData> _charList { get; set; }
        private static ActionGame.ClassRoomSelectScene _classroomScene { get; set; }
        private static System.Random Rand = new System.Random();
        private static bool _changeTop = true;
        private static bool _changeBottom = true;
        private static bool _changeEmblem = true;
        private static bool _refreshFlag = false;
        private static GUIStyle _textStyle { get; set; }

        public static Dictionary<int, Color[]> _colorDict { get; set; }
        public static bool IsInClassRoom { get; private set; }
        
        void Start()
        {
            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(typeof(KK_UniformUniforms));

            _guiVisible = false;
            _guiCurrClothing = Clothes.Top;
            _guiCurrIndex = 0;
            _guiHSV = true;
            _charList = new List<SaveData.CharaData>();
            _colorDict = new Dictionary<int, Color[]>();
            _copy = new Color(1, 1, 1);
            _colorDict.Add(Clothes.Top, new Color[]
            {
                new Color(1,1,1),
                new Color(1,0,0),
                new Color(0,1,0),
                new Color(0,0,1),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
            });
            _colorDict.Add(Clothes.Bottom, new Color[]
            {
                new Color(1,1,1),
                new Color(1,0,0),
                new Color(0,1,0),
                new Color(0,0,1),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
            });

            _textStyle = new GUIStyle();
            _textStyle.normal.textColor = Color.black;
            _textStyle.fontSize = 16;
        }

        void Update()
        {
            _classroomScene = Singleton<ActionGame.ClassRoomSelectScene>.Instance;
            if (_classroomScene != null && _classroomScene.classRoomList.isVisible) IsInClassRoom = true;
            else IsInClassRoom = false;
            if (IsInClassRoom)
            {
                Traverse enterPreview = Traverse.Create(_classroomScene.classRoomList).Field("enterPreview");
                if (enterPreview.FieldExists())
                    _currClassData = enterPreview.GetValue<ReactiveProperty<ActionGame.PreviewClassData>>().Value;
                _emblemID = Singleton<Manager.Game>.Instance.saveData.emblemID;
                _guiVisible = true;
            }
            else _guiVisible = false;
        }

        private void OnGUI()
        {
            if (_guiVisible)
            {
                GUILayout.Space(30);
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(30);
                    GUILayout.Label("Set School Uniforms", _textStyle);
                    for (int b = 0; b < 4; b++)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(String.Format("Top {0}", (b + 1).ToString()))) SetColorIndex(Clothes.Top, b);
                        if (GUILayout.Button(String.Format("Pattern {0}", (b + 1).ToString()))) SetColorIndex(Clothes.Top, b + 4);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);
                    }
                    for (int b = 0; b < 4; b++)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(String.Format("Bottom {0}", (b + 1).ToString()))) SetColorIndex(Clothes.Bottom, b);
                        if (GUILayout.Button(String.Format("Pattern {0}", (b + 1).ToString()))) SetColorIndex(Clothes.Bottom, b + 4);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);
                    }
                    if (_guiHSV)
                    {
                        Color.RGBToHSV(_colorDict[_guiCurrClothing][_guiCurrIndex], out float H, out float S, out float V);
                        float[] hsv = SetGUISliderTextures(H, S, V, true);
                        _colorDict[_guiCurrClothing][_guiCurrIndex] = Color.HSVToRGB(hsv[0], hsv[1], hsv[2]);
                    }
                    else
                    {
                        float[] rgb = SetGUISliderTextures(
                            _colorDict[_guiCurrClothing][_guiCurrIndex].r,
                            _colorDict[_guiCurrClothing][_guiCurrIndex].g,
                            _colorDict[_guiCurrClothing][_guiCurrIndex].b,
                            false
                        );
                        _colorDict[_guiCurrClothing][_guiCurrIndex].r = rgb[0];
                        _colorDict[_guiCurrClothing][_guiCurrIndex].g = rgb[1];
                        _colorDict[_guiCurrClothing][_guiCurrIndex].b = rgb[2];
                    }

                    _colorDict[_guiCurrClothing][_guiCurrIndex].a = 1;
                    Texture2D tex = new Texture2D(100, 80);
                    for (int x = 1; x <= 100; x++)
                        for (int y = 1; y <= 80; y++)
                            tex.SetPixel(x, y, _colorDict[_guiCurrClothing][_guiCurrIndex]);
                    tex.Apply();
                    tex.wrapMode = TextureWrapMode.Repeat;
                    if (GUILayout.Button(_guiHSV ? "RGB" : "HSV")) { _guiHSV = !_guiHSV; };
                    GUILayout.Space(5);
                    GUILayout.Box(tex);
                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Copy")) { _copy = _colorDict[_guiCurrClothing][_guiCurrIndex]; };
                    if (GUILayout.Button("Paste")) { _colorDict[_guiCurrClothing][_guiCurrIndex] = _copy; };
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Auto Pat.")) SetDarkerPatternColors();
                    if (GUILayout.Button("From Card")) LoadColorsFromCharacter();
                    GUILayout.EndHorizontal();
                    SetToggles();
                    if (GUILayout.Button("Apply")) SetAllCharRandomClothes();
                }
            }
        }

        private static float[] SetGUISliderTextures(float x, float y, float z, bool hsv = false)
        {
            Texture2D normal = GUI.skin.horizontalSlider.normal.background;
            Texture2D active = GUI.skin.horizontalSlider.active.background;
            Texture2D hover = GUI.skin.horizontalSlider.hover.background;
            Texture2D focused = GUI.skin.horizontalSlider.focused.background;

            Texture2D xTex = new Texture2D(1, 1);
            Texture2D yTex = new Texture2D(1, 1);
            Texture2D zTex = new Texture2D(1, 1);

            if (hsv)
            {
                xTex.SetPixel(1, 1, Color.HSVToRGB(x, 1, 1));
                yTex.SetPixel(1, 1, Color.HSVToRGB(x, y, 1));
                zTex.SetPixel(1, 1, Color.HSVToRGB(x, y, z));
            } else
            {
                xTex.SetPixel(1, 1, new Color(x, 0, 0));
                yTex.SetPixel(1, 1, new Color(0, y, 0));
                zTex.SetPixel(1, 1, new Color(0, 0, z));
            }

            xTex.Apply(); yTex.Apply(); zTex.Apply();

            GUILayout.Label(hsv ? "Hue" : "Red", _textStyle);
            GUI.skin.horizontalSlider.normal.background = xTex;
            GUI.skin.horizontalSlider.active.background = xTex;
            GUI.skin.horizontalSlider.hover.background = xTex;
            GUI.skin.horizontalSlider.focused.background = xTex;
            x = GUILayout.HorizontalSlider(x, 0, 1);

            GUILayout.Label(hsv ? "Saturation" : "Green", _textStyle);
            GUI.skin.horizontalSlider.normal.background = yTex;
            GUI.skin.horizontalSlider.active.background = yTex;
            GUI.skin.horizontalSlider.hover.background = yTex;
            GUI.skin.horizontalSlider.focused.background = yTex;
            y = GUILayout.HorizontalSlider(y, 0, 1);

            GUILayout.Label(hsv ? "Value" : "Blue", _textStyle);
            GUI.skin.horizontalSlider.normal.background = zTex;
            GUI.skin.horizontalSlider.active.background = zTex;
            GUI.skin.horizontalSlider.hover.background = zTex;
            GUI.skin.horizontalSlider.focused.background = zTex;
            z = GUILayout.HorizontalSlider(z, 0, 1);

            GUI.skin.horizontalSlider.normal.background = normal;
            GUI.skin.horizontalSlider.active.background = active;
            GUI.skin.horizontalSlider.hover.background = hover;
            GUI.skin.horizontalSlider.focused.background = focused;
            return new float[] { x, y, z };
        }

        private static void SetToggles()
        {
            // Even hackier, but it works...

            Color onActive = GUI.skin.toggle.onActive.textColor;
            Color onHover = GUI.skin.toggle.onHover.textColor;
            Color onFocused = GUI.skin.toggle.onFocused.textColor;
            Color onNormal = GUI.skin.toggle.onNormal.textColor;
            Color normal = GUI.skin.toggle.normal.textColor;
            Color active = GUI.skin.toggle.active.textColor;
            Color hover = GUI.skin.toggle.hover.textColor;
            Color focused = GUI.skin.toggle.focused.textColor;
            GUI.skin.toggle.onActive.textColor = Color.black;
            GUI.skin.toggle.onHover.textColor = Color.black;
            GUI.skin.toggle.onNormal.textColor = Color.black;
            GUI.skin.toggle.onFocused.textColor = Color.black;
            GUI.skin.toggle.normal.textColor = Color.black;
            GUI.skin.toggle.active.textColor = Color.black;
            GUI.skin.toggle.hover.textColor = Color.black;
            GUI.skin.toggle.focused.textColor = Color.black;
            _changeTop = GUILayout.Toggle(_changeTop, "Change Top");
            _changeBottom = GUILayout.Toggle(_changeBottom, "Change Bottom");
            _changeEmblem = GUILayout.Toggle(_changeEmblem, "Change Emblem");
            GUI.skin.toggle.onActive.textColor = onActive;
            GUI.skin.toggle.onHover.textColor = onHover;
            GUI.skin.toggle.onNormal.textColor = onNormal;
            GUI.skin.toggle.onFocused.textColor = onFocused;
            GUI.skin.toggle.normal.textColor = normal;
            GUI.skin.toggle.active.textColor = active;
            GUI.skin.toggle.hover.textColor = hover;
            GUI.skin.toggle.focused.textColor = focused;
        }

        public static Color GetSlightlyDarkerColor(Color incolor)
        {
            Color.RGBToHSV(incolor, out float h, out float s, out float v);
            v = v > 0.08f ? v - 0.08f : 0;
            return Color.HSVToRGB(h, s, v);
        }

        private static void SetDarkerPatternColors()
        {
            for (int i=0; i < 2; i++)
                for (int j=0; j < 4; j++)
                    _colorDict[i][j + 4] = GetSlightlyDarkerColor(_colorDict[i][j]);
        }

        private static void SetColorIndex(int clothes, int colidx)
        {
            _guiCurrClothing = clothes;
            _guiCurrIndex = colidx;
        }

        private static void ClearCharList()
        {
            while (_charList.Count > 0) _charList.RemoveAt(_charList.Count-1);
        }

        private static void LoadCharList()
        {
            // Clear list
            ClearCharList();

            // Load current data from heroineList and new characters from charaList
            List<SaveData.Heroine> heroineList = Singleton<Manager.Game>.Instance.HeroineList;
            List<SaveData.CharaData> classList = Traverse.Create(_classroomScene.classRoomList).Field("charaList").GetValue<List<SaveData.CharaData>>();
            foreach (SaveData.Heroine heroine in heroineList)
            {
                _charList.Add(heroine);
            }
            for (int i = heroineList.Count; i < classList.Count; i++)
            {
                if (classList[i].GetType() != typeof(SaveData.Player))
                {
                    _charList.Add(classList[i]);
                }
            }
        }

        private static void SetRandomClothes(ChaFileControl chaFile)
        {
            for (int i=0; i < 2; i++)
            {
                ChaFileClothes.PartsInfo[] currParts = chaFile.coordinate[i].clothes.parts;
                int[] currSubParts = chaFile.coordinate[i].clothes.subPartsId;
                bool topChanged = false;

                currParts[Clothes.Top].emblemeId = _changeEmblem ? _emblemID : currParts[Clothes.Top].emblemeId;

                if (_changeTop && !Lists.Top.Contains(currParts[Clothes.Top].id))
                {
                    currParts[Clothes.Top].id = Lists.Top[Rand.Next(Lists.Top.Count)];
                    topChanged = true;
                }
                if (_changeBottom && !Lists.Bottom.Contains(currParts[Clothes.Bottom].id))
                {
                    currParts[Clothes.Bottom].id = Lists.Bottom[Rand.Next(Lists.Bottom.Count)];
                }
                if (topChanged)
                {
                    switch (currParts[Clothes.Top].id)
                    {
                        case 1:
                            currSubParts[Clothes.Subshirt] = Lists.Subshirt[Rand.Next(Lists.Subshirt.Count)];
                            currSubParts[Clothes.Subjacketcollar] = Lists.Subjacket[Rand.Next(Lists.Subjacket.Count)];
                            currSubParts[Clothes.Subdecoration] = Lists.Subblazerdeco[Rand.Next(Lists.Subblazerdeco.Count)];
                            break;
                        case 2:
                            currSubParts[Clothes.Subshirt] = Lists.Subsailor[Rand.Next(Lists.Subsailor.Count)];
                            currSubParts[Clothes.Subjacketcollar] = Lists.Subcollar[Rand.Next(Lists.Subcollar.Count)];
                            currSubParts[Clothes.Subdecoration] = Lists.Subsailordeco[Rand.Next(Lists.Subsailordeco.Count)];
                            break;
                    }
                }
            }

            SetRandomColors(chaFile);
        }

        private static void SetRandomColors(ChaFileControl chaFile)
        {
            for (int i = 0; i < 2; i++)
            {
                ChaFileClothes.PartsInfo[] currParts = chaFile.coordinate[i].clothes.parts;

                for (int j = 0; j < 4; j++)
                {
                    currParts[Clothes.Top].colorInfo[j].baseColor = _colorDict[Clothes.Top][j];
                    currParts[Clothes.Top].colorInfo[j].patternColor = _colorDict[Clothes.Top][j + 4];
                    currParts[Clothes.Bottom].colorInfo[j].baseColor = _colorDict[Clothes.Bottom][j];
                    currParts[Clothes.Bottom].colorInfo[j].patternColor = _colorDict[Clothes.Bottom][j + 4];
                }
            }
        }

        private static void LoadColorsFromCharacter()
        {
            if (_currClassData.data == null) return;
            ChaFileControl chaFile = _currClassData.data.charFile;
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    _colorDict[(int)Clothes.Top][i] = chaFile.coordinate[j].clothes.parts[Clothes.Top].colorInfo[i].baseColor;
                    _colorDict[(int)Clothes.Bottom][i] = chaFile.coordinate[j].clothes.parts[Clothes.Bottom].colorInfo[i].baseColor;
                    _colorDict[(int)Clothes.Top][i + 4] = chaFile.coordinate[j].clothes.parts[Clothes.Top].colorInfo[i].patternColor;
                    _colorDict[(int)Clothes.Bottom][i + 4] = chaFile.coordinate[j].clothes.parts[Clothes.Bottom].colorInfo[i].baseColor;
                }
            }
        }

        private static void SetAllCharRandomClothes()
        {
            LoadCharList();
            foreach (SaveData.CharaData chaData in _charList)
            {
                if (!((SaveData.Heroine)chaData).isTeacher) SetRandomClothes(chaData.charFile);
                if (chaData.chaCtrl != null) chaData.chaCtrl.chaFile.coordinate = chaData.charFile.coordinate;
            }
            SaveData.Player player = Singleton<Manager.Game>.Instance.Player;
            if (player.chaCtrl != null) player.chaCtrl.chaFile.coordinate = player.charFile.coordinate;
            SetRandomColors(player.charFile);
            Utils.Sound.Play(SystemSE.ok_l);
        }
    }
}
