﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using SplitAndMerge;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if __ANDROID__
using scripting.Droid;
#elif __IOS__
using scripting.iOS;
#endif

namespace scripting
{
    public class CommonFunctions
    {
        public static void RegisterFunctions()
        {
            ParserFunction.RegisterFunction("GetLocation", new GetLocationFunction());
            ParserFunction.RegisterFunction("AddWidget", new AddWidgetFunction());
            ParserFunction.RegisterFunction("AddView", new AddWidgetFunction("View"));
            ParserFunction.RegisterFunction("AddButton", new AddWidgetFunction("Button"));
            ParserFunction.RegisterFunction("AddLabel", new AddWidgetFunction("Label"));
            ParserFunction.RegisterFunction("AddTextEdit", new AddWidgetFunction("TextEdit"));
            ParserFunction.RegisterFunction("AddTextView", new AddWidgetFunction("TextView"));
            ParserFunction.RegisterFunction("AddTextEditView", new AddWidgetFunction("TextEditView"));
            ParserFunction.RegisterFunction("AddImageView", new AddWidgetFunction("ImageView"));
            ParserFunction.RegisterFunction("AddPickerView", new AddWidgetFunction("Picker"));
            ParserFunction.RegisterFunction("AddTypePickerView", new AddWidgetFunction("TypePicker"));
            ParserFunction.RegisterFunction("AddSwitch", new AddWidgetFunction("Switch"));
            ParserFunction.RegisterFunction("AddSlider", new AddWidgetFunction("Slider"));
            ParserFunction.RegisterFunction("AddStepper", new AddWidgetFunction("Stepper"));
            ParserFunction.RegisterFunction("AddStepperLeft", new AddWidgetFunction("Stepper", "left"));
            ParserFunction.RegisterFunction("AddStepperRight", new AddWidgetFunction("Stepper", "right"));
            ParserFunction.RegisterFunction("AddListView", new AddWidgetFunction("ListView"));
            ParserFunction.RegisterFunction("AddCombobox", new AddWidgetFunction("Combobox"));
            ParserFunction.RegisterFunction("AddIndicator", new AddWidgetFunction("Indicator"));
            ParserFunction.RegisterFunction("AddSegmentedControl", new AddWidgetFunction("SegmentedControl"));

            ParserFunction.RegisterFunction("AddWidgetData", new AddWidgetDataFunction());
            ParserFunction.RegisterFunction("AddWidgetImages", new AddWidgetImagesFunction());
            ParserFunction.RegisterFunction("AddTab", new AddTabFunction(true));
            ParserFunction.RegisterFunction("AddOrSelectTab", new AddTabFunction(false));
            ParserFunction.RegisterFunction("GetSelectedTab", new GetSelectedTabFunction());
            ParserFunction.RegisterFunction("SelectTab", new SelectTabFunction());
            ParserFunction.RegisterFunction("OnTabSelected", new OnTabSelectedFunction());
            ParserFunction.RegisterFunction("AddBorder", new AddBorderFunction());
            ParserFunction.RegisterFunction("AutoScale", new AutoScaleFunction());
            ParserFunction.RegisterFunction("SetBaseWidth", new SetBaseWidthFunction());
            ParserFunction.RegisterFunction("SetBaseHeight", new SetBaseHeightFunction());
            ParserFunction.RegisterFunction("AddLongClick", new AddLongClickFunction());
            ParserFunction.RegisterFunction("AddSwipe", new AddSwipeFunction());
            ParserFunction.RegisterFunction("AddDragAndDrop", new AddDragAndDropFunction());
            ParserFunction.RegisterFunction("ShowView", new ShowHideFunction(true));
            ParserFunction.RegisterFunction("HideView", new ShowHideFunction(false));
            ParserFunction.RegisterFunction("SetVisible", new ShowHideFunction(true));
            ParserFunction.RegisterFunction("RemoveView", new RemoveViewFunction());
            ParserFunction.RegisterFunction("RemoveViewIfExists", new RemoveViewIfExistsFunction());
            ParserFunction.RegisterFunction("RemoveAllViews", new RemoveAllViewsFunction());
            ParserFunction.RegisterFunction("RemoveTabViews", new RemoveAllViewsFunction());
            ParserFunction.RegisterFunction("GetX", new GetCoordinateFunction(true));
            ParserFunction.RegisterFunction("GetY", new GetCoordinateFunction(false));
            ParserFunction.RegisterFunction("MoveView", new MoveViewFunction(false));
            ParserFunction.RegisterFunction("MoveViewTo", new MoveViewFunction(true));
            ParserFunction.RegisterFunction("SetBackgroundColor", new SetBackgroundColorFunction());
            ParserFunction.RegisterFunction("SetBackground", new SetBackgroundImageFunction());
            ParserFunction.RegisterFunction("AddText", new AddTextFunction());
            ParserFunction.RegisterFunction("SetText", new SetTextFunction());
            ParserFunction.RegisterFunction("GetText", new GetTextFunction());
            ParserFunction.RegisterFunction("SetValue", new SetValueFunction());
            ParserFunction.RegisterFunction("GetValue", new GetValueFunction());
            ParserFunction.RegisterFunction("SetImage", new SetImageFunction());
            ParserFunction.RegisterFunction("SetFontColor", new SetFontColorFunction());
            ParserFunction.RegisterFunction("SetFontSize", new SetFontSizeFunction());
            ParserFunction.RegisterFunction("SetFont", new SetFontFunction());
            ParserFunction.RegisterFunction("SetBold", new SetFontTypeFunction(SetFontTypeFunction.FontType.BOLD));
            ParserFunction.RegisterFunction("SetItalic", new SetFontTypeFunction(SetFontTypeFunction.FontType.ITALIC));
            ParserFunction.RegisterFunction("SetNormalFont", new SetFontTypeFunction(SetFontTypeFunction.FontType.NORMAL));
            ParserFunction.RegisterFunction("AlignText", new AlignTitleFunction());
            ParserFunction.RegisterFunction("SetSize", new SetSizeFunction());
            ParserFunction.RegisterFunction("Relative", new RelativeSizeFunction());
            ParserFunction.RegisterFunction("ShowHideKeyboard", new ShowHideKeyboardFunction());
            ParserFunction.RegisterFunction("IsKeyboard", new IsKeyboardFunction());
            ParserFunction.RegisterFunction("NumKeyboard", new NumKeyboardFunction());
            ParserFunction.RegisterFunction("ClearWidget", new ClearWidgetDataFunction());

            ParserFunction.RegisterFunction("AddAction", new AddActionFunction());
            ParserFunction.RegisterFunction("AllowedOrientation", new AllowedOrientationFunction());
            ParserFunction.RegisterFunction("OnOrientationChange", new OrientationChangeFunction());
            ParserFunction.RegisterFunction("RegisterOrientationChange", new RegisterOrientationChangeFunction());
            ParserFunction.RegisterFunction("OnEnterBackground", new OnEnterBackgroundFunction());
            ParserFunction.RegisterFunction("KillMe", new KillMeFunction());
            ParserFunction.RegisterFunction("ShowToast", new ShowToastFunction());
            ParserFunction.RegisterFunction("AlertDialog", new AlertDialogFunction());
            //ParserFunction.RegisterFunction("AlertEditDialog", new AlertDialogFunction(true));
            ParserFunction.RegisterFunction("ColorPicker", new PickColorDialogFunction());
            ParserFunction.RegisterFunction("ConvertColor", new ConvertColorFunction());

            ParserFunction.RegisterFunction("Speak", new SpeakFunction());
            ParserFunction.RegisterFunction("SetupSpeech", new SpeechOptionsFunction());
            ParserFunction.RegisterFunction("VoiceRecognition", new VoiceFunction());
            ParserFunction.RegisterFunction("StopVoiceRecognition", new StopVoiceFunction());
            ParserFunction.RegisterFunction("Localize", new LocalizedFunction());
            ParserFunction.RegisterFunction("TranslateTabBar", new TranslateTabBar());
            ParserFunction.RegisterFunction("InitIAP", new InitIAPFunction());
            ParserFunction.RegisterFunction("InitTTS", new InitTTSFunction());
            ParserFunction.RegisterFunction("Purchase", new PurchaseFunction());
            ParserFunction.RegisterFunction("Restore", new RestoreFunction());
            ParserFunction.RegisterFunction("ProductIdDescription", new ProductIdDescriptionFunction());
            ParserFunction.RegisterFunction("ReadFile", new ReadFileFunction());
            ParserFunction.RegisterFunction("ReadFileAsString", new ReadFileFunction(true));
            ParserFunction.RegisterFunction("Schedule", new PauseFunction(true));
            ParserFunction.RegisterFunction("CancelSchedule", new PauseFunction(false));
            ParserFunction.RegisterFunction("GetDeviceLocale", new GetDeviceLocale());
            ParserFunction.RegisterFunction("SetAppLocale", new SetAppLocale());
            ParserFunction.RegisterFunction("GetSetting", new GetSettingFunction());
            ParserFunction.RegisterFunction("SetSetting", new SetSettingFunction());
            ParserFunction.RegisterFunction("SetStyle", new SetStyleFunction());
            ParserFunction.RegisterFunction("DisplayWidth", new GadgetSizeFunction(true));
            ParserFunction.RegisterFunction("DisplayHeight", new GadgetSizeFunction(false));
            ParserFunction.RegisterFunction("DisplayDPI", new GadgetSizeFunction(false, true));
            ParserFunction.RegisterFunction("Orientation", new OrientationFunction());
            ParserFunction.RegisterFunction("GetTrie", new CreateTrieFunction());
            ParserFunction.RegisterFunction("SearchTrie", new SearchTrieFunction());
            ParserFunction.RegisterFunction("ImportFile", new ImportFileFunction());
            ParserFunction.RegisterFunction("OpenUrl", new OpenURLFunction());
            ParserFunction.RegisterFunction("WebRequest", new WebRequestFunction());

            ParserFunction.RegisterFunction("EnableWidget", new EnableFunction());
            ParserFunction.RegisterFunction("SetSecure", new MakeSecureFunction());
            ParserFunction.RegisterFunction("SaveToPhotos", new SaveToPhotosFunction());

            //ParserFunction.RegisterFunction("AddCustomDialog", new DialogFunction());

            ParserFunction.RegisterFunction("_ANDROID_", new CheckOSFunction(CheckOSFunction.OS.ANDROID));
            ParserFunction.RegisterFunction("_IOS_", new CheckOSFunction(CheckOSFunction.OS.IOS));
            ParserFunction.RegisterFunction("_DEVICE_INFO_", new GetDeviceInfoFunction());
            ParserFunction.RegisterFunction("_VERSION_INFO_", new GetVersionInfoFunction());
            ParserFunction.RegisterFunction("_VERSION_NUMBER_", new GetVersionNumberFunction());
            ParserFunction.RegisterFunction("_DEBUG_", new IsDebugFunction());
            ParserFunction.RegisterFunction("_APPVERSION_", new AppVersionFunction());
            ParserFunction.RegisterFunction("_TOTALRAM_", new GetRamFunction());
            ParserFunction.RegisterFunction("_USEDRAM_", new AllocatedMemoryFunction());
            ParserFunction.RegisterFunction("_TOTALSPACE_", new GetStorageFunction());
            ParserFunction.RegisterFunction("_FREESPACE_", new GetStorageFunction(false));
            ParserFunction.RegisterFunction("_BUILDTIME_", new BuildTimeFunction());
            ParserFunction.RegisterFunction("CompareVersions", new CompareVersionsFunction());

            ParserFunction.RegisterFunction("Run", new RunScriptFunction());
            ParserFunction.RegisterFunction("SetOptions", new SetOptionsFunction());

            ParserFunction.RegisterFunction("GetLocalIp", new GetLocalIpFunction(true));
            ParserFunction.RegisterFunction("isiPhoneX", new IsiPhoneXFunction());
            ParserFunction.RegisterFunction("isiPhoneXR", new IsiPhoneXRFunction());
            ParserFunction.RegisterFunction("isAndroid", new IsAndroidFunction());

            ParserFunction.RegisterFunction("RunOnMain", new RunOnMainFunction());
            ParserFunction.RegisterFunction("PrintConsole", new PrintConsoleFunction());

            ParserFunction.RegisterFunction("time:year", new MyDateTimeFunction("yyyy"));
            ParserFunction.RegisterFunction("time:month", new MyDateTimeFunction("MM"));
            ParserFunction.RegisterFunction("time:day", new MyDateTimeFunction("dd"));
            ParserFunction.RegisterFunction("time:hour", new MyDateTimeFunction("HH"));
            ParserFunction.RegisterFunction("time:minute", new MyDateTimeFunction("mm"));
            ParserFunction.RegisterFunction("time:second", new MyDateTimeFunction("ss"));
            ParserFunction.RegisterFunction("time:millis", new MyDateTimeFunction("fff"));

            SQLLite.Init();
        }

