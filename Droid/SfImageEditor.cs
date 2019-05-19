using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Content;
using Android.Provider;
using Java.IO;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;

using Syncfusion.SfImageEditor.Android;
using SplitAndMerge;
using System.Collections.Generic;

namespace scripting.Droid
{
  public class ImageEditor : DroidVariable
  {
    internal static int SELECT_FROM_GALLERY = 50;
    internal static int SELECT_FROM_CAMERA = 51;

    static ImageEditor Instance;

    static Intent mainIntent;
    static Android.Net.Uri m_imageCaptureUri;

    internal string Path { get; set; }
    internal Context Context { get; set; }
    internal SfImageEditor Editor { get; set; }

    string m_data;
    int m_width;
    int m_height;

    internal bool m_editing;
    internal string m_actionStarting;
    internal string m_actionDoneEdit;

    public ImageEditor()
    {}

    public ImageEditor(string name, string text, Context context,
               int width = 0, int height = 0) :
    base(UIType.CUSTOM, name)
    {
      m_data = text;
      Context = context;
      m_width = width;
      m_height = height;

      Editor = new SfImageEditor(Context);
      Editor.ImageSaved += (sender, args) => {
        if (!string.IsNullOrEmpty(m_actionDoneEdit)) {
          UIVariable.GetAction(m_actionDoneEdit, WidgetName, args.Location);
        }
        m_editing = false;
      };
      Editor.EndReset += (sender, args) => {
        if (m_editing) {
          if (!string.IsNullOrEmpty(m_actionDoneEdit)) {
            UIVariable.GetAction(m_actionDoneEdit, WidgetName, "");
          }
          //ActionDelegate?.Invoke(WidgetName, "");
          m_editing = false;
        }
      };
      Editor.Visibility = ViewStates.Invisible;
      ViewX = Editor;

      Instance = this;
    }

    public override void AddAction(string varName,
                                   string strAction, string argument = "")
    {
      if (argument == "start") {
        m_actionStarting = strAction;
      } else {
        m_actionDoneEdit = strAction;
      }
    }
    public void Start(string arg)
    {
      arg = arg.ToLower();
      if (arg == "camera") {
        UploadFromCamera();
      } else {
        UploadFromGallery();
      }
    }

    public void UploadFromCamera()
    {
      mainIntent = new Intent(MediaStore.ActionImageCapture);
      m_imageCaptureUri = Android.Net.Uri.FromFile(new File(CreateDirectoryForPictures(),
                                      string.Format("ImageEditor_Photo_{0}.jpg",
                                      DateTime.Now.ToString("yyyyMMddHHmmssfff"))));

      mainIntent.PutExtra(MediaStore.ExtraOutput, m_imageCaptureUri);

      Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
      mediaScanIntent.SetData(m_imageCaptureUri);
      MainActivity.TheView.SendBroadcast(mediaScanIntent);

      try {
        mainIntent.PutExtra("return-data", false);
        MainActivity.TheView.StartActivityForResult(mainIntent, SELECT_FROM_CAMERA);
      } catch (Exception ex) {
        System.Console.WriteLine("Exception: " + ex);
      }
    }
    public void UploadFromGallery()
    {
      mainIntent = new Intent();
      mainIntent.SetType("image/*");
      mainIntent.SetAction(Intent.ActionGetContent);
      MainActivity.TheView.StartActivityForResult(Intent.CreateChooser(mainIntent, "Select Picture"),
                                                  SELECT_FROM_GALLERY);
    }

   public static void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
      if (data == null) {
        data = mainIntent;
      }
      if ((resultCode != Result.Ok) || (data == null)) {
        return;
      }
      if (resultCode == Result.Ok) {
        var uri = data.Data;
        ImageEditor.Instance.m_editing = true;
        if (!string.IsNullOrEmpty(ImageEditor.Instance.m_actionStarting)) {
          UIVariable.GetAction(ImageEditor.Instance.m_actionStarting, ImageEditor.Instance.WidgetName, "");
        }
        if (requestCode == SELECT_FROM_GALLERY) {
          try {
            ImageEditor.Instance.Path = GetPathToImage(uri);
            ImageEditor.Instance.Editor.Bitmap = BitmapFactory.DecodeFile(ImageEditor.Instance.Path);

            //MainActivity.TheView.StartActivity(typeof(SfImageEditorActivity));
          } catch (Exception ex) {
            System.Console.WriteLine("Exception: " + ex);
          }
        } else if (requestCode == SELECT_FROM_CAMERA) {
          try {
            mainIntent.PutExtra("image-path", m_imageCaptureUri.Path);
            mainIntent.PutExtra("scale", true);
            ImageEditor.Instance.Path = m_imageCaptureUri.Path;
            ImageEditor.Instance.Editor.Bitmap = BitmapFactory.DecodeFile(ImageEditor.Instance.Path);
            //MainActivity.TheView.StartActivity(typeof(SfImageEditorActivity));
          } catch (Exception ex) {
            System.Console.WriteLine("Exception: " + ex);
          }
        }
      }
    }

    static string GetPathToImage(Android.Net.Uri uri)
    {
      string imgId = "";
      using (var c1 = MainActivity.TheView.ContentResolver.Query(uri, null, null, null, null)) {
        c1.MoveToFirst();
        string imageId = c1.GetString(0);
        imgId = imageId.Substring(imageId.LastIndexOf(":") + 1);
      }

      string path = null;

      string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
      using (var cursor = MainActivity.TheView.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { imgId }, null)) {
        if (cursor == null) return path;
        var columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
        cursor.MoveToFirst();
        path = cursor.GetString(columnIndex);
      }
      return path;
    }
    private File CreateDirectoryForPictures()
    {
      var dir = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(
          Android.OS.Environment.DirectoryPictures), "ImageEditor");
      if (!dir.Exists()) {
        dir.Mkdirs();
      }

      return dir;
    }
  }

  public class StartSfImageEditor : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 1, m_name);

      string varName = Utils.GetSafeString(args, 0);
      ImageEditor editor = Utils.GetVariable(varName, script) as ImageEditor;
      Utils.CheckNotNull(editor, m_name);

      string initArg = Utils.GetSafeString(args, 1);
      editor.Start(initArg);

      return editor;
    }
  }

 /* [Activity(Label = "SfImageEditor", ScreenOrientation = ScreenOrientation.Portrait,
      Theme = "@style/PropertyApp", Icon = "@drawable/icon")]
  public class SfImageEditorActivity : Activity
  {
    protected override void OnCreate(Bundle savedInstanceState)
    {
      SfImageEditor editor = new SfImageEditor(ImageEditor.Context);
      editor.Bitmap = BitmapFactory.DecodeFile(ImageEditor.Path);
      SetContentView(editor);

      base.OnCreate(savedInstanceState);
    }
  }*/
}
