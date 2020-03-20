using Govorun;

namespace GovorunPavel
{
  public class SpeakerPavel : ISpeaker
  {
    private readonly ISpeaker speakerEngine;

    public SpeakerPavel()
    {
      speakerEngine = new Speaker("phonems.zip");
    }

    public SpeakerPavel(ISpeaker speaker)
    {
      speakerEngine = speaker;
    }

    public void Speak(string text)
    {
      speakerEngine.Speak(text);
    }
  }
}
