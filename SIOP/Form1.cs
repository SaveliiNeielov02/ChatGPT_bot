using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;

namespace SIOP
{
    public partial class Form1 : Form
    {
        string apiKey = null;
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string systemMessages = "";
        bool isOver = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
 
        }
        public async void Looping() 
        {
            while (!isOver)
            {
                await Task.Delay(600);
                if (!isOver)
                {
                    Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText("*");
                    }));
                }
            }
        }
        public string PreprocessInput(string userInput)
        {
            userInput = userInput.ToLower();
            if ((userInput.Contains("посоветуй") && userInput.Contains("книг"))
                || (userInput.Contains("помо") && userInput.Contains("книг"))
                || (userInput.Contains("скажи") && userInput.Contains("книг"))
                || (userInput.Contains("рекомен") && userInput.Contains("книг")))
            {
                if (userInput.Contains("жанр"))
                {
                    return "Жанр указан";
                }
                else
                {
                    return "Не указан жанр";
                }
            }
            else 
            {
                return "Не книжная тематика";
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            isOver = false;
            string formatDefinition = PreprocessInput(textBox1.Text);
            if (formatDefinition == "Не указан жанр")
            {
                richTextBox1.AppendText("Пользователь: " + textBox1.Text + '\n');
                richTextBox1.AppendText("Система: Пожалуйста, укажите жанр книги при формировании запроса!" + '\n');
                return;
            }
            string userMessage = textBox1.Text;
            richTextBox1.Text += "Пользователь: " + userMessage + '\n';
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var requestBodyObject = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new object[]
                    {
                        new { role = "system", content = systemMessages },
                        new { role = "user", content = userMessage }
                    }
                };
                Thread loopThreadFunc = new Thread(Looping);
                loopThreadFunc.Start();
                string requestBody = JsonConvert.SerializeObject(requestBodyObject);
                var response = await client.PostAsync(apiUrl, new StringContent(requestBody, Encoding.UTF8, "application/json"));
                isOver = true;
                richTextBox1.Text += '\n';
                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    JObject jsonResponse = JObject.Parse(responseBody);
                    JArray choices = (JArray)jsonResponse["choices"];
                    string message = (string)choices[0]["message"]["content"]; 
                    richTextBox1.Text += "GPT-3.5: " + message + '\n';
                    systemMessages += " " + message + " ";
                }
                else
                {
                    richTextBox1.Text += "Произошла ошибка при отправке запрса!";
                }

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
