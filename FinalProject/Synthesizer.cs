using System;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;

namespace FinalProject
{
    public static class Synthesizer
    {
        public static async void Speak(String toSay)
        {
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            MediaElement mediaElement = new MediaElement();
            var synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(toSay);
            mediaElement.SetSource(synthesisStream, synthesisStream.ContentType);
            mediaElement.Play();
        }
    }
}
