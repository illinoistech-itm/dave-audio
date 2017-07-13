using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FinalProject
{
    public class Recorder
    {
        AudioGraph graph;
        AudioFileOutputNode outputNode;

        public async Task StartRecording()
        {
            //Create file in ApplicationData LocalFolder
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("MyVoice.wav", CreationCollisionOption.ReplaceExisting);
                if (file != null)
                {
                    //Create AudioGraph for Speech audio
                    var result = await AudioGraph.CreateAsync(
                           new AudioGraphSettings(AudioRenderCategory.Speech));

                    if (result.Status == AudioGraphCreationStatus.Success)
                    {
                        this.graph = result.Graph;

                        var microphone = await DeviceInformation.CreateFromIdAsync(
                          MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Default));

                        //.WAV file AudioQuality.Low gives only 1 channel
                        var outProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
                        //PCM encoding, 16k rate, Monochannel, 16 bit per sample
                        outProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);

                        //Create File Output Node
                        var outputResult = await this.graph.CreateFileOutputNodeAsync(file, outProfile);

                        if (outputResult.Status == AudioFileNodeCreationStatus.Success)
                        {
                            this.outputNode = outputResult.FileOutputNode;

                            var inProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);

                            var inputResult = await this.graph.CreateDeviceInputNodeAsync(
                            MediaCategory.Speech,
                            this.graph.EncodingProperties,
                            microphone);

                            if (inputResult.Status == AudioDeviceNodeCreationStatus.Success)
                            {
                                inputResult.DeviceInputNode.AddOutgoingConnection(this.outputNode);

                                this.graph.Start();
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.WriteLine("Unauthorized exception when recording " + e.Message);
            }
        }

        public async void StopRecording()
        {
            if (this.graph != null)
            {
                this.graph?.Stop();

                await this.outputNode.FinalizeAsync();


                this.graph?.Dispose();

                this.graph = null;
            }
        }

        public static async Task<IRandomAccessStreamWithContentType> GetStreamAsync()
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.GetFileAsync("MyVoice.wav");
            var stream = await file.OpenReadAsync();
            return stream;
        }
    }
}