        public static void RunScript(string fileName)
        {
            Interpreter.Instance.Init();
            RegisterFunctions();

#if __ANDROID__
            UIVariable.WidgetTypes.Add(new DroidVariable());
#elif __IOS__
            UIVariable.WidgetTypes.Add(new iOSVariable());
#endif

            string script = FileToString(fileName);
            Variable result = null;
            try
            {
                result = Interpreter.Instance.Process(script, fileName);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc.Message);
                Console.WriteLine(exc.StackTrace);
                ParserFunction.InvalidateStacksAfterLevel(0);
                throw;
            }
        }

        public static string FileToString(string filename)
        {
            string contents = "";
#if __ANDROID__
      Android.Content.Res.AssetManager assets = MainActivity.TheView.Assets;
      using (StreamReader sr = new StreamReader(assets.Open(filename))) {
        contents = sr.ReadToEnd();
      }
#elif __IOS__
            string[] lines = System.IO.File.ReadAllLines(filename);
            contents = string.Join("\n", lines);
#endif
            return contents;
        }

        public static void RunOnMainThread(string strAction, string arg1, string arg2 = null, string arg3 = null)
        {
#if  __ANDROID__
            scripting.Droid.MainActivity.TheView.RunOnUiThread(() =>
            {
#elif __IOS__
            scripting.iOS.AppDelegate.GetCurrentController().InvokeOnMainThread(() =>
            {
#endif
                UIVariable.GetAction(strAction, arg1, arg2, arg3);
#if __ANDROID__ || __IOS__
            });
#endif
        }

        public static void RunOnMainThread(CustomFunction callbackFunction,
            string arg1 = null, string arg2 = null, string arg3 = null)
        {
            List<Variable> args = new List<Variable>();
            if (arg1 != null)
            {
                args.Add(new Variable(arg1));
            }
            if (arg2 != null)
            {
                args.Add(new Variable(arg2));
            }
            if (arg3 != null)
            {
                args.Add(new Variable(arg3));
            }
#if __ANDROID__
            scripting.Droid.MainActivity.TheView.RunOnUiThread(() =>
            {
#elif __IOS__
            scripting.iOS.AppDelegate.GetCurrentController().InvokeOnMainThread(() =>
            {
#endif
                callbackFunction.Run(args);
#if __ANDROID__ || __IOS__
            });
#endif
        }

        public static void RunFunctionOnMainThread(ParserFunction func, ParsingScript script)
        {
#if __ANDROID__
            scripting.Droid.MainActivity.TheView.RunOnUiThread(() =>
            {
#elif __IOS__
            scripting.iOS.AppDelegate.GetCurrentController().InvokeOnMainThread(() =>
            {
#endif
                func.GetValue(script);
#if __ANDROID__ || __IOS__
            });
#endif
        }
    }

    public class RunOnMainFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string funcName = Utils.GetToken(script, Constants.NEXT_OR_END_ARRAY);
            var initPointer = script.Pointer;

            List<Variable> args = script.GetFunctionArgs();

            string arg1 = Utils.GetSafeString(args, 0, null);
            string arg2 = Utils.GetSafeString(args, 1, null);
            string arg3 = Utils.GetSafeString(args, 2, null);

            ParserFunction func = ParserFunction.GetFunction(funcName, script);
            Utils.CheckNotNull(funcName, func, script);

            if (func is CustomFunction)
            {
                CommonFunctions.RunOnMainThread(func as CustomFunction, arg1, arg2, arg3);
                return Variable.EmptyInstance;
            }

            ParsingScript tempScript = script.GetTempScript(script.String, initPointer);
            PrintConsoleFunction.Print("RunOnMain rest=" + tempScript.Rest);
            CommonFunctions.RunFunctionOnMainThread(func, tempScript);
            //Thread.Sleep(100);
            return Variable.EmptyInstance;
        }
    }

    public class RelativeSizeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            double original = Utils.GetSafeDouble(args, 0);
            double multiplier = Utils.GetSafeDouble(args, 1);
            double relative = AutoScaleFunction.TransformSizeW(original,
                                AutoScaleFunction.GetRealScreenSize(true), multiplier);

            return new Variable(relative);
        }
    }

    public class AutoScaleFunction : ParserFunction
    {
        public static int BASE_WIDTH = 640;
        public static int BASE_HEIGHT = 960;

        public static double ScaleX { get; private set; }
        public static double ScaleY { get; private set; }

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            ScaleX = Utils.GetSafeDouble(args, 0, 1.0);
            ScaleY = Utils.GetSafeDouble(args, 1, ScaleX);

            return Variable.EmptyInstance;
        }

        public static double GetScale(double configOverride, bool isWidth)
        {
            if (configOverride != 0.0)
            {
                return configOverride;
            }
            return isWidth ? ScaleX : ScaleY;
        }
        public static void TransformSizes(ref int width, ref int height,
                                          int screenWidth, double extra = 0.0)
        {
            int newWidth = (int)TransformSize(width, screenWidth, extra);
            if (width != 0)
            {
                double ratio = (double)newWidth / (double)width;
                height = (int)(height * ratio);
            }
            else
            {
                height = (int)TransformSize(height, screenWidth, extra);
            }
            width = newWidth;

            return;
        }
        public static double TransformSize(double size, int screenWidth, double extra = 0.0)
        {
            if (size == 0.0)
            {
                return size;
            }
            if (extra == 0.0)
            {
                extra = ScaleX;
                if (extra == 0.0)
                {
                    return size;
                }
            }
            //int oldSize = (int)(size * screenWidth * extra / BASE_WIDTH);
            double newSize = (size * screenWidth / BASE_WIDTH);
            double delta = (newSize - size) * extra;
            size = (size + delta);

            return size;
        }
        public static double TransformSizeW(double size, int screenWidth, double extra)
        {
            if (extra == 0.0)
            {
                extra = ScaleX;
                if (extra == 0.0)
                {
                    return size;
                }
            }
            double newSize = (size * screenWidth / BASE_WIDTH);
            size += (newSize - size) * extra;

            return size;
        }
        public static double TransformSizeH(double size, int screenHeight, double extra)
        {
            if (extra == 0.0)
            {
                extra = ScaleY;
                if (extra == 0.0)
                {
                    return size;
                }
            }
            double newSize = (size * screenHeight / BASE_HEIGHT);
            size += (newSize - size) * extra;

            return size;
        }
        public static int GetRealScreenSize(bool width = true)
        {
#if __ANDROID__
            var size = UtilsDroid.GetScreenSize();
            return width ? size.Width : size.Height;
#elif __IOS__
            return width ? (int)UtilsiOS.GetRealScreenWidth() : (int)UtilsiOS.GetRealScreenHeight();
#endif
        }

        public static float ConvertFontSize(float original, int widgetWidth)
        {
            float extra = widgetWidth > 640 || original <= 14 ? 0 : original * 0.2f;
            float newSize = original - extra;

            newSize += (widgetWidth <= 480 ? -4.5f :
                        widgetWidth <= 540 ? -4.0f :
                        widgetWidth <= 600 ? -3.0f :
                        widgetWidth <= 640 ? -2.0f :
                        widgetWidth <= 720 ? -0.5f :
                        widgetWidth <= 900 ?  0.5f :
                        widgetWidth <= 960 ?  2.0f :
                        widgetWidth <= 1024 ? 2.5f :
                        widgetWidth <= 1200 ? 3.0f :
                        widgetWidth <= 1300 ? 3.5f :
                        widgetWidth <= 1400 ? 4.0f : 4.0f);

#if __ANDROID__
            newSize--;
            if (GadgetSizeFunction.GetDPI() < 460)
            {
                newSize -= (//widgetWidth >= 1200 ? 8.0f :
                            widgetWidth >= 1000 ? 7.0f :
                            widgetWidth >= 900 ? 3.0f :
                            widgetWidth >= 800 ? 1.0f : 0f);
            }
#endif
            return newSize;
        }
    }
    public class RunScriptFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string strScript = Utils.GetSafeString(args, 0);
            Variable result = null;

