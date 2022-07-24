using ExamProject.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExamProject
{
    internal class Program
    {
        static EntityContext db = new EntityContext();
        static void Main(string[] args)
        {
            int count = 1;
            int choice = 0;
            Console.WriteLine("Welcome to my App\n" +
                "Please Wait until menu pops up");
            while (choice >= 0 && choice < 8)
            {
                if (count == 1)
                {
                    ClearTable();
                    UpdateVacancies();
                    count++;
                }
                Console.WriteLine("Choose between these options:");
                PrintMenu();
                choice = Convert.ToInt32(Console.ReadLine());
                ProceedUserInput(choice);
            }
            Console.WriteLine("Thanks for using the app" +
                "Made By Titorenko Artyom");
        }

        public static void UpdateVacancies()
        {
            int category = 0;
            string content = "";
            while (category != 13)
            {
                RestClient client = new RestClient("http://vacancy.kharkov.ua/");

                RestRequest request = new RestRequest("/widgets/rssfeeds/rss/?category=" + category, Method.Get);

                request.RequestFormat = DataFormat.Xml;

                RestResponse data = client.Execute(request);
                if (data.IsSuccessful)
                {
                    content = data.Content;
                }

                XElement doc = XElement.Parse(content);
                var rss = doc.Element("channel").Elements("item");
                foreach (XElement item in rss)
                {
                    Vacancies vacancies = new Vacancies();

                    vacancies.Title = item.Element("title").Value;
                    if (vacancies.Title.Length > 70)
                    {
                        vacancies.Title = vacancies.Title.Substring(0, 69);
                    }
                    vacancies.Description = item.Element("description").Value;
                    if (vacancies.Description.Length > 100)
                    {
                        vacancies.Description = vacancies.Description.Substring(0, 99);
                    }

                    vacancies.Date = DateTime.Parse(item.Element("pubDate").Value);
                    vacancies.Author = item.Element("author").Value;
                    if (vacancies.Author.Length > 30)
                    {
                        vacancies.Author = vacancies.Author.Substring(0, 29);
                    }
                    db.Vacancies.Add(vacancies);
                }
                category++;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static int CountVacancies()
        {
            return db.Vacancies.Count();
        }

        public static int MaxSalary()
        {
            int defaultValue = 0;
            string resultFirst = Regex.Match(db.Vacancies.First().Title, @"\d+").Value;
            int.TryParse(resultFirst, out defaultValue);
            int toCompare = defaultValue;

            foreach (Vacancies vacancy in db.Vacancies)
            {
                string loopString = Regex.Match(vacancy.Title, @"\d+").Value;
                int.TryParse(loopString, out defaultValue);
                int temp = defaultValue;
                if (toCompare < temp)
                {
                    toCompare = temp;
                }
                defaultValue = 0;
            }

            return toCompare;
        }

        public static int MinSalary()
        {
            int toCompare;
            int defaultValue = int.MaxValue;
            string resultFirst = Regex.Match(db.Vacancies.First().Title, @"\d+").Value;
            int.TryParse(resultFirst, out defaultValue);
            if (defaultValue <= 100)
            {
                toCompare = int.MaxValue;
            }
            else
            {
                toCompare = defaultValue;
            }
             
            foreach (Vacancies vacancy in db.Vacancies)
            {
                string loopString = Regex.Match(vacancy.Title, @"\d+").Value;
                int.TryParse(loopString, out defaultValue);
                if (defaultValue < 10)
                {
                    defaultValue = int.MaxValue;
                    continue;
                }
                int temp = defaultValue;
                if (toCompare > temp)
                {
                    toCompare = temp;
                }
                defaultValue = int.MaxValue;
            }
           
            return toCompare;
        }

        public static List<Vacancies> GetVacancies()
        {
            List<Vacancies> vacancies = new List<Vacancies>();
            foreach (Vacancies vacancy in db.Vacancies)
            {
                vacancies.Add(vacancy);
            }
            return vacancies;
        }

        public static List<Vacancies> ShowVacsByFirstKvartal()
        {
            List<Vacancies> vacs = GetVacancies();
            List<Vacancies> vacanciesResult = new List<Vacancies>();
            foreach (Vacancies vac in vacs)
            {
                if (vac.Date?.Month == 1 || vac.Date?.Month == 2 || vac.Date?.Month == 3)
                {
                    vacanciesResult.Add(vac);
                }
            }

            return vacanciesResult;
        }
        public static List<Vacancies> ShowVacsBySecondKvartal()
        {
            List<Vacancies> vacs = GetVacancies();
            List<Vacancies> vacanciesResult = new List<Vacancies>();
            foreach (Vacancies vac in vacs)
            {
                if (vac.Date?.Month == 4 || vac.Date?.Month == 5 || vac.Date?.Month == 6)
                {
                    vacanciesResult.Add(vac);
                }
            }

            return vacanciesResult;
        }

        public static Dictionary<string, int> GetIdAndAuthors()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            List<Vacancies> vacs = GetVacancies();

            foreach (Vacancies vac in vacs)
            {
                if (result.ContainsKey(vac.Author))
                {
                    continue;
                }
                result.Add(vac.Author, vac.VacancyId);
            }

            return result;
        }

        public static Dictionary<string, List<string>> DicOfVacsByAthor()
        {
            Dictionary<string, List<string>> result = 
                new Dictionary<string, List<string>>();
            List<Vacancies> vacs = GetVacancies();

            foreach (Vacancies vac in vacs)
            {
                if (result.ContainsKey(vac.Author))
                {
                    continue;
                }
                result.Add(vac.Author, ListOfVacByAuthor(vac.Author, vacs));
            }
            return result;
        }

        public static List<string> ListOfVacByAuthor(string author, List<Vacancies> vacs)
        {
            List<string> result = new List<string>();

            foreach (Vacancies vac in vacs)
            {
                if (vac.Author == author)
                {
                    result.Add(vac.Title);
                }
            }

            return result;
        }

        public static void ClearTable()
        {
            string conString = ConfigurationManager.ConnectionStrings["ConnectionSql"].ConnectionString;
            string comand = ConfigurationManager.AppSettings["ErasingQuery"];

            using (SqlConnection connection = new SqlConnection(conString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connection;
                cmd.CommandText = comand;
                cmd.ExecuteNonQuery();
            }
        }

        public static void PrintMenu()
        {
            Console.WriteLine("1) Count Vacancies that database holds");
            Console.WriteLine("2) Show MAX salary");
            Console.WriteLine("3) Show MIN salary");
            Console.WriteLine("4) Show Vacancies by First Quarter");
            Console.WriteLine("5) Show Vacancies by Second Quarter");
            Console.WriteLine("6) Show All Authors of Vacancies");
            Console.WriteLine("7) Show All Vacancies of each Author");
            Console.WriteLine("8) Exit");
        }
        public static void ProceedUserInput(int choice)
        {
            switch (choice)
            {
                case 1:
                    Console.WriteLine("There are " + CountVacancies() + " Vacancies");
                    break;
                case 2:
                    Console.WriteLine("MAX salary = " + MaxSalary());
                    break;
                case 3:
                    Console.WriteLine("MIN salary = " + MinSalary());
                    break;
                case 4:
                    Console.WriteLine("First Quarter Vacancies:");
                    List<Vacancies> vacancies = ShowVacsByFirstKvartal();
                    foreach (Vacancies vac in vacancies)
                    {
                        Console.WriteLine(vac.ToString());
                    }
                    break;
                case 5:
                    Console.WriteLine("Second Quarter Vacancies:");
                    List<Vacancies> vacancies2 = ShowVacsBySecondKvartal();
                    foreach (Vacancies vac in vacancies2)
                    {
                        Console.WriteLine(vac.ToString());
                    }
                    break;
                case 6:
                    Console.WriteLine("All Authors of Vacancies:");
                    Dictionary<string, int> dic = GetIdAndAuthors();
                    foreach (var pair in dic)
                    {
                        Console.WriteLine(pair.Key);
                    }
                    break;
                case 7:
                    Console.WriteLine("All Vacancies of each Author:");
                    Dictionary<string, List<string>> dict = DicOfVacsByAthor();
                    foreach (var item in dict)
                    {
                        Console.WriteLine(item.Key + " ");
                        foreach (var value in item.Value)
                        {
                            Console.WriteLine(value);
                        }
                        Console.WriteLine("---------------------------");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
