using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Verification;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FinalProject
{
    class VerificationController
    {
        private SpeakerVerificationServiceClient serviceClient;

        public VerificationController()
        {
            serviceClient = new SpeakerVerificationServiceClient(Keys.subscriptionKey);
        }

        public async Task<Verification> VerifySpeaker(Guid speakerId)
        {
            //Get recorded .wav file
            var audioStream = await Recorder.GetStreamAsync();

            try
            {
                //Verify user using .wav file and his speakerID
                Verification verificationResult = await serviceClient.VerifyAsync(audioStream.AsStream(), speakerId);
                Debug.WriteLine(verificationResult.Phrase);
                return verificationResult;
            }
            catch (VerificationException ve)
            {
                if (ve.Message.Equals("SpeechNotRecognized"))
                {
                    Synthesizer.Speak("I am sorry, I couldn't recognize a human's voice. Please try again.");
                    Debug.WriteLine("I am sorry, I couldn't recognize a human's voice. Please try again.");
                }
                else
                {
                    Debug.WriteLine("Couldn't verify Speaker: " + ve.Message);
                    Synthesizer.Speak("Something went wrong. Please try again.");
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Couldn't verify Speaker: " + e.Message);
                Synthesizer.Speak("Something went wrong. Please try again.");
                return null;
            }
        }
    }
}