            ParserFunction.StackLevelDelta++;
            try
            {
                result = Execute(strScript);
            }
            finally
            {
                ParserFunction.StackLevelDelta--;
            }

            return result != null ? result : Variable.EmptyInstance;
        }

        public static Variable Execute(string text, string filename = "")
        {
            string[] lines = text.Split(new char[] { '\n' });

            Dictionary<int, int> char2Line;
            string includeScript = Utils.ConvertToScript(text, out char2Line);
            ParsingScript tempScript = new ParsingScript(includeScript, 0, char2Line);
            tempScript.Filename = filename;
            tempScript.OriginalScript = string.Join(Constants.END_LINE.ToString(), lines);

            Variable result = null;
            while (tempScript.Pointer < includeScript.Length)
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }
            return result;
        }
    }
    public class SetBaseWidthFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            int baseWidth = Utils.GetSafeInt(args, 0);

            Utils.CheckPosInt(args[0], script);

            AutoScaleFunction.BASE_WIDTH = baseWidth;

            return new Variable(baseWidth);
        }
    }
    public class SetBaseHeightFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            int baseHeight = Utils.GetSafeInt(args, 0);

            Utils.CheckPosInt(args[0], script);

            AutoScaleFunction.BASE_HEIGHT = baseHeight;

            return new Variable(baseHeight);
        }
    }
    public class GetRandomFunction : ParserFunction
    {
        static Random m_random = new Random();

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);
            int limit = args[0].AsInt();
            Utils.CheckPosInt(args[0], script);
            int numberRandoms = Utils.GetSafeInt(args, 1, 1);

            if (numberRandoms <= 1)
            {
                return new Variable(m_random.Next(0, limit));
            }

            List<int> available = Enumerable.Range(0, limit).ToList();
            List<Variable> result = new List<Variable>();

            for (int i = 0; i < numberRandoms && available.Count > 0; i++)
            {
                int nextRandom = m_random.Next(0, available.Count);
                result.Add(new Variable(available[nextRandom]));
                available.RemoveAt(nextRandom);
            }

            return new Variable(result);
        }
    }
    public class CreateTrieFunction : ParserFunction
    {
        static Dictionary<string, Trie> m_tries = new Dictionary<string, Trie>();

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string id = Utils.GetSafeString(args, 0);

            Trie trie = null;
            if (m_tries.TryGetValue(id, out trie))
            {
                return trie;
            }
            Variable data = Utils.GetSafeVariable(args, 1, null);
            Utils.CheckNotNull(data, m_name, script);
            Utils.CheckNotNull(data.Tuple, m_name, script);

            List<string> words = new List<string>();
            for (int i = 0; i < data.Tuple.Count; i++)
            {
                words.Add(data.Tuple[i].AsString());
            }

            trie = new Trie(words);
            m_tries[id] = trie;
            return trie;
        }
    }
    public class SearchTrieFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            Trie trie = Utils.GetSafeVariable(args, 0, null) as Trie;
            Utils.CheckNotNull(trie, m_name, script);
            string text = args[1].AsString();
            int max = Utils.GetSafeInt(args, 2, 10);

            List<WordHint> words = new List<WordHint>();

            trie.Search(text, max, words);

            List<Variable> results = new List<Variable>(words.Count);
            foreach (WordHint word in words)
            {
                results.Add(new Variable(word.Id));
            }

            return new Variable(results);
        }
    }

    public class GetLocalIpFunction : ParserFunction
    {
        bool m_usePattern;

        public GetLocalIpFunction(bool usePattern = false)
        {
            m_usePattern = usePattern;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            /*List<Variable> args = */
            script.GetFunctionArgs();
            string ip = GetIPAddress();

            if (m_usePattern)
            {
                int ind = ip.LastIndexOf(".");
                if (ind > 0)
                {
                    ip = ip.Substring(0, ind) + ".*";
                }
            }

            return new Variable(ip);
        }

        public static string GetIPAddress()
        {
            string localIP = "";
            string hostname = Dns.GetHostName();

            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            localIP = addrInfo.Address.ToString();
                            break;
                        }
                    }
                }
            }
            return localIP;
        }
    }

    class AppVersionFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string version = "";

