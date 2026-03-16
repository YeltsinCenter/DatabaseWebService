using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;  // Для работы с JSON

namespace DatabaseWebClient
{
    public partial class Form1 : Form
    {
        // Поля класса (переменные, которые живут, пока открыта форма)
        private HttpClient _client;
        private string _baseUrl = "https://localhost:44374/";  // ⚠️ ИЗМЕНИ НА СВОЙ ПОРТ!
        private string _sessionId;

        // КОНСТРУКТОР (вызывается при создании формы)
        public Form1()
        {
            InitializeComponent();  // Этот метод создает все кнопки и поля (он в Form1.Designer.cs)

            // Настраиваем обработку SSL-сертификатов (для локальной разработки)
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        // ОБРАБОТЧИК: Кнопка "Подключиться"
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // Блокируем кнопки, пока идет запрос
                btnConnect.Enabled = false;
                lblStatus.Text = "Подключаюсь...";

                // Создаем HttpClient, если ещё не создан
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.BaseAddress = new Uri(_baseUrl);
                    _client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                }

                // Берем строку подключения из текстового поля
                var connectionString = txtConnectionString.Text;

                // Создаем содержимое POST-запроса (строка в JSON-формате)
                var content = new StringContent(
                    $"\"{connectionString}\"",  // Оборачиваем в кавычки для JSON
                    System.Text.Encoding.UTF8,
                    "application/json");

                // Отправляем POST-запрос
                var response = await _client.PostAsync("api/database/connect", content);

                // Читаем ответ
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Парсим JSON-ответ
                    var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    _sessionId = result.sessionId;

                    // Показываем результат
                    txtResult.Text = $"✅ Подключение успешно!\r\n";
                    txtResult.AppendText($"Session ID: {_sessionId}\r\n");

                    // Обновляем статус
                    lblStatus.Text = $"Подключено (Session: {_sessionId.Substring(0, 8)}...)";

                    // Активируем другие кнопки
                    btnGetVersion.Enabled = true;
                    btnDisconnect.Enabled = true;
                }
                else
                {
                    txtResult.Text = $"❌ Ошибка: {responseBody}\r\n";
                    lblStatus.Text = "Ошибка подключения";
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = $"❌ Исключение: {ex.Message}\r\n";
                lblStatus.Text = "Ошибка";
            }
            finally
            {
                // Разблокируем кнопку в любом случае
                btnConnect.Enabled = true;
            }
        }

        private async void btnConnect_Click_1(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;
                lblStatus.Text = "Подключаюсь...";

                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.BaseAddress = new Uri(_baseUrl);
                    _client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                }

                var connectionString = txtConnectionString.Text;

                var content = new StringContent(
                    $"\"{connectionString}\"",
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _client.PostAsync("api/database/connect", content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    _sessionId = result.sessionId;

                    txtResult.Text = $"✅ Подключение успешно!\r\n";
                    txtResult.AppendText($"Session ID: {_sessionId}\r\n");

                    lblStatus.Text = $"Подключено (Session: {_sessionId.Substring(0, 8)}...)";

                    btnGetVersion.Enabled = true;
                    btnDisconnect.Enabled = true;
                }
                else
                {
                    txtResult.Text = $"❌ Ошибка: {responseBody}\r\n";
                    lblStatus.Text = "Ошибка подключения";
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = $"❌ Исключение: {ex.Message}\r\n";
                lblStatus.Text = "Ошибка";
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private async void btnGetVersion_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sessionId))
            {
                txtResult.AppendText("❌ Сначала подключитесь к базе!\r\n");
                return;
            }

            try
            {
                btnGetVersion.Enabled = false;
                lblStatus.Text = "Получаю версию...";

                // GET-запрос с параметром sessionId
                var response = await _client.GetAsync($"api/database/version?sessionId={_sessionId}");
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    txtResult.AppendText($"\r\n📊 Версия SQL Server:\r\n{result.version}\r\n");
                    lblStatus.Text = "Версия получена";
                }
                else
                {
                    txtResult.AppendText($"❌ Ошибка: {responseBody}\r\n");
                    lblStatus.Text = "Ошибка получения версии";
                }
            }
            catch (Exception ex)
            {
                txtResult.AppendText($"❌ Исключение: {ex.Message}\r\n");
            }
            finally
            {
                btnGetVersion.Enabled = true;
            }
        }

        private async void btnDisconnect_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sessionId))
            {
                txtResult.AppendText("❌ Нет активного подключения\r\n");
                return;
            }

            try
            {
                btnDisconnect.Enabled = false;
                lblStatus.Text = "Отключаюсь...";

                var content = new StringContent(
                    $"\"{_sessionId}\"",
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _client.PostAsync("api/database/disconnect", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    txtResult.AppendText($"\r\n🔌 {result.message}\r\n");

                    _sessionId = null;
                    lblStatus.Text = "Отключено";

                    btnGetVersion.Enabled = false;
                    btnDisconnect.Enabled = false;
                }
                else
                {
                    txtResult.AppendText($"❌ Ошибка: {responseBody}\r\n");
                }
            }
            catch (Exception ex)
            {
                txtResult.AppendText($"❌ Исключение: {ex.Message}\r\n");
            }
            finally
            {
                btnDisconnect.Enabled = true;
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            txtConnectionString.Text = "Server=localhost;Database=master;Trusted_Connection=True;";

            txtResult.ReadOnly = true;

            btnGetVersion.Enabled = false;
            btnDisconnect.Enabled = false;
        }
    }
}