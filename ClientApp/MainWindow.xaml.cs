using ClientApp.Commands;
using ModelsDLL;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace ClientApp;

public partial class MainWindow : Window
{
    public ICommand GetCommand { get; set; }
    public ICommand PutCommand { get; set; }
    public ICommand PostCommand { get; set; }

    private HttpClient _httpClient;



    public int? Value
    {
        get { return (int?)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int?), typeof(MainWindow));



    public string Key
    {
        get { return (string)GetValue(KeyProperty); }
        set { SetValue(KeyProperty, value); }
    }

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register("Key", typeof(string), typeof(MainWindow));






    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        _httpClient = new();

        GetCommand = new RelayCommand(ExecuteGetCommand);
        PostCommand = new RelayCommand(ExecutePostCommand,CanExecutePostCommand);
        PutCommand = new RelayCommand(ExecutePutCommand, CanExecutePutCommand);
    }

    private bool CanExecutePutCommand(object? obj) => Key != null && Value != null;

    private async void ExecutePutCommand(object? obj)
    {
        var keyValue = new KeyValue()
        {
            Key = Key,
            Value = Value.Value
        };

        var jsonStr = JsonSerializer.Serialize(keyValue);

        var content = new StringContent(jsonStr);

        var response = await _httpClient.PutAsync("http://localhost:27001/", content);

        if (response.StatusCode == HttpStatusCode.OK)
            MessageBox.Show("Putted Succesfully");
        else
            MessageBox.Show("Key doesn't exist");
    }

    private bool CanExecutePostCommand(object? obj) => Key != null && Value != null;

    private async void ExecutePostCommand(object? obj)
    {
        var keyValue = new KeyValue()
        {
            Key = Key,
            Value = Value.Value

        };
        var jsonStr = JsonSerializer.Serialize(keyValue);

        var content = new StringContent(jsonStr);

        var response = await _httpClient.PostAsync("http://localhost:27001/",content);

        if(response.StatusCode == HttpStatusCode.OK)
            MessageBox.Show("Posted Succesfully");
        else
            MessageBox.Show("Already Exists");

    }

    private async void ExecuteGetCommand(object? obj)
    {

        var response = await _httpClient.GetAsync($"http://localhost:27001/?key={Key}");

        var content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var keyValue = JsonSerializer.Deserialize<KeyValue>(content);
            Value = keyValue.Value;
            Key = string.Empty;
            Key += keyValue.Key;
        }
        else
            MessageBox.Show(response.StatusCode.ToString());

    }
}