#if __ANDROID__
            var ctx = Android.App.Application.Context;
            version = ctx.ApplicationContext.PackageManager.GetPackageInfo(ctx.ApplicationContext.PackageName, 0).VersionName;
#elif __IOS__
            version = iOSApp.GetAppVersion();
#endif

            return new Variable(version);
        }
    }

    class CheckOSFunction : ParserFunction
    {
        public enum OS { NONE, IOS, ANDROID, WINDOWS_PHONE, MAC, WINDOWS };

        OS m_os;
        public CheckOSFunction(OS toCheck)
        {
            m_os = toCheck;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            bool isTheOS = false;

#if __ANDROID__
            isTheOS = m_os == OS.ANDROID;
#elif __IOS__
            isTheOS = m_os == OS.IOS;
#elif SILVERLIGHT
            isTheOS = m_os == OS.WINDOWS_PHONE;
#endif
            return new Variable(isTheOS);
        }
    }

    class BuildTimeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            var linkTimeLocal = GetLinkerTime(System.Reflection.Assembly.GetExecutingAssembly());
            return new Variable(linkTimeLocal.ToString("yyyy/MM/dd HH:mm"));
        }

        public static DateTime GetLinkerTime(System.Reflection.Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            var localTime = File.GetLastWriteTime(filePath);

            /*const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);*/

            return localTime;
        }
    }

    class GetDeviceInfoFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string deviceName = "";
