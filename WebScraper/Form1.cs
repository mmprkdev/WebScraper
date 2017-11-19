using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace WebScraper
{
    public partial class Form1 : Form
    {
        DataTable table;
        HtmlWeb web = new HtmlWeb();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            InitTable();
            int count = 0;
            label2.Text = "Collecting data...";
            label2.Visible = true;
            label3.Text = "Number of elements: 0";
            label3.Visible = true;

            int pageNumber = 0;
            var rankings = await GameRankingsFromPage(0);

            while (rankings.Count > 0)
            {
                foreach (var ranking in rankings)
                {
                    table.Rows.Add(ranking.Name, ranking.Score);
                    count++;
                    label3.Text = "Number of rows: " + count.ToString();
                }
                rankings = await GameRankingsFromPage(++pageNumber);
            }
            label2.Text = "Finished";
        }

        // NOTE: To get the XPath for an element, right click on the element on the webpage, click on
        // Inspect Element, right click on the element in the inpector window, go to copy and choose
        // the XPath option.
        // NOTE: make sure to remove tbody in tables. Replacing [n] with // will increase the number of nodes.
        private async Task<List<NameAndScore>> GameRankingsFromPage(int pageNumber)
        {
            string url = "https://www.gamerankings.com/browse.html";
            // Need to add "?page=n" to access other pages. 
            if (pageNumber != 0)
            {
                url = "https://www.gamerankings.com/browse.html?page=" + pageNumber.ToString();
            }

            var doc = await Task.Factory.StartNew(() => web.Load(url));
            var nameNodes = doc.DocumentNode.SelectNodes("/html/body/div/div[5]/div[2]/div[1]/div/div[2]/div[2]/table//tr//td//a");
            var scoreNodes = doc.DocumentNode.SelectNodes("/html/body/div/div[5]/div[2]/div[1]/div/div[2]/div[2]/table//tr//td//span/b");

            if (nameNodes == null || scoreNodes == null)
            {
                // nodes could not be found, return empty list
                return new List<NameAndScore>();
            }

            var names = nameNodes.Select(node => node.InnerText);
            var scores = scoreNodes.Select(score => score.InnerText);

            return names.Zip(scores, (name, score) => new NameAndScore() { Name = name, Score = score}).ToList();
        }

        private void InitTable()
        {
            table = new DataTable("GameRankingsDataTable");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Score", typeof(string));

            gameRankingsDataView.DataSource = table;
        }

        private void gameRankingsDataView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // xpath for steampowered top sellers
        // /html/body/div[1]/div[7]/div[3]/div[1]/div[13]/div/div[1]/div[2]/div[2]/a[1]/div[3]/div[1]

    }

    // using this to set name and score values
    public class NameAndScore
    {
        public string Name { get; set; }
        public string Score { get; set; }
    }
}
