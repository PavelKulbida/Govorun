using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using NAudio.Wave;

namespace Govorun
{
  public sealed class Speaker : ISpeaker
  {
    private readonly string zipPath;
    private readonly HashSet<string> knownPhonems;

    public Speaker(string file)
    {
      zipPath = file;

      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      knownPhonems = LoadPhonems(zipPath);
    }

    public void Speak(string text)
    {
      string normalizedText = Normalize(text);

      List<string> textPhonems = BuildPhonems(normalizedText, knownPhonems);

      PlayPhonems(zipPath, textPhonems);
    }




    private HashSet<string> LoadPhonems(string zipFilePath)
    {
      var ret = new HashSet<string>();

      using (ZipArchive archive = new ZipArchive(File.OpenRead(zipFilePath), ZipArchiveMode.Read, false, Encoding.GetEncoding("cp866")))
      {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
          ret.Add(Path.GetFileNameWithoutExtension(entry.FullName));
        }
      }

      return ret;
    }

    private string Normalize(string text)
    {
      text = text.ToLower();
      text = MassReplace(text, digitReplacer);
      text = MassReplace(text, englishReplacer);

      const string knownChars = "ёйцукенгшщзхъфывапролджэячсмитьбю";
      var sb = new StringBuilder();

      foreach (char item in text)
      {
        if (knownChars.Contains(item))
        {
          sb.Append(item);
        }
        else
        {
          sb.Append(' ');
        }
      }

      return sb.ToString();
    }

    private string MassReplace(string text, Dictionary<string, string> replacer)
    {
      foreach(var item in replacer)
      {
        text = text.Replace(item.Key, item.Value);
      }

      return text;
    }

    private List<string> BuildPhonems(string normalizedText, HashSet<string> knownPhonems)
    {
      string[] sepor = new string[] { " " };
      var words = normalizedText.Split(sepor, StringSplitOptions.RemoveEmptyEntries);
      var textPhonems = new List<string>();

      foreach (var item in words)
      {
        var itemPhonems = string.Empty;
        GetKnownPhonems(item, ref itemPhonems, knownPhonems);

        if (!string.IsNullOrEmpty(itemPhonems))
        {
          textPhonems.Add(itemPhonems);
        }
      }

      return textPhonems;
    }

    private void GetKnownPhonems(string text, ref string textPhonems, HashSet<string> knownPhonems)
    {
      if (string.IsNullOrEmpty(text))
      {
        return;
      }

      if (knownPhonems.Contains(text))
      {
        if (string.IsNullOrEmpty(textPhonems))
        {
          textPhonems = text;          
        }
        else
        {
          textPhonems = string.Concat(textPhonems, "-", text);
        }

        return;
      }

      const string vowels = "яиюэоаыйуеь";
      bool isCharVowel;
      bool lastVowelCheckResult;
      int sc;

      lastVowelCheckResult = isCharVowel = false;

      for (sc = 0; sc < text.Length; sc++)
      {
        isCharVowel = vowels.Contains(text[sc]);

        if (sc > 0  && isCharVowel != lastVowelCheckResult)
        {
          break;
        }

        lastVowelCheckResult = isCharVowel;
      }

      if (sc != text.Length)
      {
        sc++;
      }

      var textPhonem = text.Substring(0, sc);
      var textRest = text.Substring(sc);

      GetPhonems(textPhonem, ref textPhonems, knownPhonems);

      GetKnownPhonems(textRest, ref textPhonems, knownPhonems);
    }

    private void GetPhonems(string text, ref string textPhonems, HashSet<string> knownPhonems)
    {
      if (string.IsNullOrEmpty(text))
      {
        return;
      }

      for (int sc = text.Length; sc > 0; sc--)
      {
        var subText = text.Substring(0, sc);

        if (knownPhonems.Contains(subText))
        {
          if (!string.IsNullOrEmpty(textPhonems))
          {
            textPhonems = string.Concat(textPhonems, "-");
          }

          textPhonems = string.Concat(textPhonems, subText);
          
          GetPhonems(text.Substring(sc), ref textPhonems, knownPhonems);

          return;
        }
      }

      GetPhonems(text.Substring(1), ref textPhonems, knownPhonems);
    }

    private void PlayPhonems(string zipFilePath, List<string> textPhonems)
    {
      using (ZipArchive archive = new ZipArchive(File.OpenRead(zipFilePath), ZipArchiveMode.Read, false, Encoding.GetEncoding("cp866")))
      using (var outputDevice = new WaveOutEvent())
      {
        foreach (var item in textPhonems)
        {
          var files = item.Split('-').Select(x => x + ".wav");

          foreach (var file in files)
          {
            var entry = archive.Entries.First(x => x.FullName == file);

            using (var streamMain = entry.Open())
            using (var ms = new MemoryStream())
            {
              streamMain.CopyTo(ms);
              ms.Seek(0, SeekOrigin.Begin);

              using (var audioFile = new WaveFileReader(ms))
              {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                  Thread.Sleep(10);
                }
              }
            }
          }

          Thread.Sleep(100);
        }
      }
    }

    private Dictionary<string, string> digitReplacer = new Dictionary<string, string>()
    {
      { "0", "ноль " },
      { "1", "один " },
      { "2", "два " },
      { "3", "три " },
      { "4", "четыре " },
      { "5", "пять " },
      { "6", "шесть " },
      { "7", "семь " },
      { "8", "восемь " },
      { "9", "девять " },
    };

    private Dictionary<string, string> englishReplacer = new Dictionary<string, string>()
    {
      { "a", "э" },
      { "b", "б" },
      { "c", "ц" },
      { "d", "д" },
      { "e", "и" },
      { "f", "ф" },
      { "g", "г" },
      { "h", "х" },
      { "i", "и" },
      { "j", "ж" },
      { "k", "к" },
      { "l", "л" },
      { "m", "м" },
      { "n", "н" },
      { "o", "о" },
      { "p", "п" },
      { "q", "кью" },
      { "r", "р" },
      { "s", "с" },
      { "t", "т" },
      { "u", "у" },
      { "v", "в" },
      { "w", "в" },
      { "x", "х" },
      { "y", "у" },
      { "z", "з" },
    };

  }
}
