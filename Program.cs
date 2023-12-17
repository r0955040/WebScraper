using CsvHelper;
using CsvHelper.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.Globalization;
using System.Text.Json;


namespace WebScraperDemo
{
    class Program
    {
        // Het hoofdprogramma dat de gebruiker vraagt om een scraping-optie te kiezen. Als de gebruiker een invalide nummer ingeeft, dan krijgt hij de foutmelding "Oops, invalid choice! Please enter a valid option." en krijg hij/zij de kans om opnieuw een optie te kiezen.
        static void Main(string[] args)
        {
            bool validChoice = false;
            do
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Scrape YouTube");
                Console.WriteLine("2. Scrape ICT Jobs");
                Console.WriteLine("3. Scrape Barnes & Noble Books");
                Console.Write("Enter your choice (1, 2, or 3): ");


                // Hier wordt de keuze van de gebruiker gelezen en de bijhorende scraping-methode aangeroepen.
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ScrapeYouTube();
                        validChoice = true;
                        break;
                    case "2":
                        ScrapeICTJobs();
                        validChoice = true;
                        break;
                    case "3":
                        ScrapeBarnesAndNobleBooks();
                        validChoice = true;
                        break;
                    default:
                        // dit is dus wanneer een verkeerde nummer is ingegeven.
                        Console.WriteLine("Oops, invalid choice! Please enter a valid option.");
                        break;
                }
            } while (!validChoice);
        }


        // Methode om YouTube-gegevens te schrapen.
        static void ScrapeYouTube()
        {
            // Hier vraagt het programma de gebruiker om een zoekterm in te voeren.
            Console.WriteLine("Enter the search term for YouTube:");
            // De zoekterm wordt opgeslagen in de variabele youtubeSearchTerm.
            string youtubeSearchTerm = Console.ReadLine();
            // De Console wordt leeg, dit heb ik toegevoegd om het netjes te houden.
            Console.Clear();


            // Initialisatie van ChromeDriver voor het besturen van de browser.
            IWebDriver driver = new ChromeDriver();


            // Navigeer naar de YouTube-website.
            driver.Navigate().GoToUrl("https://www.youtube.com/");


            // Wanneer we navigeren naar de Youtube-website krijgen we de cookies melding. Deze moet eerst weggeklikt worden voor dat de zoekterm uitgevoerd wordt. Ik heb gebruik gemaakt van Xpath om de "Reject All' knop te vinden waarmmee we de cookies afwijzen.
            IWebElement reject = driver.FindElement(By.XPath("//*[@id=\"content\"]/div[2]/div[6]/div[1]/ytd-button-renderer[1]/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]"));
            // Er wordt op de "Reject all" knop geklikt.
            reject.Click();


            // Zoek naar de zoekbalk op de YouTube-pagina met behulp van XPath.
            IWebElement youtubeSearchInput = driver.FindElement(By.XPath("/html/body/ytd-app/div[1]/div/ytd-masthead/div[4]/div[2]/ytd-searchbox/form/div[1]/div[1]/input"));
            // Klik op de zoekbalk, voer de zoekterm in en voer de zoekopdracht uit.
            youtubeSearchInput.Click();
            youtubeSearchInput.SendKeys(youtubeSearchTerm); // hier wordt de zoekterm ingevoerd op de zoekbalk.
            youtubeSearchInput.Submit(); // hier wordt de zoekterm uitgevoerd


            // Wacht maximaal 10 seconden op impliciete elementen. Deze kon weggelaten worden, maar voor de zekerheid heb ik deze laten staan, zodat de programma genoeg tijd heeft om te werken.
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);


            // Zoek en klik op de 'Filters' knop om zoekresultaten te filteren.
            IWebElement filters = driver.FindElement(By.XPath("/html/body/ytd-app/div[1]/ytd-page-manager/ytd-search/div[1]/div/ytd-search-header-renderer/div[3]/ytd-button-renderer/yt-button-shape/button/yt-touch-feedback-shape/div/div[2]"));
            // Klik op de knop, waarna je uit verschillende filter opties kunt kiezen.
            filters.Click();


            // Aangezien ik op datum moest sorteren, heb ik ann de hand van XPath van de 'upload op datum' knop deze geidentificeerd. Daarna klikt de tool op de 'Uplooad op datum' filter om resultaten op te laden op datum.
            IWebElement uploaddate = driver.FindElement(By.XPath("/html/body/ytd-app/ytd-popup-container/tp-yt-paper-dialog/ytd-search-filter-options-dialog-renderer/div[2]/ytd-search-filter-group-renderer[5]/ytd-search-filter-renderer[2]/a/div/yt-formatted-string"));
            uploaddate.Click();


            // Wacht 5 seconden om de pagina-elementen te laden.
            Thread.Sleep(5000);


            // Zoek alle video-items op de pagina aan de hand van XPath.
            var youtubevideos = driver.FindElements(By.XPath("//*[@id=\"dismissible\"]"));

            // Ik heb een lijst aangemaakt om YouTube-gegevens op te slaan.
            List<YoutubeData> videoList = new List<YoutubeData>();

            // Loop door de eerste 5 video's op de pagina.
            for (int i = 0; i < 5; i++)
            {
               
                int index = videoList.Count + 1;

                // Vernieuw de lijst van video-items.
                youtubevideos = driver.FindElements(By.XPath("//*[@id=\"dismissible\"]"));

                // hier worden alle gegevens dat ik nodig heb voor deze website opgeslagen in verschillende variables en met verschillende methodes (XPath, CssSelector en ClassName)
                var videoTitle = youtubevideos[i].FindElement(By.CssSelector("#video-title")).Text;
                var link = youtubevideos[i].FindElement(By.ClassName("yt-simple-endpoint")).GetAttribute("href");
                var uploader = youtubevideos[i].FindElement(By.XPath("/html/body/ytd-app/div[1]/ytd-page-manager/ytd-search/div[1]/ytd-two-column-search-results-renderer/div/ytd-section-list-renderer/div[2]/ytd-item-section-renderer/div[3]/ytd-video-renderer[" + (i+1) + "]/div[1]/div/div[2]/ytd-channel-name/div/div/yt-formatted-string/a")).Text;
                var views = youtubevideos[i].FindElement(By.XPath(".//*[@id='metadata-line']/span[1]")).Text;

                // Dit wordt getoond in de console (de details).
                Console.WriteLine($"Video {index}:");
                Console.WriteLine($"Title: {videoTitle}");
                Console.WriteLine($"Link: {link}");
                Console.WriteLine($"Uploader: {uploader}");
                Console.WriteLine($"Number of views: {views}");
                Console.WriteLine(" ");

                // Voeg gegevens toe aan de lijst.
                videoList.Add(new YoutubeData
                {
                    VideoTitle = videoTitle,
                    VideoLink = link,
                    VideoUploader = uploader,
                    VideoViews = views
                }) ;
            }

            // Schrijf YouTube-gegevens naar CSV- en JSON-bestanden.
            WriteToCsv(videoList, "youtube_output.csv");
            WriteToJson(videoList, "youtube_output.json");

            Console.WriteLine("YouTube scraping completed. Press any key to exit.");
            Console.ReadKey();

            // Sluit de ChromeDriver na het scrapen.
            driver.Quit();
        }

        //***********************************************************************************************************************************************************//

        // Methode om ICT-jobgegevens te schrapen.
        static void ScrapeICTJobs()
        {
            // Hier vraagt het programma de gebruiker om een zoekterm in te voeren.
            Console.WriteLine("Enter the search term for ICT jobs:");
            // De zoekterm wordt opgeslagen in de variabele searchTerm.
            string searchTerm = Console.ReadLine();
            Console.Clear();

            // Initialisatie van ChromeDriver voor het besturen van de browser.
            IWebDriver driver = new ChromeDriver();

            // Navigeer naar de ICT Jobs-website.
            driver.Navigate().GoToUrl("https://www.ictjob.be");

            // Zoek de zoekbalk op de ICT Jobs-pagina.
            IWebElement jobsSearchInput = driver.FindElement(By.XPath("//*[@id=\"keywords-input\"]"));

            // Voer de zoekterm in en voer de zoekopdracht uit
            jobsSearchInput.SendKeys(searchTerm);
            jobsSearchInput.Submit();

            // Zoek en klik op de 'OK'-knop voor cookies.
            IWebElement ok = driver.FindElement(By.XPath("//*[@id=\"cookie-info-text\"]/div/div/div[2]/a"));
            ok.Click();

            // Zoek en klik op de 'date' knop om resultaten op datum te sorteren, aangezien het default op 'relevancie' staat
            IWebElement date = driver.FindElement(By.XPath("//*[@id=\"sort-by-date\"]"));
            date.Click();

            // Scroll naar beneden om meer zoekresultaten te laden.
            Actions actions = new Actions(driver);
            actions.SendKeys(Keys.PageDown).Perform();

            // Wacht 11 seconden om pagina-elementen te laden. Ik heb deze op 11 seconden gezet, omdat de site zoveel tijd nodig had om het resultaat op datum te sorteren.
            Thread.Sleep(11000);

            // Hier heb ik XPath gebruikt om individuele jobitems te selecteren.
            var jobItems = driver.FindElements(By.CssSelector(".search-item"));

            // Lijst om ICT-jobgegevens op te slaan.
            List<JobData> jobDataList = new List<JobData>();

            // Loop door de eerste 5 jobs op de pagina gebaseerd op die zoekterm
            for (int i = 0; i < 6; i++)
            {
                // Sla de derde iteratie over (index 3). Dit heb ik toegevoegd omdat de 3de element op deze site was geen vacature.
                if (i == 3)
                {

                    continue;
                }

                int index = jobDataList.Count + 1;

                // Vernieuw de lijst van jobitems.
                jobItems = driver.FindElements(By.CssSelector(".search-item"));

                var jobTitle = jobItems[i].FindElement(By.CssSelector(".job-title")).Text;
                var company = jobItems[i].FindElement(By.CssSelector(".job-company")).Text;
                var location = jobItems[i].FindElement(By.CssSelector(".job-location")).Text;
                var keywords = jobItems[i].FindElement(By.CssSelector(".job-keywords")).Text;
                var link = jobItems[i].FindElement(By.ClassName("search-item-link")).GetAttribute("href");

                // Toon de details op de console.
                Console.WriteLine($"Job {index}:");
                Console.WriteLine($"Job Title: {jobTitle}");
                Console.WriteLine($"Company: {company}");
                Console.WriteLine($"Location: {location}");
                Console.WriteLine($"Keywords: {keywords}");
                Console.WriteLine($"Link: {link}");
                Console.WriteLine(" ");

                // Voeg gegevens toe aan de lijst.
                jobDataList.Add(new JobData
                {
                    JobTitle = jobTitle,
                    Company = company,
                    Location = location,
                    Keywords = keywords,
                    Link = link
                });
            }

            // Schrijf ICT-jobgegevens naar CSV- en JSON-bestanden.
            WriteToCsv(jobDataList, "ictjobs_output.csv");
            WriteToJson(jobDataList, "ictjobs_output.json");

            Console.WriteLine("ICT Jobs scraping completed. Press any key to exit.");
            Console.ReadKey();

            // Sluit de ChromeDriver na het scrapen.
            driver.Quit();
        }

        //***********************************************************************************************************************************************************//

        // Methode om gegevens van Barnes & Noble Books te schrapen.
        static void ScrapeBarnesAndNobleBooks()
        {
            Console.WriteLine("Enter the search term for Barnes & Noble Books:");
            string booksearchTerm = Console.ReadLine();
            Console.Clear();

            // Initialisatie van ChromeDriver voor het besturen van de browser.
            IWebDriver driver = new ChromeDriver();

            // Navigeer naar de Barnes & Noble Books-website.
            driver.Navigate().GoToUrl("https://www.barnesandnoble.com");

            // Zoek en klik op de 'Accept all' knop voor cookies.
            IWebElement cookiesButton = driver.FindElement(By.XPath("//*[@id=\"onetrust-accept-btn-handler\"]"));
            cookiesButton.Click();

            // Wacht 5 seconden om pagina-elementen te laden.
            Thread.Sleep(5000);

            // Zoek de zoekbalk op de Barnes & Noble Books-pagina.
            IWebElement booksSearchInput = driver.FindElement(By.XPath("//*[@id=\"rhf_header_element\"]/nav/div/div[3]/form/div/div[2]/div/input[1]"));

            // Voer de zoekterm in het zoekvak in.
            booksSearchInput.SendKeys(booksearchTerm);

            // Zoek en klik op de zoekknop.
            IWebElement searchButton = driver.FindElement(By.XPath("//*[@id=\"rhf_header_element\"]/nav/div/div[3]/form/div/span/button"));
            searchButton.Click();

            // Zoek en klik op de 'Sort' knop om resultaten te sorteren.
            IWebElement sortButton = driver.FindElement(By.XPath("//*[@id=\"sortProducts1-replacement\"]"));
            sortButton.Click();

            // Wacht 5 seconden om pagina-elementen te laden
            Thread.Sleep(5000);

            // hier wordt dan op een dropdown menu de vijfde optie gekozen, en dat is sorteren op prijs, hoog naar laag.
            IWebElement sort = driver.FindElement(By.XPath("//*[@id=\"sortProducts1-option-5\"]"));
            sort.Click();

            // naar beneden scrollen
            Actions actions = new Actions(driver);
            actions.SendKeys(Keys.PageDown).Perform();

            // Wacht 5 seconden om pagina-elementen te laden. Deze site had soms extra tijd nodig om juist te laden.
            Thread.Sleep(7000);

            // Hier heb ik ook een lijst om de gegevens op te slaan
            List<BookData> bookDataList = new List<BookData>();

            // Loop door de eerste 5 boeken op de pagina gebaseerd op die zoekterm
            for (int i = 0; i < 5 ; i++)
            {
                int index = bookDataList.Count + 1;

                // bookitems heb ik opgeslagen in de variabele bookItems en ik heb Xpath gebrukt om deze op te halen.
                string bookItems = "/html/body/main/div[2]/div[1]/div[2]/div[2]/div/div/section[2]/div/div[" + (i + 1) + "]";

                // variabelen van de gegevens dat ik nodig heb. Hier heb ik gebruikt gemaakt van alleen maar XPaths.
                var bookTitle = driver.FindElement(By.XPath(bookItems + "/div[2]/div[1]/a")).Text;
                var author = driver.FindElement(By.XPath(bookItems + "/div[2]/div[2]/a")).Text; 
                var price = driver.FindElement(By.XPath(bookItems + "/div[2]/div[4]/div/a/span[2]")).Text;  

                // De gegevens in de console printen
                Console.WriteLine($"Book {index}:");
                Console.WriteLine($"Title: {bookTitle}");
                Console.WriteLine($"Author: {author}");
                Console.WriteLine($"Price: {price}");
                Console.WriteLine(" ");

                //Gegevens toevoegen aan de lijst.
                bookDataList.Add(new BookData
                {
                    BookTitle = bookTitle,
                   Author = author,
                    Price = price
                });
            }

            // schrijf de data naar csv en json bestanden.
            WriteToCsv(bookDataList, "barnesandnoble_output.csv");
            WriteToJson(bookDataList, "barnesandnoble_output.json");

            Console.WriteLine("Barnes & Noble Books scraping completed. Press any key to exit.");
            Console.ReadKey();
            // Sluit de ChromeDriver na het scrapen.
            driver.Quit();
        }

        //***********************************************************************************************************************************************************//

        // Schrijf gegevens naar een CSV-bestand met behulp van CsvHelper
        static void WriteToCsv<T>(List<T> dataList, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = " - "
            }))
            {
                csv.WriteRecords(dataList);
            }
        }

        // Schrijf gegevens naar een JSON-bestand met behulp van System.Text.Json
        static void WriteToJson<T>(List<T> dataList, string filePath)
        {
            string json = JsonSerializer.Serialize(dataList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        //***********************************************************************************************************************************************************//

        class YoutubeData
        {
            // Definieer eigenschappen voor YouTube-gegevens
            public string VideoTitle { get; set; }
            public string VideoLink { get; set; }
            public string VideoUploader { get; set; }
            public string VideoViews { get; set; }
        }

        class JobData
        {
            // Definieer eigenschappen voor jobgegevens
            public string JobTitle { get; set; }
            public string Company { get; set; }
            public string Location { get; set; }
            public string Keywords { get; set; }
            public string Link { get; set; }
        }

        class BookData
        {
            // Definieer eigenschappen voor boekgegevens
            public string BookTitle { get; set; }
            public string Author { get; set; }
            public string Price { get; set; }
        }
    }
}
