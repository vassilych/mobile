using System;

using CoreGraphics;
using Foundation;
using UIKit;

using Syncfusion.SfImageEditor.iOS;
using SplitAndMerge;
using System.Collections.Generic;

namespace scripting.iOS
{
  public class ImageEditor : iOSVariable
  {
    public enum ImageType
    {
      LIBRARY, CAMERA
    };
    SfImageEditor m_sfImageEditor;
    UIImagePickerController m_imagePicker;
    string m_data;
    CGRect m_rect;

    bool m_editing;
    string m_actionStarting;
    string m_actionDoneEdit;

    public ImageEditor()
    {
    }
    public ImageEditor(string name, string initArg, CGRect rect) :
      base(UIType.CUSTOM, name)
    {
      m_data = initArg;
      m_rect = rect;

      m_sfImageEditor = new SfImageEditor(m_rect);
      m_sfImageEditor.ImageSaved += (sender, args) => {
        if (!string.IsNullOrEmpty(m_actionDoneEdit)) {
          UIVariable.GetAction(m_actionDoneEdit, WidgetName, "\"" + args.Location + "\"");
        }
        m_editing = false;
      };
      m_sfImageEditor.EndReset += (sender, args) => {
        if (m_editing) {
          if (!string.IsNullOrEmpty(m_actionDoneEdit)) {
            UIVariable.GetAction(m_actionDoneEdit, WidgetName, "");
          }
          //ActionDelegate?.Invoke(WidgetName, "");
          m_editing = false;
        }
      };
      m_sfImageEditor.Hidden = true;
      ViewX = m_sfImageEditor;
    }
    public override Variable Clone()
    {
      ImageEditor newVar = (ImageEditor)this.MemberwiseClone();
      return newVar;
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

    void UploadFromCamera()
    {
      m_imagePicker = new UIImagePickerController();
      m_imagePicker.SourceType = UIImagePickerControllerSourceType.Camera;

      m_imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(
        UIImagePickerControllerSourceType.Camera);

      m_imagePicker.FinishedPickingMedia += ImagePicker_FinishedPickingMedia; ;
      m_imagePicker.Canceled += (sender, evt) => {
        m_imagePicker.DismissModalViewController(true);
      };

      var window = UIApplication.SharedApplication.KeyWindow;
      UINavigationController navigationController = window.RootViewController as UINavigationController;
      //UINavigationController navigationController = new UINavigationController(new iOSApp());
      //window.RootViewController = navigationController;
      //window.MakeKeyAndVisible();

      navigationController.PresentViewController(m_imagePicker, true, null);
      //AppDelegate.GetCurrentController().ShowViewController(m_imagePicker, null);
    }
    void UploadFromGallery()
    {
      m_imagePicker = new UIImagePickerController();
      m_imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;

      m_imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(
        UIImagePickerControllerSourceType.PhotoLibrary);

      m_imagePicker.FinishedPickingMedia += ImagePicker_FinishedPickingMedia; ;
      m_imagePicker.Canceled += (sender, evt) => {
        m_imagePicker.DismissModalViewController(true);
      };

      AppDelegate.GetCurrentController().ShowViewController(m_imagePicker, null);
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

    void ImagePicker_FinishedPickingMedia(object sender,
                                          UIImagePickerMediaPickedEventArgs e)
    {
      m_imagePicker.DismissModalViewController(true);
      var path = e.MediaUrl;
      m_sfImageEditor.Image = e.OriginalImage;
      m_sfImageEditor.Hidden = false;

      m_editing = true;
      if (!string.IsNullOrEmpty(m_actionStarting)) {
        UIVariable.GetAction(m_actionStarting, WidgetName, "");
      }
      //AppDelegate.GetCurrentController().ShowViewController(
      //  new ImageEditorViewController(e.OriginalImage), null);
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
}
