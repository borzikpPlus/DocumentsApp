using System;
using System.Collections.Generic;
using System.Text;

interface ITextProcessor
{
    string ProcessText();
    int CountWords();
    bool ContainsWord(string word);
}

record DocumentStatistics(int WordCount, int CharacterCount, string LongestWord, int UniqueWordCount);

record DocumentSummary(string Title, string Author, string Type, int WordCount);

class Document : ITextProcessor
{
    private static char[] separators = { ' ', ',', '.', '!', '?', ';', ':', '(', ')', '"', '\t' };

    private string title;
    private string author;
    private string text;
    private DateTime createdAt;

    public string Title
    {
        get { return title; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Назва документа не може бути порожньою");
            }
            title = value;
        }
    }

    public string Author
    {
        get { return author; }
        set { author = value; }
    }

    public string Text
    {
        get { return text; }
        set { text = value; }
    }

    public DateTime CreatedAt
    {
        get { return createdAt; }
    }

    public virtual string Type => "Документ";

    public Document(string title, string author, string text)
    {
        Title = title;
        Author = author;
        Text = text;
        createdAt = DateTime.Now;
    }

    public string[] GetWords()
    {
        return text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    public string ProcessText()
    {
        return string.Join(" ", GetWords()).ToLower();
    }

    public int CountWords()
    {
        return GetWords().Length;
    }

    public bool ContainsWord(string word)
    {
        string[] words = ProcessText().Split(' ');
        return Array.IndexOf(words, word.Trim().ToLower()) != -1;
    }

    public DocumentSummary GetSummary()
    {
        return new DocumentSummary(Title, Author, Type, CountWords());
    }

    public virtual string GetInfo()
    {
        return $"{Type}: {Title}\nАвтор: {Author}\nСтворено: {CreatedAt:dd.MM.yyyy HH:mm}\nКількість слів: {CountWords()}";
    }
}

class Article : Document
{
    public string Category { get; set; }
    public int ReadTime { get; set; }

    public override string Type => "Стаття";

    public Article(string title, string author, string text, string category, int readTime) : base(title, author, text)
    {
        Category = category;
        ReadTime = readTime;
    }

    public override string GetInfo()
    {
        return $"{base.GetInfo()}\nКатегорія: {Category}\nЧас читання: {ReadTime} хв";
    }
}

class Report : Document
{
    public string Department { get; set; }
    public int PageCount { get; set; }

    public override string Type => "Звіт";

    public Report(string title, string author, string text, string department, int pageCount) : base(title, author, text)
    {
        Department = department;
        PageCount = pageCount;
    }

    public override string GetInfo()
    {
        return $"{base.GetInfo()}\nВідділ: {Department}\nКількість сторінок: {PageCount}";
    }
}

static class DocumentAnalyzer
{
    public static DocumentStatistics Analyze(Document document)
    {
        string[] words = document.GetWords();
        string longest = "";
        HashSet<string> unique = new HashSet<string>();
        foreach (string word in words)
        {
            if (word.Length > longest.Length)
            {
                longest = word;
            }
            unique.Add(word.ToLower());
        }
        return new DocumentStatistics(words.Length, document.Text.Length, longest, unique.Count);
    }
}

class ReportGenerator
{
    public string Generate(List<Document> documents)
    {
        StringBuilder sb = new StringBuilder();
        int totalWords = 0;
        int articles = 0;
        int reports = 0;

        sb.AppendLine("========== ЗАГАЛЬНИЙ ЗВІТ ==========");
        foreach (Document document in documents)
        {
            DocumentStatistics stats = DocumentAnalyzer.Analyze(document);
            sb.AppendLine(document.GetInfo());
            sb.Append("Символів: ").Append(stats.CharacterCount);
            sb.Append(", унікальних слів: ").Append(stats.UniqueWordCount);
            sb.Append(", найдовше слово: ").AppendLine(stats.LongestWord);
            sb.AppendLine("------------------------------------");

            totalWords += stats.WordCount;
            if (document is Article)
            {
                articles++;
            }
            else if (document is Report)
            {
                reports++;
            }
        }

        sb.AppendLine("ПІДСУМОК");
        sb.Append("Усього документів: ").Append(documents.Count).AppendLine();
        sb.Append("Статей: ").Append(articles).Append(", звітів: ").Append(reports).AppendLine();
        sb.Append("Загальна кількість слів: ").Append(totalWords).AppendLine();
        return sb.ToString();
    }
}

class Program
{
    static List<Document> documents = new List<Document>();

    static int ReadNumber(string prompt)
    {
        Console.Write(prompt);
        int number;
        if (!int.TryParse(Console.ReadLine(), out number) || number <= 0)
        {
            throw new FormatException("Потрібно ввести ціле число більше 0");
        }
        return number;
    }

