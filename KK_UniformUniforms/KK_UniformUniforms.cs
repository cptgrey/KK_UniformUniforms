using BepInEx;
using Harmony;
using UniRx;
using UnityEngine;
using Illusion.Game;
using System;
using System.Linq;
using System.Collections.Generic;
using Logger = BepInEx.Logger;

namespace KK_UniformUniforms
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class KK_UniformUniforms : BaseUnityPlugin
    {
        public const string GUID = "com.cptgrey.bepinex.uniform";
        public const string PluginName = "KK Uniform Uniforms";
        public const string PluginNameInternal = "KK_UniformUniforms";
        public const string Version = "1.0";

        private enum Apply
        {
            All, Class, Card
        }

        public enum ColorKeys
        {
            MainTop, MainBottom, PETop, PEBottom, SwimsuitTop, SwimsuitBottom
        }

        public static class Outfits
        {
            public static int School = 0;
            public static int GoingHome = 1;
            public static int PE = 2;
            public static int Swimsuit = 3;
            public static int Club = 4;
            public static int Casual = 5;
            public static int Sleep = 6;
        }

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
            public static List<int> Bra = new List<int> { };
            public static List<int> Panties = new List<int> { };
            public static List<int> Pantyhose = new List<int> { };
            public static List<int> Gloves = new List<int> { };
            public static List<int> Legwear = new List<int> { };
            public static List<int> Inshoe = new List<int> { };
            public static List<int> Outshoe = new List<int> { };
            public static List<int> PETop = new List<int> { 2, 6, 21, 38, 41};
            public static List<int> PEBottom = new List<int> { 1, 2, 12};
            public static List<int> SwimsuitTop = new List<int> { 0 };
            public static List<int> SwimsuitBottom = new List<int> { 0 };
            public static List<int> SwimsuitBras = new List<int> { 2, 10 };

            public static List<int> SubBlazerUnder = new List<int> { 0, 1, 2, 3, 4, 6, 8 };
            public static List<int> SubSailorUnder = new List<int> { 0, 1, 2, 3, 4, 5 };
            public static List<int> SubBlazerOver = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            public static List<int> SubSailorOver = new List<int> { 0, 1, 2, 3, 4, 5 };
            public static List<int> SubBlazerdeco = new List<int> { 0, 1, 2, 3, 4, 5 };
            public static List<int> SubSailorDeco = new List<int> { 0, 1, 2, 3, 4, 5, 6 };
            public static List<int> SubPEUnder = new List<int> { 5 };
            public static List<int> SubPEOver = new List<int> { 0, 4, };
            public static List<int> SubPEDeco = new List<int> { 0 };
            public static List<int> SubSwimsuitUnder = new List<int> { 0 };
            public static List<int> SubSwimsuitOver = new List<int> { 0 };
            public static List<int> SubSwimsuitDeco = new List<int> { 0 };
        }

        private static ActionGame.PreviewClassData _currClassData { get; set; }
        private static int _emblemID { get; set; }
        private static ColorKeys _guiCurrClothing { get; set; }
        private static int _guiCurrIndex { get; set; }
        private static bool _patternToggle { get; set; }
        private static Color _copy { get; set; }
        private static bool _guiHSV { get; set; }
        private static bool _guiVisible { get; set; }
        private static List<SaveData.CharaData> _charList { get; set; }
        private static ActionGame.ClassRoomSelectScene _classroomScene { get; set; }
        private static Rect _screenRect { get; set; }
        private static Vector2 _scrollPos { get; set; }
        private static System.Random Rand = new System.Random();

        // Change Flags
        private static bool _changeTop = true;
        private static bool _changeBottom = true;
        private static bool _changeColorTop = true;
        private static bool _changeColorBottom = true;
        private static bool _changeEmblem = true;
        private static bool _advanced = false;
        private static bool _changeSchool = true;
        private static bool _changeGoingHome = true;
        private static bool _changeSwimsuit = true;
        private static bool _changePE = true;
        private static bool _bottomToggle = false;
        private static int _togglewidth = 57;
        private static int _guiOffset = 165;
        private static int _advancedMenuHeight = 457;
        private static int _width = 165;
        private static int _widthlim = 155;
        private static float _guiHeightMul = .92f;
        private static int _strictUniform = 0;

        public static Dictionary<ColorKeys, Color[]> _colorDict { get; set; }
        public static bool IsInClassRoom { get; private set; }
        
        void Start()
        {
            var harmony = HarmonyInstance.Create(GUID);
            harmony.PatchAll(typeof(KK_UniformUniforms));

            _guiVisible = false;
            _guiCurrClothing = ColorKeys.MainTop;
            _guiCurrIndex = 0;
            _patternToggle = false;
            _guiHSV = true;
            _charList = new List<SaveData.CharaData>();
            _colorDict = new Dictionary<ColorKeys, Color[]>();
            _copy = new Color(1, 1, 1);
            _colorDict.Add(ColorKeys.MainTop, new Color[]
            {
                new Color(.9f,.9f,.9f),
                new Color(.364f,.434f,.52f),
                new Color(.957f,.957f,.957f),
                new Color(.73f,.117f,.178f),
                new Color(.364f,.809f,1),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(.596f,.228f,.228f),
            });
            _colorDict.Add(ColorKeys.MainBottom, new Color[]
            {
                new Color(.768f,.134f,.134f),
                new Color(.912f,.912f,.912f),
                new Color(0,0,0),
                new Color(1,1,1),
                new Color(.768f,.134f,.134f),
                new Color(.912f,.912f,.912f),
                new Color(0,0,0),
                new Color(1,1,1),
            });
            _colorDict.Add(ColorKeys.PETop, new Color[]
            {
                new Color(.958f,.958f,.958f),
                new Color(.376f,.431f,.529f),
                new Color(.494f,.621f,1),
                new Color(.784f,.352f,.352f),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
            });
            _colorDict.Add(ColorKeys.PEBottom, new Color[]
            {
                new Color(.408f,.606f,.952f),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(.408f,.606f,.952f),
                new Color(1,1,1),
                new Color(1,1,1),
                new Color(1,1,1),
            });
            _colorDict.Add(ColorKeys.SwimsuitTop, new Color[]
            {
                new Color(.378f,.433f,.529f),
                new Color(.378f,.433f,.529f),
                new Color(.378f,.433f,.529f),
                new Color(1,1,1),
                new Color(.912f,.912f,.912f),
                new Color(.912f,.912f,.912f),
                new Color(1,1,1),
                new Color(1,1,1),
            });
            _colorDict.Add(ColorKeys.SwimsuitBottom, new Color[]
            {
                new Color(.912f,.912f,.912f),
                new Color(1,.787f,.787f),
                new Color(1,.485f,.485f),
                new Color(1,1,1),
                new Color(.912f,.912f,.912f),
                new Color(1,.787f,.787f),
                new Color(1,.485f,.485f),
                new Color(1,1,1),
            });
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
            float rectHeight = 1080 - _guiOffset >= Screen.height * _guiHeightMul ? Screen.height * _guiHeightMul : 1080 - _guiOffset;
            _screenRect = new Rect(5, 5, _advanced && rectHeight != 1080 - _guiOffset ? _width : _width - 24, _advanced ? rectHeight : 1080 - _guiOffset - _advancedMenuHeight);
        }

        private void OnGUI()
        {
            if (_guiVisible)
            {
                GUILayout.Window(94761634, _screenRect, DrawMainGUI, "Set School Uniforms");
            }
        }

        private static void DrawMainGUI(int id)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(20);
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(Screen.height * _guiHeightMul - 22));
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(_widthlim));
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginVertical();
                        {
                            if (GUILayout.Button("Load From Selected")) LoadColorsFromCharacter();

                            _advanced = GUILayout.Toggle(_advanced, "Advanced Controls");
                            if (_advanced) AdvancedColor();
                            GUILayout.BeginVertical(GUI.skin.box);
                            {
                                GUILayout.Label("Outfits to change");
                                _changeSchool = GUILayout.Toggle(_changeSchool, "School");
                                _changeGoingHome = GUILayout.Toggle(_changeGoingHome, "Going Home");
                                _changePE = GUILayout.Toggle(_changePE, "PE");
                                _changeSwimsuit = GUILayout.Toggle(_changeSwimsuit, "Swimsuit");
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical(GUI.skin.box);
                            {
                                GUILayout.Label("Parts to change");
                                GUILayout.BeginHorizontal();
                                _changeTop = GUILayout.Toggle(_changeTop, "Top", GUILayout.Width(_togglewidth));
                                _changeBottom = GUILayout.Toggle(_changeBottom, "Bottom", GUILayout.Width(_togglewidth));
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical(GUI.skin.box);
                            {
                                GUILayout.Label("Colors to change");
                                GUILayout.BeginHorizontal();
                                _changeColorTop = GUILayout.Toggle(_changeColorTop, "Top", GUILayout.Width(_togglewidth));
                                _changeColorBottom = GUILayout.Toggle(_changeColorBottom, "Bottom", GUILayout.Width(_togglewidth));
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical(GUI.skin.box);
                            {
                                GUILayout.Label("Lock uniform");
                                GUILayout.BeginHorizontal();
                                _strictUniform = GUILayout.Toggle(_strictUniform == 1, "Blazer", GUILayout.Width(_togglewidth)) ? 1 : _strictUniform == 2 ? 2 : 0;
                                _strictUniform = GUILayout.Toggle(_strictUniform == 2, "Sailor", GUILayout.Width(_togglewidth)) ? 2 : _strictUniform == 1 ? 1 : 0;
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();

                            _changeEmblem = GUILayout.Toggle(_changeEmblem, "Change Emblem");

                            if (GUILayout.Button("Apply to Selected")) ApplySettings(Apply.Card);
                            if (GUILayout.Button("Apply to Class")) ApplySettings(Apply.Class);
                            if (GUILayout.Button("Apply to All")) ApplySettings(Apply.All);
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(24);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
        }

        private static void AdvancedColor()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                GUILayout.Label("Select Outfit");
                GUILayout.BeginHorizontal();
                _guiCurrClothing = GUILayout.Toggle(_guiCurrClothing == ColorKeys.MainTop || _guiCurrClothing == ColorKeys.MainBottom, "School", GUILayout.Width(_togglewidth)) ?
                    ColorKeys.MainTop : _guiCurrClothing;
                _guiCurrClothing = GUILayout.Toggle(_guiCurrClothing == ColorKeys.PETop || _guiCurrClothing == ColorKeys.PEBottom, "PE", GUILayout.Width(_togglewidth)) ?
                    ColorKeys.PETop : _guiCurrClothing;
                GUILayout.EndHorizontal();
                _guiCurrClothing = GUILayout.Toggle(_guiCurrClothing == ColorKeys.SwimsuitTop || _guiCurrClothing == ColorKeys.SwimsuitBottom, "Swimsuit") ?
                    ColorKeys.SwimsuitTop : _guiCurrClothing;

                GUILayout.Label("Select Part Color");
                GUILayout.BeginHorizontal();
                _guiCurrIndex = GUILayout.Toggle(_guiCurrIndex == 0 || _guiCurrIndex == 4, "1") ? 0 : _guiCurrIndex;
                _guiCurrIndex = GUILayout.Toggle(_guiCurrIndex == 1 || _guiCurrIndex == 5, "2") ? 1 : _guiCurrIndex;
                _guiCurrIndex = GUILayout.Toggle(_guiCurrIndex == 2 || _guiCurrIndex == 6, "3") ? 2 : _guiCurrIndex;
                _guiCurrIndex = GUILayout.Toggle(_guiCurrIndex == 3 || _guiCurrIndex == 7, "4") ? 3 : _guiCurrIndex;
                GUILayout.EndHorizontal();

                if (_patternToggle) _guiCurrIndex += 4;
                if (_guiCurrClothing != ColorKeys.SwimsuitTop)
                {
                    GUILayout.BeginHorizontal();
                    _bottomToggle = !GUILayout.Toggle(!_bottomToggle, "Top", GUILayout.Width(_togglewidth));
                    _bottomToggle = GUILayout.Toggle(_bottomToggle, "Bottom", GUILayout.Width(_togglewidth));
                    GUILayout.EndHorizontal();
                    if (_bottomToggle) _guiCurrClothing += 1;
                }
                else GUILayout.Space(22);

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

                GUILayout.BeginHorizontal();
                _patternToggle = !GUILayout.Toggle(!_patternToggle, "Main");
                _patternToggle = GUILayout.Toggle(_patternToggle, "Pattern");
                GUILayout.EndHorizontal();

                Texture2D tex = new Texture2D(65, 65);
                for (int x = 1; x <= 65; x++)
                    for (int y = 1; y <= 65; y++)
                        tex.SetPixel(x, y, _colorDict[_guiCurrClothing][_guiCurrIndex]);
                tex.Apply();
                tex.wrapMode = TextureWrapMode.Repeat;

                if (GUILayout.Button(_guiHSV ? "To RGB" : "To HSV")) { _guiHSV = !_guiHSV; };
                GUILayout.Space(5);
                GUILayout.Box(tex);
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy")) { _copy = _colorDict[_guiCurrClothing][_guiCurrIndex]; };
                if (GUILayout.Button("Paste")) { _colorDict[_guiCurrClothing][_guiCurrIndex] = _copy; };
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Auto Pattern Colors")) SetDarkerPatternColors();
            }
            GUILayout.EndVertical();
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

            GUILayout.Label(hsv ? "Hue" : "Red");
            GUI.skin.horizontalSlider.normal.background = xTex;
            GUI.skin.horizontalSlider.active.background = xTex;
            GUI.skin.horizontalSlider.hover.background = xTex;
            GUI.skin.horizontalSlider.focused.background = xTex;
            x = GUILayout.HorizontalSlider(x, 0, 1);

            GUILayout.Label(hsv ? "Saturation" : "Green");
            GUI.skin.horizontalSlider.normal.background = yTex;
            GUI.skin.horizontalSlider.active.background = yTex;
            GUI.skin.horizontalSlider.hover.background = yTex;
            GUI.skin.horizontalSlider.focused.background = yTex;
            y = GUILayout.HorizontalSlider(y, 0, 1);

            GUILayout.Label(hsv ? "Value" : "Blue");
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

        public static Color GetSlightlyDarkerColor(Color incolor)
        {
            Color.RGBToHSV(incolor, out float h, out float s, out float v);
            v = v > 0.08f ? v - 0.08f : 0;
            return Color.HSVToRGB(h, s, v);
        }

        private static void SetDarkerPatternColors()
        {
            foreach (ColorKeys key in (ColorKeys[]) Enum.GetValues(typeof(ColorKeys)))
                for (int j=0; j < 4; j++)
                    _colorDict[key][j + 4] = GetSlightlyDarkerColor(_colorDict[key][j]);
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

        private static List<int> GetOutfitsToChange()
        {
            List<int> outfitsToChange = new List<int>();

            if (_changeSchool) outfitsToChange.Add(Outfits.School);
            if (_changeGoingHome) outfitsToChange.Add(Outfits.GoingHome);
            if (_changePE) outfitsToChange.Add(Outfits.PE);
            if (_changeSwimsuit) outfitsToChange.Add(Outfits.Swimsuit);

            return outfitsToChange;
        }

        private static void SetRandomClothes(ChaFileControl chaFile)
        {

            List<int> outfitsToChange = GetOutfitsToChange();

            foreach (int outfit in outfitsToChange)
            {
                ChaFileClothes.PartsInfo[] currParts = chaFile.coordinate[outfit].clothes.parts;
                int[] currSubParts = chaFile.coordinate[outfit].clothes.subPartsId;
                bool topChanged = false;
                
                currParts[Clothes.Top].emblemeId = _changeEmblem ? _emblemID : currParts[Clothes.Top].emblemeId;

                List<int> tops;
                List<int> bottoms;

                if (outfit == Outfits.School || outfit == Outfits.GoingHome)
                {
                    tops = Lists.Top;
                    bottoms = Lists.Bottom;
                }
                else if (outfit == Outfits.PE)
                {
                    tops = Lists.PETop;
                    bottoms = Lists.PEBottom;
                } else
                {
                    tops = Lists.SwimsuitTop;
                    bottoms = Lists.SwimsuitBottom;
                }

                // Change Tops
                if (_strictUniform == 0)
                {
                    if (_changeTop && !tops.Contains(currParts[Clothes.Top].id))
                    {
                        currParts[Clothes.Top].id = tops[Rand.Next(tops.Count)];
                        topChanged = true;
                    }
                } else
                {
                    currParts[Clothes.Top].id = _strictUniform;
                    topChanged = true;
                }

                // Change Bottoms
                if (_changeBottom && !bottoms.Contains(currParts[Clothes.Bottom].id))
                {
                    currParts[Clothes.Bottom].id = bottoms[Rand.Next(bottoms.Count)];
                }
                
                // Change Swimsuit
                if (outfit == Outfits.Swimsuit)
                {
                    currParts[Clothes.Bra].id = Lists.SwimsuitBras[Rand.Next(Lists.SwimsuitBras.Count)];
                }

                List<int> subunder;
                List<int> subover;
                List<int> subdeco;

                if (outfit == Outfits.School || outfit == Outfits.GoingHome)
                {
                    if (currParts[Clothes.Top].id == 1)
                    {
                        subunder = Lists.SubBlazerUnder;
                        subover = Lists.SubBlazerOver;
                        subdeco = Lists.SubBlazerdeco;
                    } else
                    {
                        subunder = Lists.SubSailorUnder;
                        subover = Lists.SubSailorOver;
                        subdeco = Lists.SubSailorDeco;
                    }
                }
                else if (outfit == Outfits.PE)
                {
                    subunder = Lists.SubPEUnder;
                    subover = Lists.SubPEOver;
                    subdeco = Lists.SubPEDeco;
                }
                else
                {
                    subunder = Lists.SubSwimsuitUnder;
                    subover = Lists.SubSwimsuitOver;
                    subdeco = Lists.SubSwimsuitDeco;
                }


                if (topChanged)
                {
                    currSubParts[Clothes.Subshirt] = subunder[Rand.Next(subunder.Count)];
                    currSubParts[Clothes.Subjacketcollar] = subover[Rand.Next(subover.Count)];
                    currSubParts[Clothes.Subdecoration] = subdeco[Rand.Next(subdeco.Count)];
                }                
            }

            // Set Colors
            SetColors(chaFile);
        }

        private static void SetColors(ChaFileControl chaFile)
        {
            List<int> outfitsToChange = GetOutfitsToChange();

            foreach (int outfit in outfitsToChange)
            {
                ChaFileClothes.PartsInfo[] currParts = chaFile.coordinate[outfit].clothes.parts;

                ColorKeys key;
                int clothesindex;

                if (outfit == Outfits.School || outfit == Outfits.GoingHome)
                {
                    key = ColorKeys.MainTop;
                    clothesindex = Clothes.Top;
                } else if (outfit == Outfits.PE)
                {
                    key = ColorKeys.PETop;
                    clothesindex = Clothes.Top;
                } else
                {
                    key = ColorKeys.SwimsuitTop;
                    clothesindex = Clothes.Bra;
                }

                for (int j = 0; j < 4; j++)
                {
                    if (_changeColorTop)
                    {
                        currParts[clothesindex].colorInfo[j].baseColor = _colorDict[key][j];
                        currParts[clothesindex].colorInfo[j].patternColor = _colorDict[key][j + 4];
                    }
                    if (_changeColorBottom)
                    {
                        currParts[clothesindex + 1].colorInfo[j].baseColor = _colorDict[key + 1][j];
                        currParts[clothesindex + 1].colorInfo[j].patternColor = _colorDict[key + 1][j + 4];
                    }
                }
            }
        }

        private static void LoadColorsFromCharacter()
        {
            if (_currClassData.data == null) return;
            ChaFileControl chaFile = _currClassData.data.charFile;
            List<int> outfitsToChange = GetOutfitsToChange();
            foreach (int outfit in outfitsToChange)
            {
                ChaFileClothes.PartsInfo[] currParts = chaFile.coordinate[outfit].clothes.parts;
                for (int i = 0; i < 4; i++)
                {
                    ColorKeys key;
                    int clothesindex;

                    if (outfit == Outfits.School || outfit == Outfits.GoingHome)
                    {
                        key = ColorKeys.MainTop;
                        clothesindex = Clothes.Top;
                    }
                    else if (outfit == Outfits.PE)
                    {
                        key = ColorKeys.PETop;
                        clothesindex = Clothes.Top;
                    }
                    else
                    {
                        key = ColorKeys.SwimsuitTop;
                        clothesindex = Clothes.Bra;
                    }
                    if (_changeColorTop)
                    {
                        _colorDict[key][i] = currParts[clothesindex].colorInfo[i].baseColor;
                        _colorDict[key][i + 4] = currParts[clothesindex].colorInfo[i].patternColor;
                    }
                    if (_changeColorBottom)
                    {
                        _colorDict[key + 1][i] = currParts[clothesindex + 1].colorInfo[i].baseColor;
                        _colorDict[key + 1][i + 4] = currParts[clothesindex + 1].colorInfo[i].baseColor;
                    }
                }
            }
            Utils.Sound.Play(SystemSE.sel);
        }

        private static void ApplySettings(Apply apply)
        {
            LoadCharList();
            int appliedNo = 0;
            int currSchoolClass = Traverse.Create(_classroomScene.classRoomList).Field("_page").GetValue<IntReactiveProperty>().Value;
            _charList.Find(x => ((SaveData.Heroine)x).fixCharaID == -10).schoolClass = 1;
            foreach (SaveData.CharaData chaData in apply == Apply.Class ? _charList.Where(c => c.schoolClass == currSchoolClass) : _charList)
            {
                if (apply == Apply.Card && _currClassData != null && chaData.schoolClassIndex != _currClassData.data.schoolClassIndex) continue;
                if (!((SaveData.Heroine)chaData).isTeacher)
                {
                    if (currSchoolClass == 4)
                    {
                        // Set cards for Fixed Characters
                        SetRandomClothes(_currClassData.data.charFile);
                        appliedNo++;
                        if (apply == Apply.Card) break;
                    }
                    else
                    {
                        SetRandomClothes(chaData.charFile);
                        appliedNo++;
                    }
                }
                if (chaData.chaCtrl != null) chaData.chaCtrl.chaFile.coordinate = chaData.charFile.coordinate;
            }
            _charList.Find(x => ((SaveData.Heroine)x).fixCharaID == -10).schoolClass = -1;
            SaveData.Player player = Singleton<Manager.Game>.Instance.Player;
            if (apply != Apply.Card)
                if (apply == Apply.All || currSchoolClass == player.schoolClass)
                {
                    SetColors(player.charFile);
                    appliedNo++;
                    if (player.chaCtrl != null)
                    {
                        player.chaCtrl.chaFile.coordinate = player.charFile.coordinate;
                    }
                }
            Utils.Sound.Play(SystemSE.ok_l);
            Logger.Log(BepInEx.Logging.LogLevel.Message, String.Format("Successfully changed clothes for {0} characters!", appliedNo));

        }
    }
}
