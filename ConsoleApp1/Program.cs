using ConsoleApp1;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Linq;


try
{
    if (args.Length != 4)
    {
        throw new ArgumentOutOfRangeException();
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

string dataFilePath = args[0];
string outputPath = args[1];
string logFilePath = args[2];
string outputFormat = args[3];

try
{
    if (!File.Exists(dataFilePath))
    {
        throw new FileNotFoundException();
    }
    if (!File.Exists(logFilePath))
    {
        throw new FileNotFoundException();
    }
    if (!Directory.Exists(outputPath))
    {
        throw new DirectoryNotFoundException();
    }
    if (outputFormat != "json")
    {
        throw new InvalidOperationException();
    }
} catch (Exception e)
{
    log(logFilePath, e.Message);
}

using (StreamWriter sw = new StreamWriter(logFilePath, false)) // Wipe Log
{
}

List<Student> students = new List<Student>();

try
{
    using (StreamReader sr = new StreamReader(dataFilePath))
    {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            string[] tab = line.Split(',');
            if (tab.Contains(""))
            {
                log(logFilePath, $"Wiersz nie może posiadać pustych kolumn: {line}");
                continue;
            }
            if (tab.Length != 9)
            {
                log(logFilePath, $"Wiersz nie posiada odpowiedniej ilości kolumn: {line}");
                continue;
            }
            if (students.Any(s => s.fName == tab[0] && s.lName == tab[1] && s.indexNumber == tab[4]))
            {
                log(logFilePath, $"Duplikat: {line}");
                continue;
            }
            Student student = new Student()
            {

                fName = tab[0],
                lName = tab[1],
                studies = new Studies
                {
                    name = tab[2],
                    mode = tab[3]
                },
                indexNumber = tab[4],
                birthDate = tab[5],
                email = tab[6],
                mothersName = tab[7],
                fathersName = tab[8]
            };
            students.Add(student);
        }
    }


var studies = from s in students group s by s.studies.name into g
              select new { studyName = g.Key, studCount = g.Count() };

string outputFile = Path.Combine(outputPath, $"output.{outputFormat}");

using (StreamWriter sw = new StreamWriter(outputFile, false))
{
    var activeStudies = studies.Select(s => new 
    {
        name = s.studyName,
        NumberOfStudents = s.studCount 
    }).ToList();

    var uczelnia = new
    {
        createdAt = DateTime.Now.ToString("dd.MM.yyyy"),
        author = "Bartek Kamiński",
        students = students.Select(s => new
        {
            s.indexNumber,
            s.fName,
            s.lName,
            s.birthDate,
            s.email,
            s.mothersName,
            s.fathersName,
            studies = new
            {
                s.studies.name,
                s.studies.mode
            }
        }).ToList(),
        activeStudies
    };
    var json = JsonSerializer.Serialize(new { uczelnia }, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
    sw.WriteLine(json);
}
}
catch (Exception ex)
{
    log(logFilePath, ex.Message);
}

static void log(string logFilePath, string message)
{
    using (StreamWriter sw = new StreamWriter(logFilePath, true))
    {
        sw.WriteLine(message);
    }
}