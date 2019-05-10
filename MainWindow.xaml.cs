using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Recognition;
using Grpc.Auth;
using System.Net;
using System.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;
using Google.Cloud.TextToSpeech.V1;
using System.Windows.Media;
using Google.Cloud.Speech.V1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using NAudio.Wave;
using Google.Cloud.Translation.V2;
using System.Threading;

namespace Selami
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window 
    {
     
        SpeechClient speech;
        TextToSpeechClient client;
        string yazi;
        DispatcherTimer timer;
        string kisi = "Kişi";
        private MediaPlayer mediaPlayer = new MediaPlayer();
        bool tamam = false;
        List<string> komutlar;
        CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            DragMove();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            string credential_path = @"C:\Users\corx\source\repos\Selami\Selami\resimler\apikey.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            speech = SpeechClient.Create();
            client = TextToSpeechClient.Create();
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            cancellationTokenSource = new CancellationTokenSource();
            InitializeComponent();
            komutlar = new List<string>();

           
      

        }

  
        private void Timer_Tick(object sender, EventArgs e)
        {
            if(yazi.ToLower().IndexOf("tamam")>-1)
            {
                Console.WriteLine("tamam dendi");
                if (yazi.Length>8)
                {
                    Console.WriteLine("yazı bitti");
                    Console.WriteLine(yazi);
                    tamam = true;
                    cancellationTokenSource.Cancel();
                }
                
               
            }
            dinle(yazi);
            if(komutMod.IsChecked == true)
            {
                komut_dinle(yazi);
            }
            if (aramaMod.IsChecked == true)
            {
                arama_dinle(yazi);
            }
            timer.Stop();
            
        }

        private async void arama_dinle(string yazi)
        {
            if (yazi.ToLower().IndexOf("tamam") > -1)
            {
                await Task.Delay(500);
                if(yazi.ToLower().IndexOf("google'da ara")>-1 ||yazi.ToLower().IndexOf("google'dan ara") > -1)
                {
                    string aranan = "/C start chrome www.google.com/search?q=\"" + yazi.Substring(0, yazi.Length -20) + "\"";
                    
                    Process.Start("cmd.exe",aranan );
                }
                else if (yazi.ToLower().IndexOf("yandex'te ara") > -1 || yazi.ToLower().IndexOf("yandex'ten ara") > -1)
                {
                    string aranan = "/C start chrome yandex.com.tr/search/?lr=11503\"&\"text=\"" + yazi.Substring(0, yazi.Length - 20) + "\"";
                    Process.Start("cmd.exe", aranan);
                    //MessageBox.Show(aranan);

                }
                else if (yazi.ToLower().IndexOf("bing'te ara") > -1 || yazi.ToLower().IndexOf("bing'den ara") > -1)
                {
                    string aranan = "/C start chrome www.bing.com/search?q=\"" + yazi.Substring(0, yazi.Length - 18) + "\"";
                    Process.Start("cmd.exe", aranan);
                   
                }

            }
        }

        async Task<object> StreamingMicRecognizeAsync(int seconds)
        {
            object writeLock = new object();
            bool writeMore = true;
            if (tamam)
            {
                return 0;
            }


            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                metin.Content = "Mikrofon Yok!";
                return -1;
            }
            var speech = SpeechClient.Create();
            var streamingCall = speech.StreamingRecognize();
            
            await streamingCall.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding =
                            RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = "tr",
                        },
                        InterimResults = true,
                    }
                });
           
            Task printResponses = Task.Run(async () =>
            {
                while (await streamingCall.ResponseStream.MoveNext(
                    default(System.Threading.CancellationToken)))
                {
                    foreach (var result in streamingCall.ResponseStream
                        .Current.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {

                            if (!tamam)
                            {
                                yazi = alternative.Transcript;
                                timer.Start();

                            }

                        }
                       
                    }
                    
                }
            });
         
            
           
            var waveIn = new NAudio.Wave.WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            waveIn.DataAvailable +=
                (object sender, NAudio.Wave.WaveInEventArgs args) =>
                {
                    lock (writeLock)
                    {
                        if (!writeMore) return;
                        streamingCall.WriteAsync(
                            new StreamingRecognizeRequest()
                            {
                                AudioContent = Google.Protobuf.ByteString
                                    .CopyFrom(args.Buffer, 0, args.BytesRecorded)
                            }).Wait();
                    }
                };
            
            
            waveIn.StartRecording();
            metin.Content = "Şimdi Konuşabilirsiniz";
            kulak.Visibility = Visibility.Visible;
            acikAgiz.IsEnabled = false;
            kapaliAgiz.IsEnabled = false;
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationTokenSource.Token);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
               cancellationTokenSource.Dispose();
             
            }
            
            acikAgiz.IsEnabled = true;
            kapaliAgiz.IsEnabled = true;
            kulak.Visibility = Visibility.Hidden;
            waveIn.StopRecording();
            
            lock (writeLock) writeMore = false;


            if(genelMod.IsChecked == true)
            {
                cevapla(yazi);
            }
            if(ceviriMod.IsChecked == true)
            {
                cevir(yazi);
            }
            
            await streamingCall.WriteCompleteAsync();
            await printResponses;
            metin.Content = yazi;
            
            return 0;
        }

        private void cevir(string yazi)
        {
            TranslationClient clientTranslate = TranslationClient.Create();
            var detection = clientTranslate.DetectLanguage(text: yazi.Substring(0, yazi.Length - 6));
            if(detection.Language == "en")
            {
                var response = clientTranslate.TranslateText(yazi.Substring(0, yazi.Length - 6), "tr");
                seslendir(response.TranslatedText,detection.Language);

            }
            else if(detection.Language == "tr")
            {
                var response = clientTranslate.TranslateText(yazi.Substring(0, yazi.Length - 6), "en");
                seslendir(response.TranslatedText,detection.Language);
            }
        }

        public void dinle(string soru)
        {  
            
            metin.Content = kisi+": "+soru;
        }
        bool cevaplayamadim = false;
        bool durdurma = false;
        public void komut_dinle(string komut)
        {
            //Komut listesinde var mı varsa yap
            if(komut.ToLower().IndexOf("aç tamam")>-1)
            {
                try
                {
                    string[] ayiklanmisKomut = new string[20];
                    ayiklanmisKomut = komut.ToLower().Split(' ');
                    if (ayiklanmisKomut[1] != "aç" && ayiklanmisKomut[1] != "tamam")
                    {
                        ayiklanmisKomut[0] += " " + ayiklanmisKomut[1];
                    }
                    MessageBox.Show(ayiklanmisKomut[0]);
                    string islenecekKomut = db.komutSor(ayiklanmisKomut[0]);
                    Process.Start("cmd.exe", "/C " + islenecekKomut);
                }
                catch (Exception e)
                {
                    cevapla("anlama");
                }
                finally
                {
                    db.bagKapat();
                }
                
            }
            if(komut.ToLower().IndexOf("selam iyi kapat") > -1 ||komut.ToLower().IndexOf("kendini kapat")>-1)
            {
                Environment.Exit(0);
            }
        } 
       
    public void cevapla(string soru)
        {
            string text = "";
            mediaPlayer.Close();
            if (String.IsNullOrEmpty(soru))
            { 
                text = "yoksa bana küstün mü ?";
            }
            else
            {
                try
                {
                    soru = soru.Substring(0, yazi.Length - 6);
                    Console.WriteLine("soru :" + soru);
                    text = db.sor(soru.ToLower());
                    
                    cevaplayamadim = false;
                    durdurma = false;
                    durdurmaBtn.Visibility = Visibility.Hidden;
                }
                catch (Exception e)
                {
                    text = "Anlamadım. Lütfen tekrar söyle.";
                    cevaplayamadim = true;
                    durdurmaBtn.Visibility = Visibility.Visible;
                }
                finally
                {
                    db.bagKapat();
                }
            }
            yazi = text;
            VoiceSelectionParams voice = new VoiceSelectionParams
            {
                LanguageCode = "tr-TR",
                SsmlGender = SsmlVoiceGender.Male
            };
            AudioConfig config = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };
            SynthesisInput input = new SynthesisInput
            {
                Text = text
            };
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = voice,
                AudioConfig = config
            });
            using (Stream output = File.Create("C:\\Users\\corx\\source\\repos\\Selami\\Selami\\ses\\sample.mp3"))
            {
                response.AudioContent.WriteTo(output);
            }
            mediaPlayer.Open(new Uri("C:\\Users\\corx\\source\\repos\\Selami\\Selami\\ses\\sample.mp3"));
            mediaPlayer.Play();
            
  
         
            
        }
        public void seslendir(string metin, string detected)
        {
            mediaPlayer.Close();
            string lang = "";
            if (detected == "tr") lang = "en-EN";
            if (detected == "en") lang = "tr-TR";
            VoiceSelectionParams voice = new VoiceSelectionParams
            {
                
                LanguageCode = lang,
                SsmlGender = SsmlVoiceGender.Male
            };
            AudioConfig config = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };
            SynthesisInput input = new SynthesisInput
            {
                Text = metin
            };
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = voice,
                AudioConfig = config
            });
            using (Stream output = File.Create("C:\\Users\\corx\\source\\repos\\Selami\\Selami\\ses\\sample.mp3"))
            {
                response.AudioContent.WriteTo(output);
            }
            mediaPlayer.Open(new Uri("C:\\Users\\corx\\source\\repos\\Selami\\Selami\\ses\\sample.mp3"));
            mediaPlayer.Play();
        }

        private async void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            for (int i = 0; i <(sesliHarfSay(yazi)); i++)
            {
                agizHareket();
                await Task.Delay(80);
            }


        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (cevaplayamadim)
            {
                agiz_Click(this, new RoutedEventArgs());
                cevaplayamadim = false;
            }
            kapaliAgiz.Visibility = Visibility.Visible;
            acikAgiz.Visibility = Visibility.Hidden;

        }

        private void Ayarlarbtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (menu.Visibility == Visibility.Visible)
            {
                menu.Visibility = Visibility.Hidden;
            }
            else
            {
                menu.Visibility = Visibility.Visible;
            }
           
      
        }


        private async void agiz_Click(object sender, RoutedEventArgs e)
        {
            if (genelMod.IsChecked == true)
            {
                cancellationTokenSource = new CancellationTokenSource();
                object x = await StreamingMicRecognizeAsync(400);
            }
            else if (komutMod.IsChecked == true)
            {
                Console.WriteLine("Komut modu");
                cancellationTokenSource = new CancellationTokenSource();
                object x = await StreamingMicRecognizeAsync(400);
            }
            else if (eglenceMod.IsChecked == true)
            {
                Console.WriteLine("eğlence modu");
            }
            else if (aramaMod.IsChecked == true)
            {
                Console.WriteLine("arama modu");
                cancellationTokenSource = new CancellationTokenSource();
                object x = await StreamingMicRecognizeAsync(400);
            }
            else if (ceviriMod.IsChecked == true)
            {
                Console.WriteLine("çeviri modu");
                cancellationTokenSource = new CancellationTokenSource();
                object x = await StreamingMicRecognizeAsync(400);
            }
           
            tamam = false;
            durdurma = false;
           
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void agizHareket()
        {
            if(acikAgiz.Visibility == Visibility.Hidden)
            {
                kapaliAgiz.Visibility = Visibility.Hidden;
                acikAgiz.Visibility = Visibility.Visible;
            }else if(kapaliAgiz.Visibility == Visibility.Hidden)
            {
                acikAgiz.Visibility = Visibility.Hidden;
                kapaliAgiz.Visibility = Visibility.Visible;
            }
        }

        private void Durdurma_MouseDown(object sender, MouseButtonEventArgs e)
        {
            durdurmaBtn.Visibility = Visibility.Hidden;
            durdurma = true;
            cevaplayamadim = false;
        }
        private int sesliHarfSay(string cumle)
        {
            string sesli = "a,e,ı,i,o,ü,u "; /// extra harf eklenebilir.
            int sayac = 0;
            foreach (var item in cumle)
            {
                foreach (var item2 in sesli)
                {
                    if (item == item2)
                    {
                        sayac++;
                    }
                }

            }
            return sayac;   
        }

        private void OgrAc_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"C:\Users\corx\source\repos\Selami\Selogretmen\bin\Debug\Selogretmen.exe");
            Environment.Exit(0);
        }

        private void Kapat_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void asyncDurdur(CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
