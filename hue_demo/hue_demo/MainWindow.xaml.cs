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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json;
using Coding4Fun.Toolkit.Controls.Common;
using System.Windows.Threading;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
namespace hue_demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        VisionServiceClient VisionServiceClient = new VisionServiceClient("178cca21d70f412ea2bacf7a009768d0");
        EmotionServiceClient emotionServiceClient = new EmotionServiceClient("d0ef6aa0e7e34facb8bf814c2c86870d");
        DispatcherTimer timer1;
        public MainWindow()
        {   
            InitializeComponent();
            //音樂路徑
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            string FilePath = System.IO.Path.GetDirectoryName(ass.Location) + "\\music\\music.mp3";
            Uri fileUri = new Uri(FilePath);
            PlayMusic.Source = fileUri;
            PlayMusic.Play();
            timer1 = new DispatcherTimer();
            timer1.Tick += new EventHandler(timer_Tick1);
            //   timer1.Interval = new TimeSpan(0, 0, 0, 4, 0);
            timer1.Start();

            //初始化
            setDefault();
        }
        private async void choose_Click(object sender, RoutedEventArgs e)
        {
            status = 1; 
            int rgb;
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

            if (!(bool)result)
            {
                return;
            }

            string filePath = openDlg.FileName;

            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();

            photo.Source = bitmapSource;

            AnalysisResult images = await UploadAndAnalyzeImage(filePath);


            //Console.WriteLine(emotionResult[0].Scores.Anger);
            //Console.WriteLine(emotionResult[1].Scores.Neutral);
            //Console.WriteLine(emotionResult[1].Scores.Sadness);
            //Console.WriteLine(emotionResult[1].Scores.Fear);
            //if (emotionResult[1].Scores.Neutral > emotionResult[1].Scores.Fear) {
            //    Console.WriteLine("i wim");
            //}
            //Console.WriteLine(emotionResult[1].Scores.Happiness);
            //output.Content = emotionResult[1].Scores.Neutral.ToString();
            output.Content = images.Color.AccentColor.ToString();

            //將project oxford 傳回來的Accent color 轉成 Int 
            String temp = output.Content.ToString();
            String qq = temp;
            int num = Int32.Parse(qq, System.Globalization.NumberStyles.HexNumber);
            //轉成Color型態
            System.Drawing.Color myColor = System.Drawing.Color.FromArgb(num);
            //轉成HSB
            //取得Hue
            float huevalue = myColor.GetHue();
            //取得Brightness
            float brivalue = myColor.GetBrightness();
            //取得Saturation
            float sat = myColor.GetSaturation();

            byte B = myColor.B;
            byte R = myColor.R;
            byte G = myColor.G;

            SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, R, G, B));
            SolidColorBrush blurbrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, R, G, B));

            //  imgborder.BorderBrush = brushframe;
            back.Background = brush;
            blurimage.Source = bitmapSource;

            //Call Hue API 讓燈泡變色
            SetColorButtonClick(huevalue, sat, brivalue);
        }

        private async Task<AnalysisResult> UploadAndAnalyzeImage(string imageFilePath)
        {
            using (Stream imageFileStream = File.OpenRead(imageFilePath))
            {
                //
                // Analyze the image for all visual features
                //

                VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
                AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                return analysisResult;
            }
        }
        private void SetColorButtonClick(float hue, float sat, float bri)
        {

            try
            {
                //Get the HSV Value from the currently selected colovarr
                //var hsv = _colorPicker.Color.GetHSV();

                //build our State object
                var state = new
                {
                    on = true,
                    hue = (int)(hue * 182.04),
                    sat = (int)(sat * 254),
                    bri = (int)(bri * 254)
                    //hue = (int)(hsv.Hue * 182.04), //we convert the hue value into degrees by multiplying the value by 182.04
                    //sat = (int)(hsv.Saturation * 254)
                };
                Console.WriteLine(state);
                //_responseTextBox.Text += state.hue + " and " + state.sat;
                //convert it to json:
                var jsonObj = JsonConvert.SerializeObject(state);

                //set the api url to set the state
                var uri2 = new Uri(string.Format("http://192.168.0.101/api/coding4fun/lights/2/state"));
                var uri3 = new Uri(string.Format("http://192.168.0.101/api/coding4fun/lights/3/state"));
                var uri4 = new Uri(string.Format("http://192.168.0.101/api/coding4fun/lights/4/state"));

                var client2 = new WebClient();
                var client3 = new WebClient();
                var client4 = new WebClient();
                //decide what to do with the response we get back from the bridge
                //client.UploadStringCompleted += (o, args) => Dispatcher.BeginInvoke(() =>
                //{
                //    try
                //    {
                //   //     _responseTextBox.Text = args.Result;
                //    }
                //    catch (Exception ex)
                //    {
                // //       _responseTextBox.Text = ex.Message;
                //    }

                //});

                //Invoke the PUT method to set the state of the bulb
                client2.UploadStringAsync(uri2, "PUT", jsonObj);
                client3.UploadStringAsync(uri3, "PUT", jsonObj);
                client4.UploadStringAsync(uri4, "PUT", jsonObj);
            }
            catch (Exception ex)
            {
                //  _responseTextBox.Text = ex.Message;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //Thread newthread = new Thread(TimeCount);
            //newthread.Start();
            status = 0;
            PlayMusic.Play();


            timer1 = new DispatcherTimer();
            timer1.Tick += new EventHandler(timer_Tick1);
            //   timer1.Interval = new TimeSpan(0, 0, 0, 4, 0);
            timer1.Start();
            //PlayMusic.Source = new Uri(@"C:\Haze.mp3");
        }
        int status = 0;

        private void setDefault()
        {
            //設定初始圖片
            System.Reflection.Assembly ass22 = System.Reflection.Assembly.GetExecutingAssembly();
            string FilePaths = System.IO.Path.GetDirectoryName(ass22.Location) + "\\background\\background.jpg";
            Uri fileUris = new Uri(FilePaths);
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUris;
            bitmapSource.EndInit();
            photo.Source = bitmapSource;
            byte B = 255;
            byte R = 255;
            byte G = 255;

            SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, R, G, B));
            SolidColorBrush brushframe = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R, G, B));
            imgborder.BorderBrush = brushframe;
            back.Background = brush;
            blurimage.Source = bitmapSource;
        }
        async void timer_Tick1(object sender, EventArgs e)
        {
            

            //設定資料夾路徑
            //string FilePath = @"C:\\Users\\v-edhuan\\OneDrive\\image\\";
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            string FilePath = System.IO.Path.GetDirectoryName(ass.Location) + "\\image\\";

            //string FilePath = "./image/";

            DirectoryInfo di = new DirectoryInfo(FilePath); //設定要找的資料夾路徑 
            FileInfo[] fi = di.GetFiles("*.jpg"); //設定要找的圖片副檔名 

            timer1.Stop();
            foreach (FileInfo file in fi)
            {
                if (status == 0)
                {
                    Console.WriteLine(file.Name);
                    AnalysisResult images = await UploadAndAnalyzeImage(FilePath + file.Name);
                    output.Content = images.Color.AccentColor.ToString();
                    Console.WriteLine(output.Content);

                    Uri fileUri = new Uri(FilePath + file.Name);
                    BitmapImage bitmapSource = new BitmapImage();

                    bitmapSource.BeginInit();
                    bitmapSource.CacheOption = BitmapCacheOption.None;
                    bitmapSource.UriSource = fileUri;
                    bitmapSource.EndInit();
                    photo.Source = bitmapSource;
                    //將project oxford 傳回來的Accent color 轉成 Int 
                    String temp = output.Content.ToString();
                    String qq = temp;
                    int num = Int32.Parse(qq, System.Globalization.NumberStyles.HexNumber);
                    //轉成Color型態
                    System.Drawing.Color myColor = System.Drawing.Color.FromArgb(num);
                    //轉成HSB
                    //取得Hue
                    float huevalue = myColor.GetHue();
                    //取得Brightness
                    float brivalue = myColor.GetBrightness();
                    //取得Saturation
                    float sat = myColor.GetSaturation();

                    byte B = myColor.B;
                    byte R = myColor.R;
                    byte G = myColor.G;

                    SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, R, G, B));
                    SolidColorBrush brushframe = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R, G, B));
                    imgborder.BorderBrush = brushframe;
                    back.Background = brush;
                    blurimage.Source = bitmapSource;
                    Console.WriteLine(B.ToString() + " " + R.ToString() + " " + G.ToString());
                    Thread.Sleep(1500);
                    //Call Hue API 讓燈泡變色
                    SetColorButtonClick(huevalue, sat, brivalue);
                }
                else
                    break;

            }
            timer1.Start();
        }

        private async Task<Emotion[]> UploadAndDetectEmotions(string imageFilePath)
        {
            try
            {
                Emotion[] emotionResult;
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    //
                    // Detect the emotions in the URL
                    //
                    emotionResult = await emotionServiceClient.RecognizeAsync(imageFileStream);
                    return emotionResult;
                }
            }
            catch (Exception exception)
            {
                return null;
            }

        }

        private void DetectnchangeColor(Emotion[] emotionResult)
        {
            if (emotionResult.Length > 0)
            {
                List<double> emotionList = new List<double>();
                Dictionary<double, string> emotionDict = new Dictionary<double, string>();
                Dictionary<string, string> emotionColor = new Dictionary<string, string>()
                {
                    {"Anger","e72611"},
                    {"Contempt","8411e7"},
                    {"Disgust","034723"},
                    {"Fear","03f7fd"},
                    {"Happiness","f65cf0"},
                    {"Neutral","ffffff"},
                    {"Sadness","1903a5"},
                    {"Surprise","fff400"},
                };

                emotionList.Add(emotionResult[0].Scores.Anger);
                emotionList.Add(emotionResult[0].Scores.Contempt);
                emotionList.Add(emotionResult[0].Scores.Disgust);
                emotionList.Add(emotionResult[0].Scores.Fear);
                emotionList.Add(emotionResult[0].Scores.Happiness);
                emotionList.Add(emotionResult[0].Scores.Neutral);
                emotionList.Add(emotionResult[0].Scores.Sadness);
                emotionList.Add(emotionResult[0].Scores.Surprise);

                emotionDict.Add(emotionResult[0].Scores.Anger, "Anger");
                emotionDict.Add(emotionResult[0].Scores.Contempt, "Contempt");
                emotionDict.Add(emotionResult[0].Scores.Disgust, "Disgust");
                emotionDict.Add(emotionResult[0].Scores.Fear, "Fear");
                emotionDict.Add(emotionResult[0].Scores.Happiness, "Happiness");
                emotionDict.Add(emotionResult[0].Scores.Neutral, "Neutral");
                emotionDict.Add(emotionResult[0].Scores.Sadness, "Sadness");
                emotionDict.Add(emotionResult[0].Scores.Surprise, "Surprise");

                //生氣中帶有點鄙視
                double maxE = 0;
                double SmaxE = 0;
                foreach (var item in emotionList)
                {

                    if (item > maxE)
                    {
                        SmaxE = maxE;
                        maxE = item;
                    }
                    else if (item > SmaxE)
                    {
                        SmaxE = item;
                    }
                }

                double xx = emotionList.Max();
                string emotion1 = emotionDict[maxE];
                string emotion2 = emotionDict[SmaxE];
                Console.WriteLine(emotion1 + "    " + emotion2);

                //設定燈泡的顏色
                String temp = emotionColor[emotion1];
                //Console.WriteLine(emotionResult[0].Scores.Anger);
                //Console.WriteLine(emotionResult[0].Scores.Contempt);
                //Console.WriteLine(emotionResult[0].Scores.Disgust);
                //Console.WriteLine(emotionResult[0].Scores.Fear);
                //Console.WriteLine(emotionResult[0].Scores.Happiness);
                //Console.WriteLine(emotionResult[0].Scores.Neutral);
                //Console.WriteLine(emotionResult[0].Scores.Sadness);
                //Console.WriteLine(emotionResult[0].Scores.Surprise);


                //Console.WriteLine(emotionResult[1].Scores.Neutral);
                //Console.WriteLine(emotionResult[1].Scores.Sadness);
                //Console.WriteLine(emotionResult[1].Scores.Fear);
                //if (emotionResult[1].Scores.Neutral > emotionResult[1].Scores.Fear) {
                //    Console.WriteLine("i wim");
                //}
                //Console.WriteLine(emotionResult[1].Scores.Happiness);
                //output.Content = emotionResult[1].Scores.Neutral.ToString();
                //  output.Content = images.Color.AccentColor.ToString();

                //將project oxford 傳回來的Accent color 轉成 Int 
                //String temp = output.Content.ToString();

                int num = Int32.Parse(temp, System.Globalization.NumberStyles.HexNumber);
                //轉成Color型態
                System.Drawing.Color myColor = System.Drawing.Color.FromArgb(num);
                //轉成HSB
                //取得Hue
                float huevalue = myColor.GetHue();
                //取得Brightness
                float brivalue = myColor.GetBrightness();
                //取得Saturation
                float sat = myColor.GetSaturation();

                byte B = myColor.B;
                byte R = myColor.R;
                byte G = myColor.G;


                SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, R, G, B));
                SolidColorBrush brushframe = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R, G, B));
                imgborder.BorderBrush = brushframe;
                back.Background = brush;

                //Call Hue API 讓燈泡變色
                SetColorButtonClick(huevalue, sat, brivalue);

            }
        }


    }
}
