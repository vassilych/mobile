using System;
using System.Collections.Generic;
using System.Linq;
using SplitAndMerge;

namespace scripting
{
  public class AutoScaleFunction : ParserFunction
  {
    public static int MIN_WIDTH = 640;

    public static double ScaleX { get; private set; }
    public static double ScaleY { get; private set; }

    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      ScaleX = Utils.GetSafeDouble(args, 0);
      ScaleY = Utils.GetSafeDouble(args, 1, ScaleX);

      return Variable.EmptyInstance;
    }

    public static double GetScale(double configOverride, bool isWidth)
    {
      if (configOverride != 0.0) {
        return configOverride;
      }
      return isWidth ? ScaleX : ScaleY;
    }
    public static void TransformSizes(ref int width, ref int height, int screenWidth, string option, double extra = 0.0)
    {
      if (!string.IsNullOrWhiteSpace(option) && option != "auto") {
        return;
      }
      if (extra == 0.0) {
        extra = ScaleX;
        if (extra == 0.0) {
          return;
        }
      }

      int newWidth = TransformSize(width, screenWidth, extra);
      if (width != 0) {
        double ratio = (double)newWidth / (double)width;
        height = (int)(height * ratio);
      } else {
        height = TransformSize(height, screenWidth, extra);
      }
      width = newWidth;

      return;
    }
    public static int TransformSize(int size, int screenWidth, double extra = 1.0)
    {
      if (screenWidth <= MIN_WIDTH) {
        return size;
      }
      return (int)(size * screenWidth * extra / MIN_WIDTH);
    }
  }
  public class InvokeNativeFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string methodName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, methodName, m_name);
      script.MoveForwardIf(Constants.NEXT_ARG);

      string paramName = Utils.GetToken(script, Constants.NEXT_ARG_ARRAY);
      Utils.CheckNotEmpty(script, paramName, m_name);
      script.MoveForwardIf(Constants.NEXT_ARG);

      Variable paramValueVar = Utils.GetItem(script);
      string paramValue = paramValueVar.AsString();

      var result = Utils.InvokeCall(typeof(Statics),
                                    methodName, paramName, paramValue);
      return result;
    }
  }
  public class GetRandomFunction : ParserFunction
  {
    static Random m_random = new Random();

    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);
      int limit = args[0].AsInt();
      Utils.CheckPosInt(args[0]);
      int numberRandoms = Utils.GetSafeInt(args, 1, 1);

      if (numberRandoms <= 1) {
        return new Variable(m_random.Next(0, limit));
      }

      List<int> available = Enumerable.Range(0, limit).ToList();
      List<Variable> result = new List<Variable>();

      for (int i = 0; i < numberRandoms && available.Count > 0; i++) {
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
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      string id = Utils.GetSafeString(args, 0);

      Trie trie = null;
      if (m_tries.TryGetValue(id, out trie)) {
        return trie;
      }
      Variable data = Utils.GetSafeVariable(args, 1, null);
      Utils.CheckNotNull(data, m_name);
      Utils.CheckNotNull(data.Tuple, m_name);

      List<string> words = new List<string>();
      for (int i = 0; i < data.Tuple.Count; i++) {
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
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      Trie trie = Utils.GetSafeVariable(args, 0, null) as Trie;
      Utils.CheckNotNull(trie, m_name);
      string text = args[1].AsString();
      int max = Utils.GetSafeInt(args, 2, 7);

      List<WordHint> words = new List<WordHint>();

      trie.Search(text, max, words);

      List<Variable> results = new List<Variable>(words.Count);
      foreach (WordHint word in words) {
        results.Add(new Variable(word.Id));
      }

      return new Variable(results);
    }
  }
}
