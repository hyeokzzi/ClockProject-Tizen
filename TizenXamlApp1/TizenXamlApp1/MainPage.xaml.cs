using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Xml;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;


namespace TizenXamlApp1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        static int soccer_flag = 0;
        static string current_wt = "";
        static string night_flag = "";
        public MainPage()
        {
            InitializeComponent();
            Analog_Clock();
            digital_Clock();
            slide_clock();
            Get_Weather();
            Get_news();
            if (soccer_flag == 0)
            {
                // 무료 수준인 500회 이상을 넘어가지 않게 하기위해 설정
                Get_Score();
                soccer_flag = 1;
            }
        }

        // 아날로그 시계부
        private void Analog_Clock()
        {
            System.Timers.Timer timer = new System.Timers.Timer(500);
            timer.Elapsed += TimerElapsedEvent;
            timer.Start();
        }

        private void TimerElapsedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateAnalogClock();
        }

        private void UpdateAnalogClock()
        {
            DateTime currentTime = DateTime.Now;

            int sec = currentTime.Second;
            int min = currentTime.Minute;
            int hour = currentTime.Hour % 12;

            Analog_clock_sec.RotateTo((sec * 6) % 360, 0);
            Analog_clock_min.RotateTo((min * 6 + sec * 0.1) % 360, 0);
            Analog_clock_hour.RotateTo((hour * 30 + min * 0.5 + sec * (1 / 120)) % 360, 0);
        }


        // 디지털 시계분
        private async void slide_clock()
        {
            while (true)
            {
                await today.TranslateTo(1800, 0, 10000);
                await today.TranslateTo(-2000, 0, 0);
                await today.TranslateTo(70, 0, 2000);
                await Task.Delay(1000);
            }
        }
        private async void digital_Clock()
        {
            while (true)
            {
                // 날짜
                CultureInfo culture = new CultureInfo("ko-KR");
                today.Text = DateTime.Now.ToString("yyyy년 MM월 dd일  dddd", culture);
                // 디지털 시계 시간
                HH.Text = DateTime.Now.ToString("hh");
                MM.Text = DateTime.Now.ToString("mm");
                SS.Text = DateTime.Now.ToString("ss");
                TT.Text = DateTime.Now.ToString("tt");

                // 깜빡임 구현
                int second = DateTime.Now.Second;

                switch (second % 2)
                {
                    case 0:
                        change_text.Opacity = 0;
                        break;
                    case 1:
                        change_text.Opacity = 1;
                        break;
                }

                // 밤 낮에 따른 사진 목록 변화
                background1.Source = current_wt + night_flag + 1 + ".jpg";
                background2.Source = current_wt + night_flag + 2 + ".jpg";
                background3.Source = current_wt + night_flag + 3 + ".jpg";
                if (second % 20 == 0)
                {
                    switch (second / 20)
                    {
                        case 0:
                            //background3.Opacity = 0;
                            await background3.FadeTo(0, 500);
                            //background1.Opacity = 0.6;
                            await background1.FadeTo(1, 500);
                            break;
                        case 1:
                            //background1.Opacity = 0;
                            await background1.FadeTo(0, 500);
                            //background2.Opacity = 0.6;
                            await background2.FadeTo(1, 500);
                            break;
                        case 2:
                            //background2.Opacity = 0;
                            await background2.FadeTo(0, 500);
                            //background3.Opacity = 0.6;
                            await background3.FadeTo(1, 500);
                            break;
                    }
                }


                // 시간에 따라 변수 저장
                if ((DateTime.Now.ToString("tt") == "AM" && DateTime.Now.Hour > 6)  || (DateTime.Now.ToString("tt") == "PM" && DateTime.Now.Hour <= 18))
                {
                    night_flag = "Afternoon";
                    today.TextColor = Color.Black;
                    TT.TextColor = Color.Black;
                    HH.TextColor = Color.Black;
                    change_text.TextColor = Color.Black;
                    MM.TextColor = Color.Black;
                    SS.TextColor = Color.Black;
                    current_w.TextColor = Color.Black;
                    current_weather_text.TextColor = Color.Black;
                    h_12.TextColor = Color.Black;
                    seq0.TextColor = Color.Black;
                    h_24.TextColor = Color.Black;
                    seq1.TextColor = Color.Black;
                    h_36.TextColor = Color.Black;
                    seq2.TextColor = Color.Black;
                }
                else
                {
                    night_flag = "Night";
                    today.TextColor = Color.White;
                    TT.TextColor = Color.White;
                    HH.TextColor = Color.White;
                    change_text.TextColor = Color.White;
                    MM.TextColor = Color.White;
                    SS.TextColor = Color.White;
                    current_w.TextColor = Color.White;
                    current_weather_text.TextColor = Color.White;
                    h_12.TextColor = Color.White;
                    seq0.TextColor = Color.White;
                    h_24.TextColor = Color.White;
                    seq1.TextColor = Color.White;
                    h_36.TextColor = Color.White;
                    seq2.TextColor = Color.White;
                }

                if(DateTime.Now.Hour % 6 == 0 && DateTime.Now.Second == 0)
                {
                    soccer_flag = 0;
                }

                await Task.Delay(500);
            }
        }
        //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
        // 날씨 구현

        private async void Get_Weather()
        {
            string rssUrl = "http://www.kma.go.kr/wid/queryDFSRSS.jsp?zone=2818583000";

            while (true)
            {
                try
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync(rssUrl);

                    if (!response.IsSuccessStatusCode)
                    {

                        await Task.Delay(2000); // 2초 대기
                        continue; // 다음 반복으로 이동하여 다시 요청
                    }

                    string rssData = await response.Content.ReadAsStringAsync();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(rssData);

                    XmlNodeList dataNodes = xmlDoc.SelectNodes("//data");
                    string[] weather = new string[21];
                    int idx = 0;
                    foreach (XmlNode dataNode in dataNodes)
                    {
                        var wfEn = dataNode.SelectSingleNode("wfEn").InnerText;
                        weather[idx] = wfEn;
                        idx++;
                    }

                    // 아래 단어가 포함되어 있으면 그 날씨인 것으로
                    string[] sort_weather = new string[3] { "Clear", "Cloudy", "Rain" };

                    // 현재 날씨에 대해 사진 변경
                    current_weather_text.Text = weather[0];
                    for (int i = 0; i < sort_weather.Length; i++)
                    {
                        if (sort_weather[i].Contains(weather[0].ToString()) || weather[0].ToString().Contains(sort_weather[i]))
                        {
                            current_weather_image.Source = sort_weather[i] + ".png";
                            current_wt = sort_weather[i];
                        }
                    }

                    seq0.Text = weather[4]; // 12시간
                    seq1.Text = weather[8]; // 24시간
                    seq2.Text = weather[12]; // 36시간 뒤


                    foreach(var sort in sort_weather)
                    {
                        if (sort.Contains(weather[4].ToString()) || weather[4].ToString().Contains(sort))
                        {
                            string new_image = sort + ".png";
                            after_12.Source = new_image;
                        }
                        if (sort.Contains(weather[8].ToString()) || weather[8].ToString().Contains(sort))
                        {
                            string new_image = sort + ".png";
                            after_24.Source = new_image;
                        }
                        if (sort.Contains(weather[12].ToString()) || weather[12].ToString().Contains(sort))
                        {
                            string new_image = sort + ".png";
                            after_36.Source = new_image;
                        }
                    }


                    // 주기적으로 데이터를 가져오기 위해 일정 시간 간격만큼 대기
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    await Task.Delay(2000); // 2초 대기
                    continue; // 다음 반복으로 이동하여 다시 요청
                    //return;
                }
            }
        }
        //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ//

        private async void Get_Score()
        {
            // TLS 1.2 설정
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://odds.p.rapidapi.com/v4/sports/soccer_epl/scores?daysFrom=3"),
                Headers =
                {
                    { "X-RapidAPI-Key", "60ea6c388fmsh61438a45047d154p154a13jsnee5349dd8ff2" },
                    { "X-RapidAPI-Host", "odds.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();

                int startIndex = -1;
                int arrayStartIndex;
                int arrayEndIndex = -1;
                string[] soccer_infos = new string[10];
                int index = 0;
                while (true)
                {
                    startIndex = jsonString.IndexOf("\"scores\":", startIndex + 1, StringComparison.OrdinalIgnoreCase);
                    if (startIndex < 0)
                        break;

                    arrayStartIndex = jsonString.IndexOf('[', startIndex);
                    if (arrayStartIndex < 0)
                        break;

                    arrayEndIndex = jsonString.IndexOf(']', arrayStartIndex);
                    if (arrayEndIndex < 0)
                        break;

                    // 추출한 정보 저장
                    soccer_infos[index] = jsonString.Substring(arrayStartIndex, arrayEndIndex - arrayStartIndex + 1);
                    index++;
                }
                string pattern = "\"(.*?)\"";
                string[] values = new string[100];
                int idx = 0;
                for (int i = 0; i < index; i++)
                {
                    string soccer_info = soccer_infos[i];
                    MatchCollection matches = Regex.Matches(soccer_info, pattern);
                    foreach (Match match in matches)
                    {
                        string value = match.Groups[1].Value;
                        if (idx % 2 == 1)
                        {
                            values[idx / 2] = value;
                        }
                        idx++;
                    }
                }
                string[] soccer_logo = new string[20]{ "Tottenham Hotspur", "Brentford", "Wolverhampton Wanderers",
                "Everton","Bournemouth","Manchester United","Liverpool","Aston Villa",
                "Fulham","Crystal Palace","Nottingham Forest","Arsenal","Leeds United",
                "West Ham United","Southampton","Brighton and Hove Albion","Chelsea","Manchester City",
                "Newcastle United","Leicester City"};
                //index = 0;
                if(index > 0)
                {
                    // 1경기 홈 팀
                    for (int i = 0; i < 20; i++)
                    {
                        if (soccer_logo[i] == values[0])
                        {
                            home_team_1.Source = soccer_logo[i] + ".png";
                            break;
                        }
                    }
                    home_score_1.Text = values[1] + " :";
                    // 1경기 어웨이 팀
                    for (int i = 0; i < 20; i++)
                    {
                        if (soccer_logo[i] == values[2])
                        {
                            away_team_1.Source = soccer_logo[i] + ".png";
                            break;
                        }
                    }
                    away_score_1.Text = values[3];
                }
                else
                {
                    home_team_1.Source = "경기없음.png";
                    home_team_1.WidthRequest = 400;
                    home_team_1.HeightRequest = 200;
                    home_score_1.Text = "";

                    away_team_1.Source = "clean.png";
                    away_score_1.Text = "";

                }


                // 2경기 홈 팀
                if(index > 1)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (soccer_logo[i] == values[4])
                        {
                            home_team_2.Source = soccer_logo[i] + ".png";
                            break;
                        }
                    }
                    home_score_2.Text = values[5] + " :";

                    for (int i = 0; i < 20; i++)
                    {
                        if (soccer_logo[i] == values[6])
                        {
                            away_team_2.Source = soccer_logo[i] + ".png";
                            break;
                        }
                    }
                    away_score_2.Text = values[7];
                }
                else
                {
                    home_team_2.Source = "clean.png";
                    home_score_2.Text = " ";
                    away_team_2.Source = "clean.png";
                    away_score_2.Text = " ";

                }

                if(index > 2)
                {
                    // 3경기 홈팀
                    for (int i = 0; i < 20; i++)
                    {
                        if (soccer_logo[i] == values[8])
                        {
                            home_team_3.Source = soccer_logo[i] + ".png";
                            break;
                        }
                    }
                    home_score_3.Text = values[9] + " :";
                    // 3경기 어웨이팀
                    for (int i = 0; i < 20; i++)
                    {
                        if (soccer_logo[i] == values[10])
                        {
                            away_team_3.Source = soccer_logo[i] + ".png";
                            break;
                        }
                    }
                    away_score_3.Text = values[11];
                }
                else
                {
                    home_team_3.Source = "clean.png";
                    home_score_3.Text = " ";
                    away_team_3.Source = "clean.png";
                    away_score_3.Text = " ";
                }

                await Task.Delay(10000);
            }
        }
        //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ//

        private async void Get_news()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://news.sbs.co.kr/news/SectionRssFeed.do?sectionId=09&plink=RSSREADER");

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            string rssData = await response.Content.ReadAsStringAsync();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(rssData);

            XmlNodeList dataNodes = xmlDoc.SelectNodes("//item");
            while (true)
            {
                foreach (XmlNode DataNode in dataNodes)
                {
                    var context = DataNode.SelectSingleNode("title");
                    string news = context.InnerText;
                    slide_news.Text = context.InnerText;
                    slide_news.TranslationX = 2000;
                    await slide_news.TranslateTo(-1920, 0, 10000);
                }
            }
        }

        //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ//
    }
}

