using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Decoder
{
  class Program
  {
    private static IDictionary<string, string> _coder = new Dictionary<string, string>()
    {
      {"а","a"},      {"б","b"},      {"в","w"},
      {"г","g"},      {"д","d"},      {"е","e"},
      {"ё","e1"},     {"ж","j"},      {"з","z"},
      {"и","i"},      {"й","i1"},     {"к","k"},
      {"л","l"},      {"м","m"},      {"н","n"},
      {"о","o"},      {"п","p"},      {"р","r"},
      {"с","s"},      {"т","t"},      {"у","u"},
      {"ф","f"},      {"х","x"},      {"ц","c"},
      {"ч","c1"},     {"ш","h"},      {"щ","h1"},
      {"ъ","("},      {"ы","y"},      {"ь",")"},
      {"э","a1"},     {"ю","v"},      {"я","a2"},
    };

    static void Main(string[] args)
    {
      if (!string.IsNullOrEmpty(args[0]))
      {
        var decoder = _coder.ToDictionary(x => x.Value, y => y.Key)
          .Reverse();

        var files = Directory.GetFiles(args[0]);

        foreach (string file in files)
        {
          var fileName = Path.GetFileNameWithoutExtension(file);
          var decodedFileName = fileName.ToLower();

          foreach (var decoderItem in decoder)
          {
            decodedFileName = decodedFileName.Replace(decoderItem.Key, decoderItem.Value);
          }

          var decodedFile = file.Replace($"\\{fileName}.", $"\\{decodedFileName}.");

          Console.WriteLine($"{file} -> {decodedFile}");
          File.Move(file, decodedFile);
        }
      }
      else
      {
        Console.WriteLine("Give me a path!");
      }
    }
  }
}