    static void ShowMenu()
    {
        Console.WriteLine("\n===== СИСТЕМА ОБРОБКИ ДОКУМЕНТІВ =====");
        Console.WriteLine("1. Додати статтю");
        Console.WriteLine("2. Додати звіт");
        Console.WriteLine("3. Показати всі документи");
        Console.WriteLine("4. Знайти документи за словом");
        Console.WriteLine("5. Показати статистику документа");
        Console.WriteLine("6. Згенерувати загальний звіт");
        Console.WriteLine("7. Вийти");
        Console.Write("Оберіть дію: ");
    }

    static void AddArticle()
    {
        Console.Write("Введіть назву статті: ");
        string title = Console.ReadLine();
        Console.Write("Введіть автора: ");
        string author = Console.ReadLine();
        Console.Write("Введіть категорію: ");
        string category = Console.ReadLine();
        int readTime = ReadNumber("Введіть час читання: ");
        Console.WriteLine("Введіть текст:");
        string text = Console.ReadLine();

        documents.Add(new Article(title, author, text, category, readTime));
        Console.WriteLine("\nДокумент успішно створено!");
    }

    static void AddReport()
    {
        Console.Write("Введіть назву звіту: ");
        string title = Console.ReadLine();
        Console.Write("Введіть автора: ");
        string author = Console.ReadLine();
        Console.Write("Введіть відділ: ");
        string department = Console.ReadLine();
        int pageCount = ReadNumber("Введіть кількість сторінок: ");
        Console.WriteLine("Введіть текст:");
        string text = Console.ReadLine();

        documents.Add(new Report(title, author, text, department, pageCount));
        Console.WriteLine("\nДокумент успішно створено!");
    }

    static void ShowAll()
    {
        if (documents.Count == 0)
        {
            Console.WriteLine("Документів немає");
            return;
        }
        for (int i = 0; i < documents.Count; i++)
        {
            Console.WriteLine($"#{i + 1}");
            Console.WriteLine(documents[i].GetInfo());
            Console.WriteLine();
        }
    }

    static void FindByWord()
    {
        Console.Write("Введіть слово для пошуку: ");
        string word = Console.ReadLine();
        bool found = false;

        foreach (Document document in documents)
        {
            if (document.ContainsWord(word))
            {
                Console.WriteLine("\nЗнайдено документ:");
                Console.WriteLine(document.Title);
                Console.WriteLine($"Автор: {document.Author}");
                Console.WriteLine($"Кількість слів: {document.CountWords()}");
                found = true;
            }
        }

        if (!found)
        {
            Console.WriteLine("\nДокументів з таким словом не знайдено");
        }
    }

    static void ShowStatistics()
    {
        for (int i = 0; i < documents.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {documents[i].Title}");
        }
        int index = ReadNumber("Введіть номер документа: ") - 1;
        if (index >= documents.Count)
        {
            throw new ArgumentException("Документа з таким номером немає");
        }

        Document document = documents[index];
        DocumentStatistics stats = DocumentAnalyzer.Analyze(document);
        Console.WriteLine($"\nСтатистика документа \"{document.Title}\":");
        Console.WriteLine($"Кількість слів: {stats.WordCount}");
        Console.WriteLine($"Кількість символів: {stats.CharacterCount}");
        Console.WriteLine($"Найдовше слово: {stats.LongestWord}");
        Console.WriteLine($"Унікальних слів: {stats.UniqueWordCount}");
        Console.WriteLine($"Оброблений текст: {document.ProcessText()}");

        DocumentSummary summary = document.GetSummary();
        DocumentSummary copy = summary with { Title = $"{summary.Title} (копія)" };
        Console.WriteLine($"\nОригінал: {summary}");
        Console.WriteLine($"Копія з with: {copy}");
    }

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        documents.Add(new Article("Домашні тварини", "Ірина", "Кіт спить. Кіт їсть. Собака гуляє у дворі.", "Природа", 2));
        documents.Add(new Report("Звіт за жовтень", "Максим", "Відділ продажів виконав план. Продажі зросли на 15 відсотків, тому план на листопад збільшено.", "Продажі", 3));

        ReportGenerator generator = new ReportGenerator();

        while (true)
        {
            ShowMenu();
            string choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        AddArticle();
                        break;
                    case "2":
                        AddReport();
                        break;
                    case "3":
                        ShowAll();
                        break;
                    case "4":
                        FindByWord();
                        break;
                    case "5":
                        ShowStatistics();
                        break;
                    case "6":
                        Console.WriteLine(generator.Generate(documents));
                        break;
                    case "7":
                        Console.WriteLine("До побачення!");
                        return;
                    default:
                        Console.WriteLine("Невідома дія, спробуйте ще раз");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
    }
}