#if __ANDROID__
      deviceName   = Android.OS.Build.Brand;
      string model = Android.OS.Build.Model;
      if (!model.Contains("Android")) {
        // Simulators may have "Android" in both, Brand and Model.
        deviceName += " " + model;
      }
      deviceName = deviceName.Replace("google", "Simulator");
#elif __IOS__
            deviceName = UtilsiOS.GetDeviceName();
            deviceName = deviceName.Replace("Simulator", "iPhone");
#endif
            return new Variable(deviceName);
        }
    }
    class GetVersionInfoFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string version = "";

#if __ANDROID__
      version = "Android " + Android.OS.Build.VERSION.Release + " - " + 
                Android.OS.Build.VERSION.Sdk;
#elif __IOS__
            version = UIKit.UIDevice.CurrentDevice.SystemName + " " +
                      UIKit.UIDevice.CurrentDevice.SystemVersion;
#endif
            return new Variable(version);
        }
    }
    class GetVersionNumberFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
#if __ANDROID__
            string strVersion = Android.OS.Build.VERSION.Release;
#elif __IOS__
            string strVersion = UIKit.UIDevice.CurrentDevice.SystemVersion;
#endif
            return new Variable(strVersion);
        }
    }
    public class CompareVersionsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string version1 = Utils.GetSafeString(args, 0);
            string version2 = Utils.GetSafeString(args, 1);

            int cmp = CompareVersions(version1, version2);

            return new Variable(cmp);
        }
        public static int CompareVersions(string version1, string version2)
        {
            if (version1 == version2)
            {
                return 0;
            }
            char[] sep = ".".ToCharArray();
            string[] parts1 = version1.Split(sep);
            string[] parts2 = version2.Split(sep);
            int commonParts = Math.Min(parts1.Length, parts2.Length);
            for (int i = 0; i < commonParts; i++)
            {
                int cmp = Compare(parts1[i], parts2[i]);
                if (cmp != 0)
                {
                    return cmp;
                }
            }
            return parts1.Length < parts2.Length ? -1 : 1;
        }
        public static int Compare(string part1, string part2)
        {
            if (part1.Length == part2.Length)
            {
                return string.Compare(part1, part2);
            }
            return part1.Length < part2.Length ? -1 : 1;
        }
    }

    class IsiPhoneXFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
