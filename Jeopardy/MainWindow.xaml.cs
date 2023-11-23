/*******************************************************************************
Coder:		Antonio Brito
Project:	Jeopardy
FileName:	MainWindow.xaml.cs

********************************************************************************/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
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


namespace Jeopardy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, Clues> cluesdMap = new Dictionary<string, Clues>();

        Dictionary<int, Category> fileMap = new Dictionary<int, Category>();

        List<Category> categoriesFile = new List<Category>();
        List<Category> ListCatFileUpdated = new List<Category>();
        int Score = 0;
      
        Button btnPressed;

        public MainWindow()
        {
            InitializeComponent();
            StartGame();
        }//end main


        /*===============================================================/
                                     StartGame()
          ===============================================================*/
        void StartGame()
        {
            List<Category> ListCat = new List<Category>();
            List<Clues> ListClues = new List<Clues>();
            List<Category> ObjListCategories = new List<Category>();
            List<List<Category>> listCatList = new List<List<Category>>();

            if (CheckForInternetConnection())
            {
  
                string url;
                WebClient client = new WebClient();
                string jsonCategories;
               
                for (int i = 1; i <= 6; i++)
                {
                    int offsetVal = RandomNumber(1, 18418);
                    url = @"http://jservice.io//api/categories?offset=" + offsetVal.ToString();
                    client = new WebClient();
                    jsonCategories = client.DownloadString(url);
                    client.Dispose();
                    ObjListCategories = JsonConvert.DeserializeObject<List<Category>>(jsonCategories);
                    listCatList.Add(ObjListCategories);
                }//end for 


                foreach(List<Category> myCats in listCatList)
                {
                    ListCat.Add(myCats[0]);
                }

                ObjListCategories = ListCat;
                
                ObjListCategories = AddClues(ObjListCategories, client);

                //prepare for saving
                var CatCluesJson = JsonConvert.SerializeObject(ObjListCategories);
                var CatClues = JsonConvert.DeserializeObject(CatCluesJson);


                string path = AppDomain.CurrentDomain.BaseDirectory + "jeopardy.json";

                bool updateFile = false; //declare bool variable to check if File needs to be updated or not

                if (!File.Exists(path))
                {
                    StreamWriter output = new StreamWriter(path);
                    output.WriteLine(CatClues);
                    output.Close();
                    updateFile = false; //because it's the first time, it does not need to update File
                }
                else
                {
                    //load my file with categories and clues 
                    using (StreamReader fi = File.OpenText(path))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        categoriesFile = (List<Category>)serializer.Deserialize(fi, typeof(List<Category>));

                        foreach(Category cat in categoriesFile)
                        {
                            try
                            {
                                fileMap.Add((int)cat.id, cat);
                            }
                            catch
                            {

                            }
                      }                       
                    }
                }//end if file exist
               
                if (fileMap != null)
                {       
                    DateTime dateUpdatedURL, dateUpdatedFile;
                   
                    foreach(var objCat in ObjListCategories)
                    {
                        updateFile = false;
                        Category catURL = new Category(); //Declare an empty Category class file
                        catURL = objCat; //url cat
                        Category catSingleFile = new Category(); 
             
                        if (fileMap.TryGetValue(objCat.id, out catSingleFile)) //Check if category from url  exists on file
                        {
                            foreach (var catClueFile in catSingleFile.clues) //Loop through each clue associated with the Categories of the file
                            {
                                foreach(var catClueUrl in catURL.clues) //Loop through each clue associated with the Categories from the Url
                                {
                                    //Check if same clue id..
                                    if(catClueUrl.id == catClueFile.id)
                                    {
                                        //Ensure updated_at dates do not come null
                                        if (DateTime.TryParse(catClueUrl.updated_at, out dateUpdatedURL) && (DateTime.TryParse(catClueFile.updated_at, out dateUpdatedFile))) //verifica quew ambas fechas no vengan nulas
                                        {
                                            //Check if clue updated Id date is more up-to-date than the one from the file...then it requires to be updated
                                            if (dateUpdatedURL > dateUpdatedFile) 
                                            {
                                                updateFile = true; //File needs to be updated
                                                //Once any of the clues ID from the URL is more update..then replace entire category from URL into the File
                                                fileMap[catSingleFile.id] = catURL;
                                                break;
                                            }

                                        }//end if
                                    }//end if
                                    
                                }//end for

                                if (updateFile)
                                    break;
                            }//end foreach

                        }//end if
                        else
                        {
                            //Add cat to fileMap
                            fileMap.Add((int)objCat.id, objCat);
                            updateFile = true; //File needs to be modify with new information
                        }
                    } //end for 
                    if (updateFile)//If any information needs to be updated then...
                    {
                        if (categoriesFile.Count > 0)
                        {
                            categoriesFile.Clear();
                        }

                        foreach ( var fileMapKey in fileMap)
                        {
                            Category catSingleFile = new Category();

                            fileMap.TryGetValue(fileMapKey.Key, out catSingleFile);

                            categoriesFile.Add(catSingleFile);
                        }

                        path = AppDomain.CurrentDomain.BaseDirectory + "jeopardy.json";
                        //Overwrite file
                        var CatCluesString = JsonConvert.SerializeObject(categoriesFile); 
                        StreamWriter outputUpdated = new StreamWriter(path);
                        outputUpdated.WriteLine(CatCluesString);
                        outputUpdated.Close();
                    }
                    fillGame(ObjListCategories);

                }//end if fileMap != null

            }
            else
            {   
                MessageBox.Show("No Internet connection", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
            }
        }

        /*===============================================================/
                                    AddClues()
         ===============================================================*/
        List<Category> AddClues(List<Category> ObjListCat, WebClient client)
        {
            string urlClues;

            foreach (Category objCategory in ObjListCat)
            {
                urlClues = @"http://jservice.io//api/clues?category=" + (objCategory.id).ToString();    
                string jsonClues = client.DownloadString(urlClues);

                var catDates = JsonConvert.DeserializeObject<List<ExpandoObject>>(jsonClues);
                List<Clues> objListClues = JsonConvert.DeserializeObject<List<Clues>>(jsonClues);
                objCategory.clues = objListClues;
            }
            return ObjListCat;
        }

        /*===============================================================/
                                    RandomNumber()
         ===============================================================*/
        static int RandomNumber(int min, int max)
        { Random random = new Random(); return random.Next(min, max);}

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://jservice.io/"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        /*===============================================================/
                                    fillGame()
         ===============================================================*/
        void fillGame(List<Category> ObjListCategories)
        {
            int money = 100;
            bool foundMoney= false;

            cleanGame();
            List<Clues> cluesSelected = new List<Clues>();
            int count = 0;
            //add categories to the board
            foreach (Category objCategory in ObjListCategories)
            {
                    string val = String.Format("btn{0}txt", count.ToString());
                    ((TextBlock)this.FindName(val)).Text = objCategory.title;
                     count++;
            }//end foreach

            money = 100;
            int cantClues = 0;
            int btns = 6;
            do
            {
                foreach (Category objCategory in ObjListCategories)
                {
                    string val = String.Format("btn{0}", btns.ToString());

                    Clues CluesData = objCategory.clues.Find(x => x.value.Equals(money));
                    if (CluesData != null)
                    {
                        cluesdMap.Add(val, CluesData);
                        ((Button)this.FindName(val)).Content = "$"+ CluesData.value;
                        foundMoney = true;    
                    }
                    
                    btns++;

                }//end foreach

                if (foundMoney)
                {
                    cantClues++;
                    foundMoney = false;                     
                }
                else
                {
                    if (btns == 12)
                        btns = 6;
                    else if (btns == 18)
                        btns = 12;
                    else if (btns == 24)
                        btns = 18;
                    else if (btns == 30)
                        btns = 24;
                    else if (btns == 36)
                        btns = 30;
                }

                money += 100;

            } while (cantClues < 7 &&  money < 1001 && btns != 36);
        }


        /*===============================================================/
                            cleanGame()
         ===============================================================*/
        void cleanGame()
        {
            Score = 0;
            score.Text = "$" + Score.ToString();
            answer.Text = "";
            answer.IsEnabled = false;
            question.Text = "";
            correct.Text = "";
            cluesdMap.Clear();

            for (int i = 0; i < 5; i++)
            {
                string val = String.Format("btn{0}txt", i.ToString());
                ((TextBlock)this.FindName(val)).Text = "";
            }

            for (int i =6; i< 36; i++ )
            {
                string val = String.Format("btn{0}", i.ToString());
                ((Button)this.FindName(val)).Content = "";
                ((Button)this.FindName(val)).IsEnabled = true;
                ((Button)this.FindName(val)).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#3366ff"));
            }

            btnPressed = null;
        }

        /*===============================================================/
                                  onClick()
         ===============================================================*/
        private void onClick(object sender, RoutedEventArgs e)
        {

            Button button = sender as Button; //cast a button 
            if (cluesdMap.ContainsKey(button.Name))
            {
                question.Text = cluesdMap[button.Name].question;
                answer.Text = "";
                btnPressed = button;
                btnPressed.Background = Brushes.Aqua;

                answer.IsEnabled = true;
            }
        }

        /*===============================================================/
                          gameOver()
           ===============================================================*/
        bool gameOver()
        {        
            int countBtn = 0;
            for (int i = 6; i < 36; i++)
            {
                if (i == 33)
            {
                int p = 0;
                p++;
            }

                string val = String.Format("btn{0}", i.ToString());
                if (((Button)this.FindName(val)).IsEnabled == false || ((Button)this.FindName(val)).Content.Equals("") )
                    countBtn++;
            }

           if (countBtn == 30)
                return true;
            else
                return false;
        }

        /*===============================================================/
                            checkAnwer()
         ===============================================================*/
        private void checkAnwer(object sender, MouseButtonEventArgs e)
        {
            if (answer.Text.Length > 0)
            {

                if (cluesdMap[btnPressed.Name].answer.ToUpper().Contains(answer.Text.ToUpper()))
                {
                    Score += (int)cluesdMap[btnPressed.Name].value;
                    score.Text = "$" + Score.ToString();

                    MessageBox.Show("Correct !!!", "Right Answer", MessageBoxButton.OK, MessageBoxImage.Information);
                    answer.IsEnabled = false;
                }
                else
                {
                    answer.IsEnabled = false;

                    MessageBox.Show("Incorrect !!!", "Wrong Answer", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                btnPressed.IsEnabled = false;
                correct.Text = "Correct: " + cluesdMap[btnPressed.Name].answer;
                correct.Text += ".    Your Answer was:" + answer.Text;
                answer.Text = "";
            }
            else
            {
                if (question.Text.Length > 0)
                    MessageBox.Show("Please write an answer", "Empty Answer", MessageBoxButton.OK, MessageBoxImage.Information);
            }


            if (gameOver())
            {
                MessageBox.Show("Thanks for Playing", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /*===============================================================/
                            ReStart()
         ===============================================================*/
        private void ReStart(object sender, RoutedEventArgs e)
        {
            StartGame();
        }
    }

}