#if __ANDROID__
            bool isiPhoneX = false;
#elif __IOS__
            bool isiPhoneX = UtilsiOS.IsiPhoneX();
#endif
            return new Variable(isiPhoneX);
        }
    }

    class IsiPhoneXRFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
#if __ANDROID__
            bool isiPhoneXR = false;
#elif __IOS__
            bool isiPhoneXR = UtilsiOS.IsiPhoneXR();
#endif
            return new Variable(isiPhoneXR);
        }
    }

    class IsDebugFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
#if DEBUG
            return new Variable(true);
#else
            return new Variable(false);
#endif
        }
    }

    class GetRamFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {

#if __ANDROID__
            var total = MainActivity.GetTotalMemory() / (1024 * 1024);
#else
            var total = Foundation.NSProcessInfo.ProcessInfo.PhysicalMemory / (1024 * 1024);
#endif
            return new Variable(total);
        }
    }


    class GetStorageFunction : ParserFunction
    {
        bool m_isTotal;
        public GetStorageFunction(bool isTotal = true)
        {
            m_isTotal = isTotal;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            long total, free;
#if __ANDROID__

            MainActivity.GetStorage(out total, out free);
#else
            UtilsiOS.GetStorage(out total, out free);
#endif
            var result = (m_isTotal ? total : free) / (1024 * 1024);
            return new Variable(result);
        }
    }

    class AllocatedMemoryFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            return new Variable(GC.GetTotalMemory(false) / (1024 * 1024));
        }
    }

    class IsAndroidFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
#if __ANDROID__
            bool isAndroid = true;
#elif __IOS__
            bool isAndroid = false;
#endif
            return new Variable(isAndroid);
        }
    }

    public class PrintConsoleFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                System.Diagnostics.Debug.Write(arg.ToString());
            }
            System.Diagnostics.Debug.WriteLine("");
            return Variable.EmptyInstance;
        }
        public static void Print(string msg)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            System.Diagnostics.Debug.WriteLine(timestamp + " " + Environment.CurrentManagedThreadId + " " + msg);
        }
    }
    public class MyDateTimeFunction : ParserFunction
    {
        string m_format;
        public MyDateTimeFunction(string format)
        {
            m_format = format;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return new Variable(DateTime.Now.ToString(m_format));
        }
    }
}
